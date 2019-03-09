using System;
using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Renderscripts;
using Android.OS;
using Android.Graphics;
using Android.Graphics.Drawables;
using Java.IO;
using System.Collections;
using System.Collections.Generic;

using Cloo;
using Microsoft.Win32;
using OpenClooVision;
using OpenClooVision.Capture;
using OpenClooVision.Imaging;
using OpenClooVision.Kernels.ViolaJones;

namespace Blurate
{



    public class DataHolder
    {
        private static String data = "";
        private static Bitmap image = null;
        public static void setData(String data) { DataHolder.data = data; }
        public static String getData() { return data; }
        public static void setImage(Bitmap inIm) { DataHolder.image = inIm; }
        public static Bitmap getImage() { return image; }
    }

    [Activity (Label = "Blurate", MainLauncher = true, Icon = "@drawable/icon")]
    public class MainActivity : Activity, Android.Hardware.Camera.IPictureCallback, Android.Hardware.Camera.IPreviewCallback, Android.Hardware.Camera.IShutterCallback, ISurfaceHolderCallback
    {
        /*[BroadcastReceiver(Enabled = true, Exported = false)]
        [IntentFilter(new[] { Intent.ActionScreenOn, Intent.ActionScreenOff })]
        public class ScreenOnOffReceiver : BroadcastReceiver
        {
            public override void OnReceive(Context context, Intent intent)
            {
                if (intent.Action.Equals(Intent.ActionScreenOn))
                {
                    //Log.i("[BroadcastReceiver]", "Screen ON");
                }
                else if (intent.Action.Equals(Intent.ActionScreenOff))
                {
                    //Log.i("[BroadcastReceiver]", "Screen OFF");
                }
            }
        }*/

        private void registerScreenStatusReceiver()
        {
            /*mScreenReceiver = new ScreenOnOffReceiver();
            IntentFilter filterS = new IntentFilter();
            filterS.AddAction(Intent.ActionScreenOn);
            filterS.AddAction(Intent.ActionScreenOff);
            RegisterReceiver(mScreenReceiver, filterS);*/
        }

        private void unregisterScreenStatusReceiver()
        {
            /*try
            {
                if (mScreenReceiver != null)
                {
                    UnregisterReceiver(mScreenReceiver);
                }
            }
            catch (Exception e) { }*/
        }

        //ScreenOnOffReceiver mScreenReceiver = null;
        Android.Hardware.Camera camera = null;
        uint crntCamId = 0;

        static public float[] decodeYUV420SP_(byte[] yuv420sp, float[] rgba, int width,
                              int height)
        {
            int frameSize = width * height;
            // define variables before loops (+ 20-30% faster algorithm o0`)
            int r, g, b, y1192, y, i, uvp, u, v;

            for (int j = 0, yp = 0; j < height; j++)
            {
                uvp = frameSize + (j >> 1) * width;
                u = 0;
                v = 0;
                for (i = 0; i < width; i++, yp++)
                {
                    y = (0xff & ((int)yuv420sp[yp])) - 16;
                    if (y < 0)
                        y = 0;
                    if ((i & 1) == 0)
                    {
                        v = (0xff & yuv420sp[uvp++]) - 128;
                        u = (0xff & yuv420sp[uvp++]) - 128;
                    }

                    y1192 = 1192 * y;
                    r = (y1192 + 1634 * v);
                    g = (y1192 - 833 * v - 400 * u);
                    b = (y1192 + 2066 * u);

                    // Java's functions are faster then 'IFs'
                    r = Math.Max(0, Math.Min(r, 262143));
                    g = Math.Max(0, Math.Min(g, 262143));
                    b = Math.Max(0, Math.Min(b, 262143));

                    // rgb[yp] = 0xff000000 | ((r << 6) & 0xff0000) | ((g >> 2) &
                    // 0xff00) | ((b >> 10) & 0xff);
                    // rgba, divide 2^10 ( >> 10)
                    rgba[yp*4+3] = ((float)(r))/255.0f*262143.0f;
                    rgba[yp*4+2] = ((float)(g)) / 255.0f * 262143.0f;
                    rgba[yp*4+1] = ((float)(b)) / 255.0f * 262143.0f;
                    rgba[yp*4+0] = (float)(255);
                }
            }
            return rgba;
        }

        static public void decodeYUV420SP(byte[] yuv420sp, float[] rgb, int width, int height)
        {
            int frameSize = width * height;
            for (int j = 0, yp = 0; j < height; j++)
            {
                int uvp = frameSize + (j >> 1) * width, u = 0, v = 0;
                for (int i = 0; i < width; i++, yp++)
                {
                    int y = (int)(0xff & (yuv420sp[yp])) - 16;
                    if (y < 0) y = 0;
                    if ((i & 1) == 0)
                    {
                        v = (0xff & yuv420sp[uvp++]) - 128;
                        u = (0xff & yuv420sp[uvp++]) - 128;
                    }

                    int y1192 = y * 1192;
                    int r = (y1192 + 1634 * v);
                    int g = (y1192 - 833 * v - 400 * u);
                    int b = (y1192 + 2066 * u);

                    //OCL code will handle these in fp32
                    /*if (r < 0) r = 0; else if (r > 262143) r = 262143;
                    if (g < 0) g = 0; else if (g > 262143) g = 262143;
                    if (b < 0) b = 0; else if (b > 262143) b = 262143;*/

                    //rgb[yp] = 0xff000000 | ((r << 6) & 0xff0000) | ((g >> 2) & 0xff00) | ((b >> 10) & 0xff);

                    rgb[yp * 4 + 0] = (256.0f * b) / 262143.0f;
                    rgb[yp * 4 + 1] = (256.0f * g) / 262143.0f;
                    rgb[yp * 4 + 2] = (256.0f * r) / 262143.0f;
                    //rgb[yp * 4 + 3] = (255.0f);
                }
            }

        }

        static public float[] flushAlpha(float[] rgb, int width, int height)
        {
            for (int j = 0, yp = 0; j < height; j++)
            {
                for (int i = 0; i < width; i++, yp++)
                {
                    rgb[yp * 4 + 3] = 255;
                }
            }
            return rgb;
        }

        private byte[] ConvertYuvToJpeg(byte[] yuvData, Android.Hardware.Camera camera)
        {
            var cameraParameters = camera.GetParameters();
            var width = cameraParameters.PreviewSize.Width;
            var height = cameraParameters.PreviewSize.Height;
            var yuv = new YuvImage(yuvData, cameraParameters.PreviewFormat, width, height, null);
            var ms = new System.IO.MemoryStream();
            var quality = 100;   // adjust this as needed
            yuv.CompressToJpeg(new Rect(0, 0, width, height), quality, ms);
            var jpegData = ms.ToArray();

            return jpegData;
        }

        public static Bitmap bytesToBitmap(byte[] imageBytes)
        {
            Bitmap bitmap = BitmapFactory.DecodeByteArray(imageBytes, 0, imageBytes.Length);

            return bitmap;
        }

        long prevRuntime = 12345;
        float[] vidData;
        byte[] lastYuvData;
        int vidWidth=0;
        int vidHight=0;
        SurfaceOrientation vidRotation = 0;
        static bool terminatCam = false;
        static bool restartedCamPend = false;
        System.ComponentModel.BackgroundWorker _capThread=null;
        void Android.Hardware.Camera.IPreviewCallback.OnPreviewFrame(byte[] data, Android.Hardware.Camera thisCamera)
        {

            if (((_capThread != null) && _capThread.IsBusy) || (camera == null))
                return;
            KeyguardManager km = (KeyguardManager)BaseContext?.GetSystemService(Context.KeyguardService);
            if (((KeyguardManager)km).InKeyguardRestrictedInputMode())
            {
                //it is locked
                System.Threading.Thread.Sleep(1000);

                restartedCamPend = false;
                camera.Unlock();
                camera.StopPreview();
                camera.SetPreviewCallback(null);
                camera.Release();
                camera = null;
                if (_cameraThread != null)
                {
                    _cameraThread.CancelAsync();
                    _cameraThread = null;
                }
                if (_capThread != null)
                {
                    _capThread.CancelAsync();
                    _capThread = null;
                }
                return; 
                //LogManager.GetLogger().i("screen", "off");
            }
            else
            {
                //if (camera==null)
                //CamSetup();
                //it is not locked
                //LogManager.GetLogger().i("screen", "on");
            }



            camera = thisCamera;
            lastYuvData = data;

            if (_capThread == null)
            {
                _capThread = new System.ComponentModel.BackgroundWorker();
                _capThread.WorkerSupportsCancellation = true;
                _capThread.DoWork += (sender, args) =>
                {
                    // do your lengthy stuff here -- this will happen in a separate thread
                    Capture();
                };
                _capThread.RunWorkerCompleted += (sender, args) =>
                {
                    //if (args.Error != null)  // if an exception occurred during DoWork,
                    //    MessageBox.Show(args.Error.ToString());  // do your error handling here
                    mImageView.SetImageDrawable(currentImage);
                };
            }
            _capThread.RunWorkerAsync();
        }

        private void Capture()
        {
            int runTime = (int)(SystemClock.ElapsedRealtime()-prevRuntime);
            //GC.Collect();
            //System.Threading.Thread.Sleep(runTime); // throttle gpu usage.

            prevRuntime = SystemClock.ElapsedRealtime();
            LogManager.GetLogger().i("CAMPROC", runTime.ToString());

            var cameraParameters = camera.GetParameters();
            bool imgResize = ((vidWidth != cameraParameters.PreviewSize.Width ||
            (vidHight != cameraParameters.PreviewSize.Height))) ||
            (vidRotation != this.WindowManager.DefaultDisplay.Rotation); // TODO: can be done more efficently

            LogManager.GetLogger().i("CAMPROC-s", (SystemClock.ElapsedRealtime() - prevRuntime).ToString());

            vidWidth = cameraParameters.PreviewSize.Width;
            vidHight = cameraParameters.PreviewSize.Height;
            vidRotation = this.WindowManager.DefaultDisplay.Rotation;
            if (imgResize)
            {
                vidData = new float[vidWidth * vidHight * 4];
                flushAlpha(vidData, vidWidth, vidHight);
            }
            decodeYUV420SP(lastYuvData, vidData, vidWidth, vidHight);

            Android.Graphics.Bitmap mBitmap = null;
            if (imgResize)
            {
                //await _TesseractApi.SetImage(data); /// this hangs                
                //string text = _Api.Text;
                //string msg = string.Format("No Input Specified");
                //Toast.MakeText(this, msg, ToastLength.Long).Show();
                byte[] jpegData = ConvertYuvToJpeg(lastYuvData, camera);
                //LogManager.GetLogger().i("CAMPROC-0", (SystemClock.ElapsedRealtime() - prevRuntime).ToString());
                mBitmap = bytesToBitmap(jpegData);

                try
                {
                    Android.Hardware.Camera.CameraInfo info =  new Android.Hardware.Camera.CameraInfo();
                    Android.Hardware.Camera.GetCameraInfo((int)crntCamId, info);
                    Android.Views.SurfaceOrientation rotation = this.WindowManager.DefaultDisplay.Rotation;
                    int degrees = 0;
                    switch (rotation)
                    {
                        case Android.Views.SurfaceOrientation.Rotation0: degrees = 0; break;
                        case Android.Views.SurfaceOrientation.Rotation90: degrees = 90; break;
                        case Android.Views.SurfaceOrientation.Rotation180: degrees = 180; break;
                        case Android.Views.SurfaceOrientation.Rotation270: degrees = 270; break;
                    }

                    int result;
                    if (info.Facing == Android.Hardware.CameraFacing.Front)
                    {
                        result = (info.Orientation + degrees) % 360;
                        result = (360 - result) % 360;  // compensate the mirror
                    }
                    else
                    {  // back-facing
                        result = (info.Orientation - degrees + 360) % 360;
                    }
                    camera.SetDisplayOrientation(result+90);

                    //int orientation = Exif.GetAttributeInt(Android.Media.ExifInterface.TagOrientation, 0);
                    
                }
                catch (IOException e)
                {
                    //e.printStackTrace();
                }
                finally
                {

                    currentOrigImage = currentImage = new BitmapDrawable(Resources, mBitmap);
                }

                mBitmap = rotateBitmap(mBitmap, 90);
                

                //LogManager.GetLogger().i("CAMPROC-1",  (SystemClock.ElapsedRealtime() - prevRuntime).ToString());
            }

            
            var imageView =
                    FindViewById<ImageView>(Resource.Id.iv_photo);

            try
            {




                //LogManager.GetLogger().i("CAMPROC-a", (SystemClock.ElapsedRealtime() - prevRuntime).ToString());


                //if (orientation != -1) 
                ApplyFilter(true);
                LogManager.GetLogger().i("CAMPROC-2", (SystemClock.ElapsedRealtime() - prevRuntime).ToString());

                //mAttacher.Update();
                //mAttacher = new PhotoViewAttacher(mImageView);
                //mAttacher.SetOnMatrixChangeListener(new MatrixChangeListener(this));
            }
            catch
            {
                //imageView.SetImageBitmap(mBitmap);
            }

            LogManager.GetLogger().i("CAMPROC-3", (SystemClock.ElapsedRealtime() - prevRuntime).ToString());

            return;
        }

        void Android.Hardware.Camera.IPictureCallback.OnPictureTaken(byte[] data, Android.Hardware.Camera camera)
        {
            FileOutputStream outStream = null;
            File dataDir = Android.OS.Environment.ExternalStorageDirectory;
            if (data != null)
            {
                try
                {
                    outStream = new FileOutputStream(dataDir + "/" + "shit.jpg");
                    outStream.Write(data);
                    outStream.Close();
                }
                catch (FileNotFoundException e)
                {
                    System.Console.Out.WriteLine(e.Message);
                }
                catch (IOException ie)
                {
                    System.Console.Out.WriteLine(ie.Message);
                }
            }
        }

        void Android.Hardware.Camera.IShutterCallback.OnShutter()
        {

        }


        public void SurfaceCreated(ISurfaceHolder holder)
        {
            try
            {
                /*camera = Android.Hardware.Camera.Open();
                Android.Hardware.Camera.Parameters p = camera.GetParameters();
                p.PictureFormat = Android.Graphics.ImageFormatType.Jpeg;
                camera.SetParameters(p);
                camera.SetPreviewCallback(this);
                camera.Lock();
                camera.SetPreviewDisplay(holder);
                camera.StartPreview();*/
            }
            catch (IOException e)
            {
            }
        }

        public void SurfaceDestroyed(ISurfaceHolder holder)
        {
            try
            {
                restartedCamPend = false;
                camera.Unlock();
                camera.StopPreview();
                camera.SetPreviewCallback(null);
                camera.Release();
                camera = null;
                _capThread = null;
                System.Threading.Thread.Sleep(500); // was crashing without this on retoating camera
                terminatCam = false;
                if (restartedCamPend)
                {
                    restartedCamPend = false;
                    startCam();
                }
                if (_cameraThread != null)
                {
                    _cameraThread.CancelAsync();
                    _cameraThread.Dispose();
                    _cameraThread = null;
                }
                if (_capThread != null)
                {
                    _capThread.CancelAsync();
                    _capThread.Dispose();
                    _capThread = null;
                }
            }
            catch {
            }
        }

        public void SurfaceChanged(ISurfaceHolder holder, Android.Graphics.Format f, int i, int j)
        {
            if (holder.Surface == null)
            {
                return;
            }

            try
            {
                camera.StopPreview();
            }
            catch (Exception e)
            {
                // ignore: tried to stop a non-existent preview
            }

            try
            {
                camera.SetPreviewCallback(this);
                camera.SetPreviewDisplay(holder);
                camera.StartPreview();
                prevRuntime = SystemClock.ElapsedRealtime();
            }
            catch (Exception e)
            {
                //Log.d(TAG, "Error starting camera preview: " + e.getMessage());
            }

        }

        static readonly String PHOTO_TAP_TOAST_STRING = "Photo Tap! X: %.2f %% Y:%.2f %% ID: %d";
		static readonly String SCALE_TOAST_STRING = "Scaled to: %.2ff";

        static long last_index = -1;

        private TextView mCurrMatrixTv;
        private TextView textBox1;

        static private PhotoViewAttacher mAttacher;

		private Toast mCurrentToast;

		private Matrix mCurrentDisplayMatrix = null;

        static ClooContext _context;
        static ClooCommandQueue _queue;
        static ClooDevice _selectedDevice = null;
        static ClooProgramViolaJones _specialkernel;

        ClooSampler _sampler;
        ClooBuffer<uint> _histogram;
        static ClooBuffer<byte> final_output;
        static byte[] final_out_host;
        static System.Runtime.InteropServices.GCHandle final_out_ptr;
        ClooImage2DFloatRgbA _clooImageByteOriginal;
        ClooImage2DFloatRgbA _clooImageByteOriginal_inflight;
        ClooImage2DByteA _clooImageByteGrayOriginal;
        ClooImage2DFloatRgbA _clooImageByteResult;
        List<ClooImage2DFloatRgbA> _clooImageByteIntermediate = new List<ClooImage2DFloatRgbA>();

        ClooImage2DFloatRgbA _clooImageByteCurrentIn;
        ClooImage2DFloatRgbA _clooImageByteCurrentOut;

        ClooImage2DByteA _clooImageByteResultA;
        ClooImage2DFloatRgbA _clooImageFloatOriginal;
        ClooImage2DFloatRgbA _clooImageFloatTemp1;
        ClooImage2DFloatRgbA _clooImageFloatTemp2;
        ClooImage2DFloatA _clooImageFloatGrayOriginal;
        ClooImage2DFloatA _clooImageFloatATemp1;
        ClooImage2DFloatA _clooImageFloatATemp2;
        ClooImage2DFloatA _clooImageFloatIntegral;
        ClooImage2DUIntA _clooImageUIntIntegral;
        ClooImage2DUIntA _clooImageUIntIntegralSquare;
        ClooHaarObjectDetector _haarObjectDetector = null;

        static List<Bitmap> ImageStack = new List<Bitmap>();
        static int ImageStackPos = 0;

        static Drawable currentImage=null;
        static Bitmap curImg;
        static Java.Nio.ByteBuffer curByteBuffer;
        static BitmapDrawable _bitmapImage1;
        static List<string> KernStrngs = new List<string>();
        static List<string> VisibDeviceNameCombobox = new List<string>();
        static List<int> kernelInputBuffers = new List<int>();
        static int[] secondaryInputBuffers = null;
        static string[] secondaryInputFunc = null;
        // nesting : [] -> () -> {} -> <>
        static int numKernels = 0;

        private string GetPathToImage(Android.Net.Uri uri)
        {
            string doc_id = "";
            using (var c1 = ContentResolver.Query(uri, null, null, null, null))
            {
                c1.MoveToFirst();
                String document_id = c1.GetString(0);
                doc_id = document_id.Substring(document_id.LastIndexOf(":") + 1);
            }

            string path = null;

            // The projection contains the columns we want to return in our query.
            string selection = Android.Provider.MediaStore.Images.Media.InterfaceConsts.Id + " =? ";
            using (var cursor = ManagedQuery(Android.Provider.MediaStore.Images.Media.ExternalContentUri, null, selection, new string[] { doc_id }, null))
            {
                if (cursor == null) return path;
                var columnIndex = cursor.GetColumnIndexOrThrow(Android.Provider.MediaStore.Images.Media.InterfaceConsts.Data);
                cursor.MoveToFirst();
                path = cursor.GetString(columnIndex);
            }
            return path;
        }

        public static Bitmap rotateBitmap(Bitmap bitmap, int orientation)
        {

            Matrix matrix = new Matrix();
            switch (orientation)
            {
                case (int) Android.Media.Orientation.Normal:
                    return bitmap;
                case (int)Android.Media.Orientation.FlipHorizontal:
                    matrix.SetScale(-1, 1);
                    break;
                case (int)Android.Media.Orientation.Rotate180:
                    matrix.SetRotate(180);
                    break;
                case (int)Android.Media.Orientation.FlipVertical:
                    matrix.SetRotate(180);
                    matrix.PostScale(-1, 1);
                    break;
                case (int)Android.Media.Orientation.Transpose:
                    matrix.SetRotate(90);
                    matrix.PostScale(-1, 1);
                    break;
                case (int)Android.Media.Orientation.Rotate90:
                    matrix.SetRotate(90);
                    break;
                case (int)Android.Media.Orientation.Transverse:
                    matrix.SetRotate(-90);
                    matrix.PostScale(-1, 1);
                    break;
                case (int)Android.Media.Orientation.Rotate270:
                    matrix.SetRotate(-90);
                    break;
                default:
                    return bitmap;
            }
            try
            {
                Bitmap bmRotated = Bitmap.CreateBitmap(bitmap, 0, 0, bitmap.Width, bitmap.Height, matrix, true);
                bitmap.Recycle();
                return bmRotated;
            }
            catch (Exception e)
            {
                //e.printStackTrace();
                return null;
            }
        }

        static Drawable currentOrigImage = null;
        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);

            if (resultCode == Result.Ok)
            {
                var imageView =
                    FindViewById<ImageView>(Resource.Id.iv_photo);

                try
                {
                    Android.Graphics.Bitmap mBitmap = null;
                    mBitmap = Android.Provider.MediaStore.Images.Media.GetBitmap(this.ContentResolver, data.Data);

                    Android.Media.ExifInterface exif = null;
                    try
                    {
                        exif = new Android.Media.ExifInterface(GetPathToImage(data.Data));
                        int orientation = exif.GetAttributeInt(Android.Media.ExifInterface.TagOrientation, 0);
                        mBitmap = rotateBitmap(mBitmap, orientation);
                    }
                    catch (IOException e)
                    {
                        //e.printStackTrace();
                    }
                    finally
                    {
                        currentOrigImage = currentImage = new BitmapDrawable(Resources, mBitmap);
                    }

                    //if (orientation != -1) 
                        ApplyFilter();

                    mAttacher.Update();
                    //mAttacher = new PhotoViewAttacher(mImageView);
                    //mAttacher.SetOnMatrixChangeListener(new MatrixChangeListener(this));
                }
                catch
                {
                    imageView.SetImageURI(data.Data);
                }
            }
        }


        public void onConfigurationChanged(Android.Content.Res.Configuration newConfig)
        {
            //Log.e("MotherActivity", "onConfigurationChanged called ");
            // this method is there to ensure no configuration changes when going to sleep mode
            // because the device wants my app to go on portrait and then fire screenSize changes
            // sometime some montainview code sucks
            base.OnConfigurationChanged(newConfig);
            // and of course do nothing !!!
        }

        protected override void OnResume( )
        {
            base.OnResume();

            if (true) // camera enabled
            {
                startCam();
            }

            string CurrentKern = DataHolder.getData();
            if ((CurrentKern!=null) && (CurrentKern.Length != 0))
            {
                //for (int i = 0; i < mainKrnls.Length; i++)
                try
                {
                    if ((CurrentKern.Length > 0) && (CurrentKern.Contains("=")))
                    {

                        string newFilter = CurrentKern.Split('=')[0].Trim();


                        if (filt_list.Contains(newFilter))
                        {
                            int existCntr = 1;
                            while (filt_list.Contains(newFilter + "." + existCntr.ToString())) existCntr++;
                            newFilter = newFilter + "." + existCntr.ToString();
                        }

                        try
                        {
                            KernelsList.Add(newFilter, CurrentKern.Split('=')[1]);
                        }
                        catch { DataHolder.setData(""); return; }
                        //toolStripComboBox1.Items.Add(CurrentKern.Split('=')[0].Trim());
                        //spinner.items
                        filt_list.Add(newFilter);

                        var adapter = new ArrayAdapter(this,
                                Android.Resource.Layout.SimpleSpinnerItem, filt_list.ToArray());

                        adapter.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);
                        Spinner spinner = FindViewById<Spinner>(Resource.Id.spinner1);
                        spinner.Adapter = adapter;
                        spinner.SetSelection(filt_list.Count - 1);

                        string toast = string.Format("Filter {0} imported", newFilter);
                        Toast.MakeText(this, toast, ToastLength.Long).Show();

                    }
                }
                finally {
                    System.IO.File.AppendAllText(Android.App.Application.Context.FilesDir.AbsolutePath + @"/main.krnls", CurrentKern+ "\n");
                    DataHolder.setData("");
                    
                }
            }
            else
            {
                Bitmap CurrentBmp = DataHolder.getImage();
                if (CurrentBmp != null)
                {
                    currentOrigImage = currentImage = new BitmapDrawable(Resources, CurrentBmp);
                    ApplyFilter();
                    DataHolder.setImage(null);
                }
            }
        }

        /*protected override void OpenFileAuto( Bundle bundle) // this fucker gets called on fucking rotates
        {

        }*/

        System.ComponentModel.BackgroundWorker _cameraThread = null;
        public void startCam()
        {
            if (_cameraThread != null)
            {
                if (_cameraThread.IsBusy)
                    return; // bullshit OnRestore

                _cameraThread.Dispose();
                _cameraThread = null;
            }

            _cameraThread = new System.ComponentModel.BackgroundWorker();
            _cameraThread.WorkerSupportsCancellation = true;

            _cameraThread.DoWork += (sender, args) =>
            {
                    // do your lengthy stuff here -- this will happen in a separate thread
                    CamSetup();
            };
            _cameraThread.RunWorkerCompleted += (sender, args) =>
            {

            };
            _cameraThread.RunWorkerAsync();
        }

        public void CamSetup()
        { 
            if (terminatCam)
            {
                // if camera termination fired but not done yet, kill it.
                //terminatCam = false;
                //Wait(100);
                restartedCamPend = true;
                //System.Threading.Thread.Sleep(500);
            }
            terminatCam = true;
            crntCamId = 0;
            while (true)
            {
                try
                {
                    if (crntCamId > Android.Hardware.Camera.NumberOfCameras) return;
                    camera = Android.Hardware.Camera.Open((int)crntCamId);
                    SurfaceView surface = (SurfaceView)FindViewById(Resource.Id.iv_surf);
                    //surface.Visibility = ViewStates.Invisible;
                    var holder = surface.Holder;
                    holder.AddCallback(this);
                    holder.SetType(Android.Views.SurfaceType.PushBuffers);
                    Android.Hardware.Camera.Parameters p = camera.GetParameters();


                    try
                    {
                        Android.Hardware.Camera.CameraInfo info = new Android.Hardware.Camera.CameraInfo();
                        Android.Hardware.Camera.GetCameraInfo((int)crntCamId, info);
                        Android.Views.SurfaceOrientation rotation = this.WindowManager.DefaultDisplay.Rotation;
                        int degrees = 0;
                        switch (rotation)
                        {
                            case Android.Views.SurfaceOrientation.Rotation0: degrees = 0; break;
                            case Android.Views.SurfaceOrientation.Rotation90: degrees = 90; break;
                            case Android.Views.SurfaceOrientation.Rotation180: degrees = 180; break;
                            case Android.Views.SurfaceOrientation.Rotation270: degrees = 270; break;
                        }

                        int result;
                        if (info.Facing == Android.Hardware.CameraFacing.Front)
                        {
                            result = (info.Orientation + degrees) % 360;
                            result = (360 - result) % 360;  // compensate the mirror
                        }
                        else
                        {  // back-facing
                            result = (info.Orientation - degrees + 360) % 360;
                        }
                        camera.SetDisplayOrientation(result + 90);


                        p.PictureFormat = Android.Graphics.ImageFormatType.Jpeg;
                        camera.SetParameters(p);
                        camera.SetPreviewCallback(this);
                        camera.Lock();
                        camera.SetPreviewDisplay(holder);

                        //int orientation = Exif.GetAttributeInt(Android.Media.ExifInterface.TagOrientation, 0);

                    }
                    catch (IOException e)
                    {
                        //e.printStackTrace();
                    }

                    camera.StartPreview();
                    this.Window.AddFlags(WindowManagerFlags.KeepScreenOn);
                    break;
                }
                catch (Exception ex)
                {
                    crntCamId++;
                    LogManager.GetLogger().i("cam connect error:", ex.Message.ToString());
                    System.Threading.Thread.Sleep(500);
                    //string msg = string.Format("error switching camera on (" + ex.Message + ")");
                    //Toast.MakeText(this, msg, ToastLength.Long).Show();
                    //Wait(5);
                }
            }
        }


        [Android.Runtime.Register("onHiddenChanged", "(Z)V", "GetOnHiddenChanged_ZHandler")]
        public virtual void OnHiddenChanged(Boolean hidden)
        {

        }

        static PhotoView mImageView = null;
        static public Dictionary<string, string> KernelsList = new Dictionary<string, string>();
        static string[] mainKrnls = { };
        static List<string> filt_list = new List<string>();
        static bool init_setup = true;
        protected override void OnCreate (Bundle bundle) // this fucker gets called on fucking rotates
		{
            base.OnCreate (bundle);
            //Intent intentS = new Intent(this, typeof(ScreenOnOffReceiver)); // <- this screen on/off shit never worked
            //StartService(intentS);
            registerScreenStatusReceiver();

        /*SurfaceView surface = (SurfaceView)FindViewById(Resource.Id.iv_photo);
        var holder = surface.Holder;
        holder.AddCallback(this);
        holder.SetType(Android.Views.SurfaceType.PushBuffers);*/

        /*if (!init_setup)
        {
            if (last_index == -1)
                currentOrigImage = currentImage = Resources.GetDrawable(Resource.Drawable.Milkmaid);
            mImageView.SetImageDrawable(currentImage);

            Spinner spinner1 = FindViewById<Spinner>(Resource.Id.spinner1);
            //spinner1.ItemSelected += new EventHandler<AdapterView.ItemSelectedEventArgs>(spinner_ItemSelected);

            var adapter1 = new ArrayAdapter(this,
            Android.Resource.Layout.SimpleSpinnerItem, filt_list.ToArray());

            adapter1.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);
            spinner1.Adapter = adapter1;

            return;
        }*/

        // Set our view from the "main" layout resource
        SetContentView(Resource.Layout.Main);
                mImageView = FindViewById<PhotoView>(Resource.Id.iv_photo);
                mCurrMatrixTv = FindViewById<TextView>(Resource.Id.tv_current_matrix);
                textBox1 = FindViewById<TextView>(Resource.Id.current_kernel);
            

            //GC.Collect();
            if (last_index==-1)
                currentOrigImage = currentImage = Resources.GetDrawable(Resource.Drawable.Milkmaid);

            mImageView.SetImageDrawable(currentImage);

            if (mainKrnls.Length == 0)
            {
                if (!System.IO.File.Exists(Android.App.Application.Context.FilesDir.AbsolutePath + @"/main.krnls"))
                {
                    mainKrnls = Resources.GetString(Resource.String.main_krnls).Split('\n');
                    //Toast.MakeText(this, msg, ToastLength.Long).Show();

                    System.IO.File.WriteAllLines(Android.App.Application.Context.FilesDir.AbsolutePath + @"/main.krnls", mainKrnls);
                }
                else
                {
                    mainKrnls = System.IO.File.ReadAllLines(Android.App.Application.Context.FilesDir.AbsolutePath + @"/main.krnls");
                }
            }

            Spinner spinner = FindViewById<Spinner>(Resource.Id.spinner1);
            spinner.ItemSelected += new EventHandler<AdapterView.ItemSelectedEventArgs>(spinner_ItemSelected);
            //var adapter = ArrayAdapter.CreateFromResource(
            //        this, Resource.Array.country_arrays, Android.Resource.Layout.SimpleSpinnerItem);

            if (init_setup)
            {
                for (int i = 0; i < mainKrnls.Length; i++)
                {
                    string CurrentKern = mainKrnls[i].Split('#')[0].Trim();
                    if ((CurrentKern.Length > 0) && (CurrentKern.Contains("=")))
                    {
                        try
                        {
                            KernelsList.Add(CurrentKern.Split('=')[0].Trim(), CurrentKern.Split('=')[1]);
                        }
                        catch { continue; }
                        //toolStripComboBox1.Items.Add(CurrentKern.Split('=')[0].Trim());
                        //spinner.items
                        string name_new = CurrentKern.Split('=')[0].Trim();
                        filt_list.Add(name_new);
                    }
                }
            }
            var adapter = new ArrayAdapter(this,
                Android.Resource.Layout.SimpleSpinnerItem, filt_list.ToArray());

            adapter.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);
            spinner.Adapter = adapter;

            // The MAGIC happens here!
            mAttacher = new PhotoViewAttacher(mImageView);
            mAttacher.SetZoomTransitionDuration(0);

            // Lets attach some listeners, not required though!
            mAttacher.SetOnMatrixChangeListener(new MatrixChangeListener(this));
			//mAttacher.SetOnPhotoTapListener(new PhotoTapListener(this));


            ImageButton searchButton = FindViewById<ImageButton>(Resource.Id.myButton);

            /*ViewGroup.LayoutParams param = button.LayoutParameters;
            //Button new width
            param.Width = param.Height;
            button.LayoutParameters = param;
            button.ForceLayout();*/

            //button.SetBackgroundDrawable(Resource.Drawable.)
            searchButton.Click += delegate {
                var imageIntent = new Intent();
                imageIntent.SetType("image/*");
                imageIntent.SetAction(Intent.ActionGetContent);
                StartActivityForResult(
                Intent.CreateChooser(imageIntent, "Select photo"), 0);
            };

            searchButton.LayoutChange += delegate
            {
                ViewGroup.LayoutParams lparams = searchButton.LayoutParameters;
                lparams.Width = searchButton.Height;
                searchButton.LayoutParameters = lparams;
            };

            ImageButton sharebutton = FindViewById<ImageButton>(Resource.Id.myShareButton);

            //sharebutton.s = spinner.Height;
            //sharebutton.SetMinimumHeight(spinner.Height*2);
            //sharebutton.SetMinimumWidth(spinner.Height*2);

            //sharebutton.LayoutParameters = new RelativeLayout.LayoutParams(sharebutton.Height, sharebutton.Height);



            sharebutton.LayoutChange += delegate
            {
                ViewGroup.LayoutParams lparams = sharebutton.LayoutParameters;
                lparams.Width = sharebutton.Height;
                sharebutton.LayoutParameters = lparams;
            };

            sharebutton.Click += delegate {
                /*String message = "Text I want to share.";
                Intent share = new Intent(Intent.ActionSend);
                share.SetType("image/jpeg");
                share.PutExtra(Intent.ExtraText, message);
                //share.PutExtra(Intent.ima, message);
                StartActivity(Intent.CreateChooser(share, "Title of the dialog the system will open"));*/

                try
                {
                    File file = new File(ExternalCacheDir , "blurateShare.jpg");
                    using (var fOut = new System.IO.FileStream(file.Path, System.IO.FileMode.Create))
                    {
                        try
                        {
                            Bitmap currntBmp = drawableToBitmap(currentImage);
                            currntBmp.Compress(Bitmap.CompressFormat.Jpeg, 95, fOut);
                        }
                        finally
                        {
                            fOut.Flush();
                            fOut.Close();
                            file.SetReadable(true, false);
                        }
                    }

                    Intent intent = new Intent(Android.Content.Intent.ActionSend);
                    intent.SetFlags(ActivityFlags.NewTask);
                    intent.AddFlags(ActivityFlags.GrantReadUriPermission);
                    intent.PutExtra(Intent.ExtraSubject, "");
                    intent.PutExtra(Intent.ExtraText, "Check out my Blurate Filter; " + spinner.SelectedItem.ToString());
                    intent.PutExtra(Intent.ExtraStream, Android.Net.Uri.FromFile(file));
                    //string atribs = System.IO.File.GetAttributes(fileName).ToString();
                    //using (var fd = ContentResolver.OpenFileDescriptor(Android.Net.Uri.Parse(fileName), "r"))
                    //    atribs = fd.StatSize.ToString();

                    //if (atribs.Equals(""))
                    intent.SetType("image/jpeg");
                    StartActivityForResult(intent, 0);

                }
                catch (Exception e)
                {
                    //e.printStackTrace();
                }

            };

            if (init_setup)
            foreach (OpenClooVision.ClooDevice device in OpenClooVision.ClooDevice.CompatibleDevices)//.ToList()/*Where(x => x.Available).*/OrderByDescending(x => x.MaxComputeUnits))
            {
                //comboBoxDevices.Items.Add(device);
                string NewDeviceVisibName = device.ToString();
                //MessageBox.Show(device.Vendor.ToLower());

                //Deal with AMD bullshit here
               /* if (device.Vendor.ToLower().Contains("amd") ||
                    device.Vendor.ToLower().Contains("advanced micro"))
                {
                    try
                    {
                        int divCntr = 0;
                        int thisAMDcntr = 0;
                        for (int i = 0; i < 10; i++)
                        {
                            string gfx_path = "SYSTEM\\ControlSet001\\Control\\Class\\{4D36E968-E325-11CE-BFC1-08002BE10318}\\000";
                            using (RegistryKey key = Registry.LocalMachine.OpenSubKey(gfx_path + divCntr.ToString()))
                            {
                                if (key != null)
                                {
                                    Object v = key.GetValue("ProviderName");
                                    divCntr++;
                                    if ((v != null) && (v.ToString().ToLower().Contains("amd") || v.ToString().ToLower().Contains("advanced micro")))
                                    {
                                        thisAMDcntr++;
                                        //MessageBox.Show(AMDcntr.ToString());
                                        if (AMDcntr > thisAMDcntr) continue;
                                        AMDcntr++;
                                        Object o = key.GetValue("AdapterDesc");
                                        if (o != null)
                                        {
                                            NewDeviceVisibName = o.ToString();
                                            break;
                                        }
                                    }
                                }
                                else break;

                            }
                        }
                    }
                    catch (Exception ex)  //just for demonstration...it's always best to handle specific exceptions
                    {
                        //react appropriately
                    }
                }*/

                NewDeviceVisibName = NewDeviceVisibName.Replace("Intel", "");
                NewDeviceVisibName = NewDeviceVisibName.Replace("(TM)", "");
                NewDeviceVisibName = NewDeviceVisibName.Replace("(R)", "");
                NewDeviceVisibName = NewDeviceVisibName.Replace("AMD", "");
                NewDeviceVisibName = NewDeviceVisibName.Replace("Adavanced Micro Devices", "");
                NewDeviceVisibName = NewDeviceVisibName.Replace("NVidia", "");
                if (NewDeviceVisibName.Contains("Core"))
                {
                    if (NewDeviceVisibName.Contains("CPU"))
                        NewDeviceVisibName = NewDeviceVisibName.Replace("CPU", "");
                    if (NewDeviceVisibName.Contains("@"))
                        NewDeviceVisibName = NewDeviceVisibName.Substring(0, NewDeviceVisibName.LastIndexOf('@'));
                }
                NewDeviceVisibName = NewDeviceVisibName.Replace("Series", "");
                NewDeviceVisibName = NewDeviceVisibName.Trim();

                NewDeviceVisibName = System.Text.RegularExpressions.Regex.Replace(NewDeviceVisibName, @"\s+", " ");

                if (VisibDeviceNameCombobox.Contains(NewDeviceVisibName))
                    NewDeviceVisibName += " (2)";

                VisibDeviceNameCombobox.Add(NewDeviceVisibName);

                if ((_selectedDevice==null) || device.Type != ComputeDeviceTypes.Cpu)
                    _selectedDevice = device;
            }
            if (VisibDeviceNameCombobox.Count >= 1)
            {
                int defaultDevice = 0;
                /*try
                {
                    RegistryKey hklm = Registry.CurrentUser;
                    hklm = hklm.OpenSubKey("SOFTWARE\\BLURATE");
                    Size tSize = this.Size;
                    defaultDevice = int.Parse(hklm.GetValue("defaultdevice").ToString());
                }
                catch
                {
                }*/
                //VisibDeviceNameCombobox.SelectedIndex = defaultDevice;
            }
            else
            {
                //VendorIcon.Visible = false;
                //MessageBox.Show("No compute devices found!", "Fatal Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            //LoadKernels();
            //LoadImage();

            if (_selectedDevice != null)
            if (init_setup )
            {
                // create context
                _context = _selectedDevice.CreateContext();
                _queue = _context.CreateCommandQueue();
            }

            mCurrMatrixTv.Visibility = Android.Views.ViewStates.Gone;
            textBox1.Visibility = Android.Views.ViewStates.Gone;

            init_setup = false;
        }

        [Android.Runtime.Register("onPause", "()V", "GetOnPauseHandler")]
        protected override void OnPause()
        {
            LogManager.GetLogger().i("PASED!", (SystemClock.ElapsedRealtime() - prevRuntime).ToString());
            System.Threading.Thread.Sleep(500);
            camera.StopPreview();
            base.OnPause();
        }

        [Android.Runtime.Register("onStop", "()V", "GetOnStopHandler")]
        protected override void OnStop()
        {
            base.OnStop();
        }

        protected override void OnRestart()
        {
            base.OnRestart();
        }

        public override void OnWindowFocusChanged(bool hasFocus)
        {
            base.OnWindowFocusChanged(hasFocus);

        }

        public override void OnWindowAttributesChanged(WindowManagerLayoutParams @params)
        {
            base.OnWindowAttributesChanged(@params);
        }

        public override void OnDetachedFromWindow()
        {
            base.OnDetachedFromWindow();
        }
        public override void OnAttachedToWindow()
        {
            base.OnAttachedToWindow();
        }

        public static bool wasScreenOn = true;
        void onReceive(Context context, Intent intent)
        {
            if (intent.Action.Equals(Intent.ActionScreenOff))
            {
                // do whatever you need to do here
                wasScreenOn = false;
            }
            else if (intent.Action.Equals(Intent.ActionScreenOn))
            {
                // and do whatever you need to do here
                wasScreenOn = true;
            }
        }

        public static Bitmap drawableToBitmap(Drawable drawable)
        {
            Bitmap bitmap = null;

            if (drawable is BitmapDrawable) {
                BitmapDrawable bitmapDrawable = (BitmapDrawable)drawable;
                if (bitmapDrawable.Bitmap != null)
                {
                    return bitmapDrawable.Bitmap;
                }
            }

            if (drawable.IntrinsicWidth <= 0 || drawable.IntrinsicHeight <= 0)
            {
                bitmap = Bitmap.CreateBitmap(1, 1, Bitmap.Config.Argb8888); // Single color bitmap will be created of 1x1 pixel
            }
            else
            {
                bitmap = Bitmap.CreateBitmap(drawable.IntrinsicWidth, drawable.IntrinsicHeight, Bitmap.Config.Argb8888);
            }

            Canvas canvas = new Canvas(bitmap);
            drawable.SetBounds(0, 0, canvas.Width, canvas.Height);
            drawable.Draw(canvas);
            return bitmap;
        }

        private void spinner_ItemSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            Spinner spinner = (Spinner)sender;
            //textBox1.Text = "[convolv:({0,1,-2}{1,1,-1}{-1,1,-1}{0,-1,2}{1,-1,1}{-1,-1,1})()()]  [convolv:{,0|^+}({1,0,2}{1,1,1}{1,-1,1}{-1,0,-2}{-1,1,-1}{-1,-1,-1})()()]";
            textBox1.Text = KernelsList[spinner.SelectedItem.ToString()];

            if (last_index != spinner.SelectedItemId)
            {
                KernStrngs.Clear();// make sure rebuild is triggered

                if (_capThread != null) return;// if camera is active let it do the rest

                Matrix Cmatrix = mAttacher.GetDisplayMatrix();
                
                float Cscale = mAttacher.GetScale();
                //ViewGroup.LayoutParams shit = mImageView.LayoutParameters;

                RectF CRect = mAttacher.GetDisplayRect();
                
                currentImage = currentOrigImage;
                ApplyFilter();

                //mAttacher.SetDisplayMatrix(Cmatrix);
                
                mAttacher.SetImageViewMatrix(Cmatrix);
                mAttacher.SetScale(Cscale, CRect.CenterX(), CRect.CenterY(), true);

                string toast = string.Format("Filter {0} applied", spinner.GetItemAtPosition(e.Position));
                if (_selectedDevice != null)
                    Toast.MakeText(this, toast, ToastLength.Long).Show();

                //mAttacher.SetScale(Cscale);
                //mAttacher.SetDisplayMatrix(Cmatrix);
                //mImageView.LayoutParameters = shit;



                //RectF rect = GetDisplayRect();
                /*if (null != CRect)
                {
                    v.Post(new AnimatedZoomRunnable(this, GetScale(), mMinScale,
                        rect.CenterX(), rect.CenterY()));
                    handled = true;
                }*/

                /*
                long downTime = SystemClock.UptimeMillis();
                long eventTime = SystemClock.UptimeMillis() + 1;
                // List of meta states found here:     developer.android.com/reference/android/view/KeyEvent.html#getMetaState()
                int metaState = 0;
                MotionEvent motionEvent = MotionEvent.Obtain(
                    downTime,
                    eventTime,
                    (int)MotionEventActions.Move,
                    CRect.CenterX(),
                    CRect.CenterY(),
                    (MetaKeyStates)metaState
                );
                mImageView.DispatchTouchEvent(motionEvent);*/

                // Obtain MotionEvent object
                /*long downTime = SystemClock.UptimeMillis();
                long eventTime = SystemClock.UptimeMillis() + 100;
                float x = 0.0f;
                float y = 0.0f;
                // List of meta states found here:     developer.android.com/reference/android/view/KeyEvent.html#getMetaState()
                int metaState = 0;
                MotionEvent motionEvent = MotionEvent.Obtain(
                    downTime,
                    eventTime,
                    (int) MotionEventActions.Move,
                    x,
                    y,
                    (MetaKeyStates) metaState
                );

                // Dispatch touch event to view
                mImageView.DispatchTouchEvent(motionEvent);*/

                //mImageView.CheckAndDisplayMatrix();


            }
            last_index = spinner.SelectedItemId;
        }

        private void ApplyFilter(bool useVid=false)
        { 
            //try
            {
                if (currentImage == null)
                {
                    string msg = string.Format("No Input Specified");
                    Toast.MakeText(this, msg, ToastLength.Long).Show();
                    return;
                }
                if (_selectedDevice == null)
                {
                    string msg = string.Format("No OpenCL support detected");
                    if (System.IO.File.Exists("/system/lib64/egl/libGLES_mali.so"))
                        msg = string.Format("OpenCL runtime found, but no support detected");
                    Toast.MakeText(this, msg, ToastLength.Long).Show();
                    return;
                }

                bool reestablish = false;
                bool sizeChanged = false;

                //String KernelString = "";
                if (KernStrngs.Count == 0)
                {
                    reestablish = true;
                    string fullFormula = textBox1.Text;
                    string[] kernelFormulas = fullFormula.Split(new[] { ']' }, StringSplitOptions.RemoveEmptyEntries);
                    kernelInputBuffers.Clear(); // updated in decodeKernels()
                    secondaryInputBuffers = new int[kernelFormulas.Length];
                    secondaryInputFunc = new string[kernelFormulas.Length];
                    // nesting : [] -> () -> {} -> <>
                    numKernels = 0;
                    decodeKernels(kernelFormulas); //sets KernStrngs
                    if (_specialkernel != null) _specialkernel.Dispose();
                    if (_selectedDevice != null)
                    {
                        //_sampler = new ClooSampler(_context, false, ComputeImageAddressing.ClampToEdge, ComputeImageFiltering.Linear);
                        //_kernels = ClooProgramViolaJones.Create(_context);

                        try
                        {
                            _specialkernel = ClooProgramViolaJones.CreateSpecial(_context, KernStrngs);
                        }
                        catch (Exception ex)
                        {
                            //MessageBox.Show(ex.Message, "Build Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            //return;
                            string msg = string.Format("A build error occurred");
                            Toast.MakeText(this, msg, ToastLength.Long).Show();
                            return;
                        }
                    }
                }

                //try
                {

                    _bitmapImage1 = (BitmapDrawable)currentImage;

                    //_selectedDevice = comboBoxDevices.SelectedItem as ClooDevice;
                    if (_context != null)
                    {
                        //_startProcessing = false;
                    }
                    if (_selectedDevice != null)
                    {
                        //_sampler = new ClooSampler(_context, false, ComputeImageAddressing.ClampToEdge, ComputeImageFiltering.Linear);
                        //_kernels = ClooProgramViolaJones.Create(_context);

                        /*_haarObjectDetector = ClooHaarObjectDetector.CreateFaceDetector(_context, _queue, 640, 480);
                        _haarObjectDetector.ScalingFactor = 1.25f;
                        _haarObjectDetector.ScalingMode = ScalingMode.SmallerToLarger;
                        _haarObjectDetector.MinSize = new System.Drawing.Size(30, 30);
                        _haarObjectDetector.MaxSize = new System.Drawing.Size((int)(_bitmapImage1.Width *0.9), (int)(testDestColor.Width*0.9));*/

                        //_histogram = new ClooBuffer<uint>(_context, ComputeMemoryFlags.ReadWrite | ComputeMemoryFlags.AllocateHostPointer, 256);

                        sizeChanged = (final_output == null) || (final_output.Count != _bitmapImage1.Bitmap.Width * _bitmapImage1.Bitmap.Height * 4);
                        if (sizeChanged)
                        {
                            if (final_output != null)
                            {
                                final_output.Dispose();
                                final_out_ptr.Free();
                            }
                            int size = _bitmapImage1.Bitmap.Width * _bitmapImage1.Bitmap.Height * 4;
                            final_out_host = new byte[size];
                            final_out_ptr = System.Runtime.InteropServices.GCHandle.Alloc(final_out_host, System.Runtime.InteropServices.GCHandleType.Pinned);
                            final_output = new ClooBuffer<byte>(_context, ComputeMemoryFlags.UseHostPointer, size, final_out_ptr.AddrOfPinnedObject());
                            final_output._hostBufferByte = final_out_host; // todo: this sucks
                        }

                        if (!useVid)//!IsHDR)//trackBar1.Visible)
                        {
                            _clooImageByteOriginal = ClooImage2DFloatRgbA.CreateFromBitmap(_context, ComputeMemoryFlags.UseHostPointer , _bitmapImage1.Bitmap);
                        }
                        else
                        {
                            _clooImageByteOriginal = ClooImage2DFloatRgbA.CreateFromFloatArray(_context, ComputeMemoryFlags.UseHostPointer, vidData, vidWidth, vidHight);
                        }
                        /*else
                        {  // load HDR
                            _clooImageByteOriginal = ClooImage2DFloatRgbA.CreateFromFloatArray(_context, ComputeMemoryFlags.ReadOnly //| ComputeMemoryFlags.HostNoAccess
                                | ComputeMemoryFlags.CopyHostPointer,
                                HdrRdWr.currentFloatData, _bitmapImage1.Width, _bitmapImage1.Height);
                            trackBar1.Visible = false;
                        }*/

                        LogManager.GetLogger().i("CAMPROC-i",(SystemClock.ElapsedRealtime() - prevRuntime).ToString());

                        if (_clooImageByteOriginal == null) return;

                        //_clooImageByteOriginal.HostBuffer = HdrRdWr.currentFloatData;

                        //_clooImageByteOriginal.WriteToDevice(_queue);

                        //_clooImageByteGrayOriginal = ClooImage2DByteA.CreateFromBitmap(_context, ComputeMemoryFlags.ReadWrite | ComputeMemoryFlags.UseHostPointer, _bitmapImage1);
                        //_clooImageByteGrayOriginal.WriteToDevice(_queue);

                        //_clooImageByteResult = ClooImage2DByteRgbA.Create(_context, ComputeMemoryFlags.ReadWrite | ComputeMemoryFlags.UseHostPointer, _bitmapImage1.Width, _bitmapImage1.Height);

                        if (reestablish || (_clooImageByteIntermediate.Count==0))
                        {
                            _clooImageByteIntermediate.Clear();
                            for (int i = 0; i < numKernels; i++)
                            {
                                _clooImageByteIntermediate.Add(null);
                            }
                        }

                        //_clooImageByteResultA = ClooImage2DByteA.Create(_context, ComputeMemoryFlags.ReadWrite | ComputeMemoryFlags.UseHostPointer, _bitmapImage1.Width, _bitmapImage1.Height);
                        //_clooImageFloatOriginal = ClooImage2DFloatRgbA.Create(_context, ComputeMemoryFlags.ReadWrite | ComputeMemoryFlags.UseHostPointer, _bitmapImage1.Width, _bitmapImage1.Height);
                        //_clooImageFloatGrayOriginal = ClooImage2DFloatA.Create(_context, ComputeMemoryFlags.ReadWrite | ComputeMemoryFlags.UseHostPointer, _bitmapImage1.Width, _bitmapImage1.Height);
                        //_clooImageFloatTemp1 = ClooImage2DFloatRgbA.Create(_context, ComputeMemoryFlags.ReadWrite | ComputeMemoryFlags.UseHostPointer, _bitmapImage1.Width, _bitmapImage1.Height);
                        //_clooImageFloatTemp2 = ClooImage2DFloatRgbA.Create(_context, ComputeMemoryFlags.ReadWrite | ComputeMemoryFlags.UseHostPointer, _bitmapImage1.Width, _bitmapImage1.Height);
                        //_clooImageFloatATemp1 = ClooImage2DFloatA.Create(_context, ComputeMemoryFlags.ReadWrite | ComputeMemoryFlags.UseHostPointer, _bitmapImage1.Width, _bitmapImage1.Height);
                        /*_clooImageFloatATemp2 = ClooImage2DFloatA.Create(_context, ComputeMemoryFlags.ReadWrite | ComputeMemoryFlags.UseHostPointer, _bitmapImage1.Width, _bitmapImage1.Height);
                        _clooImageFloatIntegral = ClooImage2DFloatA.Create(_context, ComputeMemoryFlags.ReadWrite | ComputeMemoryFlags.UseHostPointer, _bitmapImage1.Width + 1, _bitmapImage1.Height + 1);
                        _clooImageUIntIntegral = ClooImage2DUIntA.Create(_context, ComputeMemoryFlags.ReadWrite | ComputeMemoryFlags.UseHostPointer, _bitmapImage1.Width + 1, _bitmapImage1.Height + 1);
                        _clooImageUIntIntegralSquare = ClooImage2DUIntA.Create(_context, ComputeMemoryFlags.ReadWrite | ComputeMemoryFlags.UseHostPointer, _bitmapImage1.Width + 1, _bitmapImage1.Height + 1);
                         */
                        _queue.Finish();
                        //_queue.Flush();
                    }
                }
                /*catch (Exception ex)
                {
                    // show exception
                    MessageBox.Show(ex.Message, ex.GetType().ToString(), MessageBoxButtons.AbortRetryIgnore);
                }*/

                //System.Windows.Controls.Image tempI = null;
                /*lock (_queue)
                {
                    _kernels.GrayScale(_queue, _clooImageByteOriginal, _clooImageFloatGrayOriginal);
                    _queue.Finish();

                    currentOrigImage = _clooImageFloatGrayOriginal.ToBitmap(_queue);//.ToBitmapSource();
                }

                lock (_queue)
                {
                    _kernels.Sobel(_queue, _clooImageFloatGrayOriginal, _clooImageFloatATemp1, _sampler);
                    _kernels.FloatToByte(_queue, _clooImageFloatATemp1, _clooImageByteResultA);
                    _queue.Finish();
                    //label6.Content = stopwatch.ElapsedMilliseconds + " ms - sobel";
                    currentOrigImage = _clooImageByteResultA.ToBitmap(_queue);//.ToBitmapSource();
                }*/


                {
                    //Stopwatch stopwatch = Stopwatch.StartNew();
                    for (int i = 0; i < numKernels; i++)
                    {
                        // Quickly release or reuse buffers that aren't going to be used anymore
                        bool ReusableFound = _clooImageByteIntermediate[i]!=null;
                        for (int j = 0; ReusableFound && (j < i) && (i != numKernels - 1); j++)
                        {
                            if (_clooImageByteIntermediate[j] != null)
                            {
                                bool Used = false;
                                for (int k = i; k < numKernels; k++)
                                {
                                    if ((kernelInputBuffers[k] == j) || (secondaryInputBuffers[k] == j)) { Used = true; break; }
                                }
                                if (!Used)
                                {
                                    if (!ReusableFound)
                                    {
                                        _clooImageByteIntermediate[i] = _clooImageByteIntermediate[j];
                                        _clooImageByteIntermediate[j] = null;
                                        ReusableFound = true;
                                        //break;
                                    }
                                    else
                                    {
                                        _clooImageByteIntermediate[j].HostBuffer = null;
                                        _clooImageByteIntermediate[j].Dispose();
                                        _clooImageByteIntermediate[j] = null;
                                    }
                                }
                            }
                        }
                        if (!ReusableFound)
                        {
                            //currentImage.p
                            //float[]  newHostBuf = new float[_bitmapImage1.Width*_bitmapImage1.Height*4];
                            if (i != numKernels - 1)
                                _clooImageByteIntermediate[i] = ClooImage2DFloatRgbA.CreateHostNoAccess(_context, ComputeMemoryFlags.AllocateHostPointer | ComputeMemoryFlags.HostNoAccess, _bitmapImage1.Bitmap.Width, _bitmapImage1.Bitmap.Height);
                            else
                            {
                                _clooImageByteIntermediate[i] = ClooImage2DFloatRgbA.CreateHostNoAccess(_context, ComputeMemoryFlags.AllocateHostPointer | ComputeMemoryFlags.HostNoAccess, _bitmapImage1.Bitmap.Width, _bitmapImage1.Bitmap.Height);
                            }
                            //_clooImageByteIntermediate[i] = ClooImage2DFloatRgbA.CreateFromFloatArray(_context, ComputeMemoryFlags.ReadWrite , newHostBuf,  _bitmapImage1.Width, _bitmapImage1.Height);

                            if (_clooImageByteIntermediate[i] == null) return;
                        }

                        _queue.Finish();

                        LogManager.GetLogger().i("CAMPROC-m:" + i, (SystemClock.ElapsedRealtime() - prevRuntime).ToString());
                        _clooImageByteCurrentIn = (kernelInputBuffers[i] == -1) ? _clooImageByteOriginal : _clooImageByteIntermediate[kernelInputBuffers[i]];
                        lock (_queue)
                        {
                            if (secondaryInputBuffers[i] == -2) // no seconadary input
                            {
                                //MessageBox.Show("A");
                                if (i != numKernels - 1)
                                    _specialkernel.SpecialKernel(_queue, _context, i, null, _clooImageByteCurrentIn, _clooImageByteIntermediate[i]);
                                else
                                    _specialkernel.SpecialKernel(_queue, _context, i, final_output, _clooImageByteCurrentIn, _clooImageByteIntermediate[i]);
                                //MessageBox.Show("B");
                            }
                            else
                            {
                                ClooImage2DFloatRgbA _cloo2ndImageByteCurrentIn = (secondaryInputBuffers[i] == -1) ? _clooImageByteOriginal : _clooImageByteIntermediate[secondaryInputBuffers[i]];
                                if (i != numKernels-1)
                                    _specialkernel.SpecialKernel(_queue, _context, i, null, _clooImageByteCurrentIn, _clooImageByteIntermediate[i], _cloo2ndImageByteCurrentIn);
                                else
                                    _specialkernel.SpecialKernel(_queue, _context, i, final_output, _clooImageByteCurrentIn, _clooImageByteIntermediate[i], _cloo2ndImageByteCurrentIn);
                            }
                        }
                        //_kernels.FloatToByte(_queue, _clooImageFloatATemp1, _clooImageByteResultA);
                        LogManager.GetLogger().i("CAMPROC-j:"+i, (SystemClock.ElapsedRealtime() - prevRuntime).ToString());
                    }

                    //label6.Content = stopwatch.ElapsedMilliseconds + " ms - sobel";
                    //currentOrigImage.Dispose();
                    //currentOrigImage = currentImage;
                    //toolStripStatusLabel1.Text = stopwatch.ElapsedMilliseconds + " ms - " + toolStripComboBox1.Text;


                    //currentImage.Dispose();

                    //Bitmap curImg = _clooImageByteIntermediate[numKernels - 1].ToBitmap(_queue);
                    if (sizeChanged || !useVid || (curByteBuffer == null))
                    {
                        curByteBuffer = Java.Nio.ByteBuffer.AllocateDirect(_bitmapImage1.Bitmap.Width * _bitmapImage1.Bitmap.Height * 4);
                        curImg = Bitmap.CreateBitmap(_bitmapImage1.Bitmap.Width, _bitmapImage1.Bitmap.Height, Bitmap.Config.Argb8888);
                        curImg = final_output.ToBitmap(_queue, curImg, curByteBuffer);
                        if (!useVid)
                        {
                            curByteBuffer.Dispose();
                            curByteBuffer = null;
                        }
                    }
                    else
                    {
                        //if (curByteBuffer.Capacity() != (_bitmapImage1.Bitmap.Width * _bitmapImage1.Bitmap.Height * 4))
                        {
                            curByteBuffer.Dispose();
                            curByteBuffer = Java.Nio.ByteBuffer.AllocateDirect(_bitmapImage1.Bitmap.Width * _bitmapImage1.Bitmap.Height * 4);
                        }
                        curImg = final_output.ToBitmap(_queue, curImg, curByteBuffer);
                    }

                    currentImage = new BitmapDrawable(Resources, curImg);
                    if (currentImage == null) return;

                    /*if (ImageStack.Count > ImageStackPos + 1)
                    {
                        ImageStack.RemoveRange(ImageStackPos + 1, ImageStack.Count - ImageStackPos - 1);
                    }
                    ImageStack.Add(curImg);
                    ImageStackPos++;*/

                    //toolStripUndoButton.Enabled = true;
                    //toolStripRedoButton.Enabled = false;
                    
                    //pictureBox1.Image = currentImage;
                }

                if (!useVid)
                {
                    if (_clooImageByteOriginal != null) { _clooImageByteOriginal.HostBuffer = null; _clooImageByteOriginal.Dispose(); }
                    if (_clooImageByteGrayOriginal != null) { _clooImageByteGrayOriginal.HostBuffer = null; _clooImageByteGrayOriginal.Dispose(); }
                    if (_clooImageByteResult != null) { _clooImageByteResult.HostBuffer = null; _clooImageByteResult.Dispose(); }
                    for (int i = 0; i < _clooImageByteIntermediate.Count; i++)
                    {
                        if (_clooImageByteIntermediate[i] != null)
                        {
                            _clooImageByteIntermediate[i].HostBuffer = null;
                            _clooImageByteIntermediate[i].Dispose();
                            _clooImageByteIntermediate[i] = null;
                        }
                    }
                    _clooImageByteIntermediate.Clear();
                    if (_clooImageByteResultA != null) { _clooImageByteResultA.HostBuffer = null; _clooImageByteResultA.Dispose(); }
                }

                //mImageView. ResetMatrix();
                if (!useVid) mImageView.SetImageDrawable(currentImage);

                if (!useVid) GC.Collect();

            }
            //catch (Exception ex)
            {
                //System.Windows.Forms.MessageBox.Show(ex.Message);
                //VisibDeviceNameCombobox_SelectedIndexChanged(null, null);
            }
        }

        void decodeKernels(string[] kernelFormulas)
        {
            for (int k = 0; k < kernelFormulas.Length; k++)
            {
                string KernelString = "";
                //get function type
                int functionType = -1;
                bool normalize = false;
                string currentFormulaUnparsed = kernelFormulas[k].Trim();
                //if (currentFormulaUnparsed.Length == 0) continue;
                string functionName = currentFormulaUnparsed.Substring(currentFormulaUnparsed.IndexOf('[') + 1).Substring(0, currentFormulaUnparsed.IndexOf(':') - 1).Trim();
                switch (functionName.ToLower())
                {
                    case "color": functionType = 0; break;
                    case "nrmconv": normalize = true; functionType = 1; break;
                    case "convolv": functionType = 1; break;
                    case "reduce": functionType = 2; break;
                    case "scale": functionType = 3; break;
                    case "exp10": functionType = 4; break;
                    case "log10": functionType = 5; break;
                    case "pow": functionType = 6; break;
                    case "min": functionType = 7; break;
                    case "max": functionType = 8; break;
                }
                currentFormulaUnparsed = currentFormulaUnparsed.Substring(currentFormulaUnparsed.IndexOf(':') + 1).Trim();

                secondaryInputBuffers[k] = -2; // meaning no secondary input
                secondaryInputFunc[k] = "";
                if (currentFormulaUnparsed.StartsWith("{"))
                {
                    string inputBufStrng = currentFormulaUnparsed.Substring(1, currentFormulaUnparsed.IndexOf("}") - 1);
                    if (inputBufStrng.Contains(","))
                    {
                        inputBufStrng = (inputBufStrng[0].Equals(',')) ? "" : inputBufStrng.Substring(0, inputBufStrng.IndexOf(","));

                        currentFormulaUnparsed = currentFormulaUnparsed.Substring(currentFormulaUnparsed.IndexOf(',') + 1).Trim();
                        string sendInputStrng = currentFormulaUnparsed.Substring(0, currentFormulaUnparsed.IndexOf("|"));
                        if (sendInputStrng.Length != 0)
                        {
                            secondaryInputBuffers[k] = int.Parse(sendInputStrng);
                            KernelString += "//Secondary input is output of Pass" + secondaryInputBuffers[k] + "\n";
                        }
                        else
                        {
                            secondaryInputBuffers[k] = -1;
                            KernelString += "//Secondary input is original surface\n";
                        }
                        currentFormulaUnparsed = currentFormulaUnparsed.Substring(currentFormulaUnparsed.IndexOf('|') + 1).Trim(); // TODO: this should be changed to support multiple secondary inputs
                        secondaryInputFunc[k] = currentFormulaUnparsed.Substring(0, currentFormulaUnparsed.IndexOf('}'));
                        currentFormulaUnparsed = currentFormulaUnparsed.Substring(currentFormulaUnparsed.IndexOf('}') + 1).Trim();
                    }
                    if (inputBufStrng.Length != 0)
                    {
                        int inpBuf = int.Parse(inputBufStrng);
                        kernelInputBuffers.Add(inpBuf);
                        KernelString += "//Primary input is output of Pass" + inpBuf + "\n";
                    }
                    else
                    {
                        kernelInputBuffers.Add(-1);
                        KernelString += "//Primary input is original surface\n";
                    }
                }
                else
                {
                    kernelInputBuffers.Add(-1);
                    KernelString += "//Primary input is original surface\n";
                }

                //get function details
                string newPixelColor = "(float4)(0, 0, 0, 0)";
                List<int[]> convolvLis = new List<int[]>();
                if (functionType == 0) // color
                {
                    string colorVal = currentFormulaUnparsed.Substring(currentFormulaUnparsed.IndexOf('('));
                    colorVal = colorVal.Substring(0, colorVal.IndexOf(')') + 1);
                    if (colorVal.Contains("{"))
                    {
                        colorVal = colorVal.Substring(0, colorVal.IndexOf('{')) + ")";
                    }
                    newPixelColor = "(float4)" + colorVal;
                }
                else if (functionType == 2) //reduce
                {
                    string weights_s = currentFormulaUnparsed.Substring(currentFormulaUnparsed.IndexOf('(') + 1);
                    weights_s = weights_s.Substring(0, weights_s.IndexOf(')'));
                    string[] weights_int = weights_s.Split(new char[] { ',' }, 3);
                    if (weights_int[weights_int.Length - 1].Contains("{")) weights_int[weights_int.Length - 1] = weights_int[weights_int.Length - 1].Substring(0, weights_int[weights_int.Length - 1].IndexOf('{'));
                    newPixelColor = "(float)((pixel_orig.x*" + weights_int[0] + "+pixel_orig.y*" + weights_int[1] + "+pixel_orig.z*" + weights_int[2] + ")/(" + weights_int[0] + "+" + weights_int[1] + "+" + weights_int[2] + "));";
                }
                else if (functionType == 3) //scale 
                {
                    string weights_s = currentFormulaUnparsed.Substring(currentFormulaUnparsed.IndexOf('(') + 1);
                    weights_s = weights_s.Substring(0, weights_s.IndexOf(')'));
                    string[] weights_int = weights_s.Split(',');
                    if (weights_int[weights_int.Length - 1].Contains("{")) weights_int[weights_int.Length - 1] = weights_int[weights_int.Length - 1].Substring(0, weights_int[weights_int.Length - 1].IndexOf('{'));
                    newPixelColor = "(float4)pixel_orig*(float4)(" + weights_int[0] + "," + weights_int[1] + "," + weights_int[2] + ",1.0)";
                }
                else if (functionType == 4) //exp10 
                {
                    newPixelColor = "(float4)(exp10(pixel_orig.x),exp10(pixel_orig.y),exp10(pixel_orig.z),255)";
                }
                else if (functionType == 5) //log10 
                {
                    newPixelColor = "(float4)(log10(pixel_orig.x),log10(pixel_orig.y),log10(pixel_orig.z),255)";
                }
                else if (functionType == 6) //pow
                {
                    //newPixelColor = "(float4)(pow(pixel_orig.x, 1.0/4.2),pow(pixel_orig.y,1.0/4.2),pow(pixel_orig.z,1.0/4.2),255)*25";
                    string weights_s = currentFormulaUnparsed.Substring(currentFormulaUnparsed.IndexOf('(') + 1);
                    weights_s = weights_s.Substring(0, weights_s.IndexOf(')'));
                    string[] weights_int = weights_s.Split(',');
                    if (weights_int[weights_int.Length - 1].Contains("{")) weights_int[weights_int.Length - 1] = weights_int[weights_int.Length - 1].Substring(0, weights_int[weights_int.Length - 1].IndexOf('{'));
                    newPixelColor = "(float4)(pow(pixel_orig.x," + weights_int[0] + "),pow(pixel_orig.y," + weights_int[0] + "),pow(pixel_orig.z," + weights_int[0] + "),255)";
                }
                else if ((functionType == 1) || // convolv  TODO: Add conditional neighbors to list 
                         (functionType == 7) ||
                         (functionType == 8))
                {
                    int startOfConvList = currentFormulaUnparsed.IndexOf('(') + 1;
                    string currentConvComp = currentFormulaUnparsed.Substring(startOfConvList, currentFormulaUnparsed.IndexOf(')') - startOfConvList).Trim();
                    while ((currentConvComp.Length > 0) && (currentConvComp[0] == '{'))
                    {
                        string[] comBreakdown = currentConvComp.Substring(1, currentConvComp.IndexOf('}') - 1).Split(',');
                        convolvLis.Add(new int[3] { int.Parse(comBreakdown[0]), int.Parse(comBreakdown[1]), int.Parse(comBreakdown[2]) });
                        currentConvComp = currentConvComp.Substring(currentConvComp.IndexOf('}') + 1).Trim();
                    }
                }
                currentFormulaUnparsed = currentFormulaUnparsed.Substring(currentFormulaUnparsed.IndexOf(')') + 1).Trim();

                //get the conditions
                int testOrigColor = -1;
                string[] origColorCheck = { "0", "0", "0", "0" };
                string[] origColorCheckLimits = { "0", "0", "0", "0" };
                int testDestColor = -1;
                string[] destColorCheck = { "0", "0", "0", "0" };
                string[] destColorCheckLimits = { "0", "0", "0", "0" };
                int testSrcColor = -1;
                string[] srcColorCheck = { "0", "0", "0", "0" };
                string[] srcColorCheckLimits = { "0", "0", "0", "0" };

                while (currentFormulaUnparsed.Length > 0)
                {
                    string crntCond = currentFormulaUnparsed.Substring(1, currentFormulaUnparsed.IndexOf(')')).Trim();

                    if (crntCond.StartsWith("Orig_"))
                    {
                        crntCond = crntCond.Substring("Orig_".Length);
                        if (crntCond.StartsWith("AbsDiffThrsh:") ||
                            crntCond.StartsWith("PrcntDiffThrsh:"))
                        {
                            if (crntCond.StartsWith("AbsDiffThrsh:"))
                                testOrigColor = 1;
                            else if (crntCond.StartsWith("PrcntDiffThrsh:"))
                                testOrigColor = 2;
                            crntCond = crntCond.Substring(crntCond.IndexOf(':') + 1);
                            string origColorCheckstring = crntCond.Split('-')[0].Trim();
                            origColorCheckstring = origColorCheckstring.Substring(origColorCheckstring.IndexOf('{') + 1);
                            origColorCheckstring = origColorCheckstring.Substring(0, origColorCheckstring.IndexOf('}'));
                            origColorCheck = origColorCheckstring.Split(',');
                            string origColorCheckRandgStrng = crntCond.Split('-')[1].Trim();
                            origColorCheckRandgStrng = origColorCheckRandgStrng.Substring(origColorCheckRandgStrng.IndexOf('{') + 1);
                            origColorCheckRandgStrng = origColorCheckRandgStrng.Substring(0, origColorCheckRandgStrng.IndexOf('}'));
                            origColorCheckLimits = origColorCheckRandgStrng.Split(',');
                        }
                    }
                    else if (crntCond.StartsWith("Dest_"))
                    {
                        crntCond = crntCond.Substring("Dest_".Length);
                        if (crntCond.StartsWith("AbsDiffThrsh:") ||
                            crntCond.StartsWith("PrcntDiffThrsh:"))
                        {
                            if (crntCond.StartsWith("AbsDiffThrsh:"))
                                testDestColor = 1;
                            else if (crntCond.StartsWith("PrcntDiffThrsh:"))
                                testDestColor = 2;
                            crntCond = crntCond.Substring(crntCond.IndexOf(':') + 1);
                            string destColorCheckstring = crntCond.Split('-')[0].Trim();
                            destColorCheckstring = destColorCheckstring.Substring(destColorCheckstring.IndexOf('{') + 1);
                            destColorCheckstring = destColorCheckstring.Substring(0, destColorCheckstring.IndexOf('}'));
                            destColorCheck = destColorCheckstring.Split(',');
                            string destColorCheckRandgStrng = crntCond.Split('-')[1].Trim();
                            destColorCheckRandgStrng = destColorCheckRandgStrng.Substring(destColorCheckRandgStrng.IndexOf('{') + 1);
                            destColorCheckRandgStrng = destColorCheckRandgStrng.Substring(0, destColorCheckRandgStrng.IndexOf('}'));
                            destColorCheckLimits = destColorCheckRandgStrng.Split(',');
                        }
                    }
                    else if (crntCond.StartsWith("Src_"))
                    {
                        crntCond = crntCond.Substring("Src_".Length);
                        if (crntCond.StartsWith("AbsDiffThrsh:") ||
                            crntCond.StartsWith("PrcntDiffThrsh:"))
                        {
                            if (crntCond.StartsWith("AbsDiffThrsh:"))
                                testSrcColor = 1;
                            else if (crntCond.StartsWith("PrcntDiffThrsh:"))
                                testSrcColor = 2;
                            crntCond = crntCond.Substring(crntCond.IndexOf(':') + 1);
                            string destColorCheckstring = crntCond.Split('-')[0].Trim();
                            destColorCheckstring = destColorCheckstring.Substring(destColorCheckstring.IndexOf('{') + 1);
                            destColorCheckstring = destColorCheckstring.Substring(0, destColorCheckstring.IndexOf('}'));
                            srcColorCheck = destColorCheckstring.Split(',');
                            string destColorCheckRandgStrng = crntCond.Split('-')[1].Trim();
                            destColorCheckRandgStrng = destColorCheckRandgStrng.Substring(destColorCheckRandgStrng.IndexOf('{') + 1);
                            destColorCheckRandgStrng = destColorCheckRandgStrng.Substring(0, destColorCheckRandgStrng.IndexOf('}'));
                            srcColorCheckLimits = destColorCheckRandgStrng.Split(',');
                        }
                    }
                    currentFormulaUnparsed = currentFormulaUnparsed.Substring(currentFormulaUnparsed.IndexOf(')') + 1).Trim();
                }

                //KernelString = "__kernel void specialKernel(__write_only image2d_t outputImage, __read_only image2d_t inputImage, __global ulong *src)\n" +
                KernelString += "__kernel void specialKernel" + k + "(read_only image2d_t inputImage, write_only image2d_t outputImage, __global read_only int* starting";

                if (secondaryInputBuffers[k] != -2) KernelString += ", read_only image2d_t inputImage2";
                if (k == kernelFormulas.Length - 1) KernelString += ", global char* final_output";

                KernelString += ")\n" +
                    "{\n" +
                    "    const sampler_t sampler=CLK_NORMALIZED_COORDS_FALSE|CLK_ADDRESS_CLAMP_TO_EDGE|CLK_FILTER_NEAREST;\n" +
                    "    int i = get_global_id(0)+starting[0];\n" +
                    "    int j = get_global_id(1)+starting[1];\n" +
                    "    //if (i>=src[0]) return;\n" +
                    "    //if (j>=src[1]) return;\n" +
                    "    float accumWeights = 0;\n" +
                    "    float4 pixel_orig=read_imagef(inputImage,sampler,(int2)(i,j));\n" +
                    "    //long4 pixel_orig_long=convert_long4(pixel_orig);\n";

                //Conditions
                if (testOrigColor != -1)
                {
                    string compX = (origColorCheck.Length == 4) ? origColorCheck[0] : "pixel_orig.x";
                    string compY = (origColorCheck.Length == 4) ? origColorCheck[1] : "pixel_orig.y";
                    string compZ = (origColorCheck.Length == 4) ? origColorCheck[2] : "pixel_orig.z";
                    string compW = (origColorCheck.Length == 4) ? origColorCheck[3] : "pixel_orig.w";
                    if (testOrigColor == 1) //AbsDiffThrsh
                    {
                        KernelString += "    float pixel_diffR = fabs(pixel_orig.x- (float)" + compX + ");\n"
                                 + "    float pixel_diffG = fabs(pixel_orig.y- (float)" + compY + ");\n"
                                 + "    float pixel_diffB = fabs(pixel_orig.z- (float)" + compZ + ");\n"
                                 + "    float pixel_diffA = fabs(pixel_orig.w- (float)" + compW + ");\n"
                                 + "    if ((pixel_diffR<" + origColorCheckLimits[0] + ")&&(pixel_diffG<" + origColorCheckLimits[1] + ")&&(pixel_diffB<" + origColorCheckLimits[2] + " )&&(pixel_diffA<" + origColorCheckLimits[3] + ")){\n\t";
                    }
                    else if (testOrigColor == 2) //PrcntDiffThrsh
                    {
                        KernelString +=
                                  "    float pixel_prcntDiffR = fabs((pixel_orig.x-(float)" + compX + ")/" + compX + ")*100;\n"
                                + "    float pixel_prcntDiffG = fabs((pixel_orig.y-(float)" + compY + ")/" + compY + ")*100;\n"
                                + "    float pixel_prcntDiffB = fabs((pixel_orig.z-(float)" + compZ + ")/" + compZ + ")*100;\n"
                                + "    float pixel_prcntDiffA = fabs((pixel_orig.w-(float)" + compW + ")/" + compW + ")*100;\n"
                                + "    if ((pixel_prcntDiffR<" + origColorCheckLimits[0] + ")&&(pixel_prcntDiffG<" + origColorCheckLimits[1] + ")&&(pixel_prcntDiffB<" + origColorCheckLimits[2] + " )&&(pixel_prcntDiffA<" + origColorCheckLimits[3] + ")){\n\t";
                    }
                }

                //Do perPix function
                if ((functionType == 0) || // color
                    (functionType == 3) || // scale
                    (functionType == 4) || // exp10
                    (functionType == 5) || // log10
                    (functionType == 6))   // pow
                {
                    KernelString += "    float4 pixel_new = " + newPixelColor + ";\n";
                }
                else if (functionType == 2) // reduce
                {
                    KernelString += "    float reduced = " + newPixelColor + ";\n";
                    KernelString += "    float4 pixel_new =  (float4)(reduced, reduced, reduced, 255);\n";
                }
                else if (functionType == 1) // convolv
                {
                    KernelString += "    float4 pixel_new = (float4)(0, 0, 0, 255);\n";
                    for (int c = 0; c < convolvLis.Count; c++)
                    {
                        KernelString += "    float4 pixel" + c + " = read_imagef(inputImage,sampler,(int2)(i" + ((convolvLis[c][0] > 0) ? "+" + convolvLis[c][0] : "-" + (0 - convolvLis[c][0])) + ",j" + ((convolvLis[c][1] >= 0) ? "+" + convolvLis[c][1] : "-" + (0 - convolvLis[c][1])) + "));\n";
                    }
                    for (int c = 0; c < convolvLis.Count; c++)
                    {
                        if (testSrcColor != -1)
                        {
                            string compX = ((srcColorCheck.Length == 4) ? srcColorCheck[0] : "pixel_orig.x");
                            string compY = ((srcColorCheck.Length == 4) ? srcColorCheck[1] : "pixel_orig.y");
                            string compZ = ((srcColorCheck.Length == 4) ? srcColorCheck[2] : "pixel_orig.z");
                            string compW = ((srcColorCheck.Length == 4) ? srcColorCheck[3] : "pixel_orig.w");

                            if (testSrcColor == 1) //AbsDiffThrsh
                            {
                                KernelString += "    float pixel" + c + "_diffR = fabs(pixel" + c + ".x- (float)" + compX + ");\n"
                                             + "    float pixel" + c + "_diffG = fabs(pixel" + c + ".y- (float)" + compY + ");\n"
                                             + "    float pixel" + c + "_diffB = fabs(pixel" + c + ".z- (float)" + compZ + ");\n"
                                             + "    float pixel" + c + "_diffA = fabs(pixel" + c + ".w- (float)" + compW + ");\n"
                                             + "   if ((pixel" + c + "_diffR<" + srcColorCheckLimits[0] + ")&& (pixel" + c + "_diffG<" + srcColorCheckLimits[1] + ")&&(pixel" + c + "_diffB<" + srcColorCheckLimits[2] + " )&&(pixel" + c + "_diffA<" + srcColorCheckLimits[3] + ")){\n\t";
                            }
                            else if (testSrcColor == 2) //PrcntDiffThrsh
                            {
                                KernelString += "    float pixel" + c + "_prcntDiffR = fabs((pixel" + c + ".x-(float)" + compX + ")/" + compX + ")*100;\n"
                                                + "    float pixel" + c + "_prcntDiffG = fabs((pixel" + c + ".y-(float)" + compY + ")/" + compY + ")*100;\n"
                                                + "    float pixel" + c + "_prcntDiffB = fabs((pixel" + c + ".z-(float)" + compZ + ")/" + compZ + ")*100;\n"
                                                + "    float pixel" + c + "_prcntDiffA = fabs((pixel" + c + ".w-(float)" + compW + ")/" + compW + ")*100;\n"
                                                + "   if ((pixel" + c + "_prcntDiffR<" + srcColorCheckLimits[0] + ")&& (pixel" + c + "_prcntDiffG<" + srcColorCheckLimits[1] + ")&&(pixel" + c + "_prcntDiffB<" + srcColorCheckLimits[2] + " )&&(pixel" + c + "_prcntDiffA<" + srcColorCheckLimits[3] + ")){\n\t";
                            }
                        }

                        if (convolvLis[c][2] > 0)
                        {
                            KernelString += "    pixel_new += (pixel" + c + ")*(float)" + convolvLis[c][2] + ".0; accumWeights += " + convolvLis[c][2] + ";\n";
                        }
                        else
                        {
                            KernelString += "    pixel_new -= (pixel" + c + ")*(float)" + (0 - convolvLis[c][2]) + ".0; accumWeights -= " + (0 - convolvLis[c][2]) + ";\n";
                        }

                        if (testSrcColor != -1) KernelString += "    }\n";
                    }

                    if (normalize)
                        KernelString += "    pixel_new = (accumWeights==0)?0:pixel_new/accumWeights; pixel_new.w = 255;\n";
                }
                else if ((functionType == 7) || (functionType == 8)) // min or max
                {
                    KernelString += "    float4 pixel_new = pixel_orig;\n";
                    for (int c = 0; c < convolvLis.Count; c++)
                    {
                        KernelString += "    float4 pixel" + c + " = read_imagef(inputImage,sampler,(int2)(i" + ((convolvLis[c][0] > 0) ? "+" + convolvLis[c][0] : "-" + (0 - convolvLis[c][0])) + ",j" + ((convolvLis[c][1] >= 0) ? "+" + convolvLis[c][1] : "-" + (0 - convolvLis[c][1])) + "));\n";
                    }
                    for (int c = 0; c < convolvLis.Count; c++)
                    {
                        if (testSrcColor != -1)
                        {
                            string compX = ((srcColorCheck.Length == 4) ? srcColorCheck[0] : "pixel_orig.x");
                            string compY = ((srcColorCheck.Length == 4) ? srcColorCheck[1] : "pixel_orig.y");
                            string compZ = ((srcColorCheck.Length == 4) ? srcColorCheck[2] : "pixel_orig.z");
                            string compW = ((srcColorCheck.Length == 4) ? srcColorCheck[3] : "pixel_orig.w");

                            if (testSrcColor == 1) //AbsDiffThrsh
                            {
                                KernelString += "    float pixel" + c + "_diffR = fabs(pixel" + c + ".x- (float)" + compX + ");\n"
                                             + "    float pixel" + c + "_diffG = fabs(pixel" + c + ".y- (float)" + compY + ");\n"
                                             + "    float pixel" + c + "_diffB = fabs(pixel" + c + ".z- (float)" + compZ + ");\n"
                                             + "    float pixel" + c + "_diffA = fabs(pixel" + c + ".w- (float)" + compW + ");\n"
                                             + "   if ((pixel" + c + "_diffR<" + srcColorCheckLimits[0] + ")&& (pixel" + c + "_diffG<" + srcColorCheckLimits[1] + ")&&(pixel" + c + "_diffB<" + srcColorCheckLimits[2] + " )&&(pixel" + c + "_diffA<" + srcColorCheckLimits[3] + ")){\n\t";
                            }
                            else if (testSrcColor == 2) //PrcntDiffThrsh
                            {
                                KernelString += "    float pixel" + c + "_prcntDiffR = fabs((pixel" + c + ".x-(float)" + compX + ")/" + compX + ")*100;\n"
                                                + "    float pixel" + c + "_prcntDiffG = fabs((pixel" + c + ".y-(float)" + compY + ")/" + compY + ")*100;\n"
                                                + "    float pixel" + c + "_prcntDiffB = fabs((pixel" + c + ".z-(float)" + compZ + ")/" + compZ + ")*100;\n"
                                                + "    float pixel" + c + "_prcntDiffA = fabs((pixel" + c + ".w-(float)" + compW + ")/" + compW + ")*100;\n"
                                                + "   if ((pixel" + c + "_prcntDiffR<" + srcColorCheckLimits[0] + ")&& (pixel" + c + "_prcntDiffG<" + srcColorCheckLimits[1] + ")&&(pixel" + c + "_prcntDiffB<" + srcColorCheckLimits[2] + " )&&(pixel" + c + "_prcntDiffA<" + srcColorCheckLimits[3] + ")){\n\t";
                            }
                        }

                        if (convolvLis[c][2] > 0)
                        {

                            KernelString += "    if (pixel_new.x " + ((functionType == 7) ? ">" : "<") + " pixel" + c + ".x) pixel_new.x = (pixel" + c + ".x*(float)" + convolvLis[c][2] + ".0);\n";
                            KernelString += "    if (pixel_new.y " + ((functionType == 7) ? ">" : "<") + " pixel" + c + ".y) pixel_new.y = (pixel" + c + ".y*(float)" + convolvLis[c][2] + ".0);\n";
                            KernelString += "    if (pixel_new.z " + ((functionType == 7) ? ">" : "<") + " pixel" + c + ".z) pixel_new.z = (pixel" + c + ".z*(float)" + convolvLis[c][2] + ".0);\n";
                        }
                        else
                        {
                            KernelString += "    if (pixel_new.x " + ((functionType == 7) ? ">" : "<") + " pixel" + c + ".x) pixel_new.x = (pixel" + c + ").x*(float)" + (0 - convolvLis[c][2]) + ".0);\n";
                            KernelString += "    if (pixel_new.y " + ((functionType == 7) ? ">" : "<") + " pixel" + c + ".y) pixel_new.y = (pixel" + c + ").y*(float)" + (0 - convolvLis[c][2]) + ".0);\n";
                            KernelString += "    if (pixel_new.z " + ((functionType == 7) ? ">" : "<") + " pixel" + c + ".z) pixel_new.z = (pixel" + c + ").z*(float)" + (0 - convolvLis[c][2]) + ".0);\n";
                        }

                        if (testSrcColor != -1) KernelString += "    }\n";
                    }

                    if (normalize)
                        KernelString += "    pixel_new.w = 255;\n";
                }

                if (secondaryInputBuffers[k] != -2)
                {
                    KernelString += "    float4 pixel_2ndary=read_imagef(inputImage2,sampler,(int2)(i,j));\n";
                    if (secondaryInputFunc[k].Equals("^+"))
                    {
                        KernelString += "    pixel_new.x = sqrt((float)(pixel_new.x*pixel_new.x+pixel_2ndary.x*pixel_2ndary.x));\n" +
                                        "    pixel_new.y = sqrt((float)(pixel_new.y*pixel_new.y+pixel_2ndary.y*pixel_2ndary.y));\n" +
                                        "    pixel_new.z = sqrt((float)(pixel_new.z*pixel_new.z+pixel_2ndary.z*pixel_2ndary.z));\n";
                    }
                    else if (secondaryInputFunc[k].Equals("+"))
                    {
                        KernelString += "    pixel_new += pixel_2ndary;\n";
                        //KernelString += "    pixel_new.x += pixel_2ndary.x;\n";
                        //KernelString += "    pixel_new.y += pixel_2ndary.y;\n";
                        //KernelString += "    pixel_new.z += pixel_2ndary.z;\n";

                    }
                    else if (secondaryInputFunc[k].Equals("-"))
                    {
                        KernelString += "    pixel_new = pixel_new - pixel_2ndary;\n";
                    }
                    else if (secondaryInputFunc[k].Equals("a-"))
                    {
                        KernelString += "    pixel_new = fabs(pixel_2ndary - pixel_new);\n";
                        /*KernelString += "    pixel_new.x = (long)abs(pixel_2ndary.x - pixel_new.x);\n";
                        KernelString += "    pixel_new.y = (long)abs(pixel_2ndary.y - pixel_new.y);\n";
                        KernelString += "    pixel_new.w = (long)abs(pixel_2ndary.w - pixel_new.w);\n";*/
                    }
                    else if (secondaryInputFunc[k].Equals("*"))
                    {
                        KernelString += "    pixel_new = pixel_new * pixel_2ndary;\n";
                    }
                    else if (secondaryInputFunc[k].Equals("/"))
                    {
                        KernelString += "    pixel_new = pixel_new / pixel_2ndary;\n";
                    }
                }

                // Do an ABS after last convolution
                if (k == kernelFormulas.Length - 1)
                {
                    KernelString += "    pixel_new = clamp(fabs(pixel_new), 0.0f, 255.0f);\n";
                    //KernelString += "    if (pixel_new.x<0.0) pixel_new.x = 0.0-pixel_new.x; ;\n";
                    //KernelString += "    if (pixel_new.x>255) pixel_new.x = 255;\n";
                    //KernelString += "    if (pixel_new.y<0.0) pixel_new.y = 0.0-pixel_new.y; ;\n";
                    //KernelString += "    if (pixel_new.y>255) pixel_new.y = 255;\n";
                    //KernelString += "    if (pixel_new.z<0.0) pixel_new.z = 0.0-pixel_new.z; ;\n";
                    //KernelString += "    if (pixel_new.z>255) pixel_new.z = 255;\n";
                }
                //KernelString += "    pixel_new.w = 255;\n";

                if (testDestColor != -1)
                {
                    string compX = ((destColorCheck.Length == 4) ? destColorCheck[0] : "pixel_orig.x");
                    string compY = ((destColorCheck.Length == 4) ? destColorCheck[1] : "pixel_orig.y");
                    string compZ = ((destColorCheck.Length == 4) ? destColorCheck[2] : "pixel_orig.z");
                    string compW = ((destColorCheck.Length == 4) ? destColorCheck[3] : "pixel_orig.w");

                    if (testDestColor == 1) // AbsDiffThrsh
                    {
                        KernelString += "    float pixel_dest_diffR = fabs(pixel_new.x- (float)" + compX + ");\n"
                                 + "    float pixel_dest_diffG = fabs(pixel_new.y- (float)" + compY + ");\n"
                                 + "    float pixel_dest_diffB = fabs(pixel_new.z- (float)" + compZ + ");\n"
                                 //+ "    float pixel_dest_diffA = fabs(pixel_new.w- (float)" + compW + ");\n"
                                 + "    if ((pixel_dest_diffR<" + destColorCheckLimits[0] + ")&&(pixel_dest_diffG<" + destColorCheckLimits[1] + ")&&(pixel_dest_diffB<" + destColorCheckLimits[2] + /*" )&&(pixel_dest_diffA<" + destColorCheckLimits[3] +*/ ")){\n\t";
                    }
                    else if (testDestColor == 2) // PrcntDiffThrsh
                    {
                        KernelString += "    float prcnt_pixel_dest_diffR = fabs((pixel_new.x-(float)" + compX + ")/" + compX + ")*100;\n"
                                        + "    float prcnt_pixel_dest_diffG = fabs((pixel_new.y-(float)" + compY + ")/" + compY + ")*100;\n"
                                        + "    float prcnt_pixel_dest_diffB = fabs((pixel_new.z-(float)" + compZ + ")/" + compZ + ")*100;\n"
                                        //+ "    float prcnt_pixel_dest_diffA = fabs((pixel_new.w-(float)" + compW + ")/" + compW + ")*100;\n"
                                        + "    if ((prcnt_pixel_dest_diffR<" + destColorCheckLimits[0] + ")&&(prcnt_pixel_dest_diffG<" + destColorCheckLimits[1] + ")&&(prcnt_pixel_dest_diffB<" + destColorCheckLimits[2] + /*" )&&(prcnt_pixel_dest_diffA<" + destColorCheckLimits[3] +*/ ")){\n\t";
                    }
                }

                //KernelString += "    write_imagef(outputImage,(int2)(i,j), pixel_new" + ");\n";
                
                if (k == kernelFormulas.Length - 1)
                {
                    KernelString += "    final_output[4*i  +j*get_image_width(outputImage)*4] =  (char) pixel_new.z;\n";
                    KernelString += "    final_output[4*i+1+j*get_image_width(outputImage)*4] =  (char) pixel_new.y;\n";
                    KernelString += "    final_output[4*i+2+j*get_image_width(outputImage)*4] =  (char) pixel_new.x;\n";
                    KernelString += "    final_output[4*i+3+j*get_image_width(outputImage)*4] =  (char) 255;\n";
                }
                else
                {
                    KernelString += "    write_imagef(outputImage,(int2)(i,j),  (float4) {pixel_new.xyz, 255});\n";
                }


                if (testDestColor != -1)
                {
                    if (k == kernelFormulas.Length - 1)
                    {
                        KernelString += "    }\n    else\n    {\n   ";
                        KernelString += "    final_output[4*i  +j*get_image_width(outputImage)*4] =  (char) pixel_orig.z;\n";
                        KernelString += "    final_output[4*i+1+j*get_image_width(outputImage)*4] =  (char) pixel_orig.y;\n";
                        KernelString += "    final_output[4*i+2+j*get_image_width(outputImage)*4] =  (char) pixel_orig.x;\n";
                        KernelString += "    final_output[4*i+3+j*get_image_width(outputImage)*4] =  (char) 255;\n   }\n";
                    }
                    else
                    {
                        KernelString += "    }\n    else\n    {\n        write_imagef(outputImage,(int2)(i,j), pixel_orig);\n    }\n";
                    }


                }

                /*
                    "    uint4 pixel_new = pixel_orig*4;accumWeights+=4;\n" +
                    "    uint4 pixela = read_imageui(inputImage,sampler,(int2)(i,j+1));\n" +
                    "    uint4 pixelb = read_imageui(inputImage,sampler,(int2)(i,j-1));\n" +
                    "    uint4 pixelc = read_imageui(inputImage,sampler,(int2)(i+1,j));\n" +
                    "    uint4 pixeld = read_imageui(inputImage,sampler,(int2)(i-1,j));\n" +
                    "    pixel_new+=pixela;accumWeights++;\n" +
                    "    uint4 pixelb_diff = (pixelb-pixel_orig>0)?pixelb-pixel_orig:pixel_orig-pixelb; if ((pixela_diff.x<60)&& (pixelb_diff.y<60 )&&(pixelb_diff.z<60 )) {pixel_new+=pixelb;accumWeights++;}\n" +
                    "    uint4 pixelc_diff = (pixelc-pixel_orig>0)?pixelc-pixel_orig:pixel_orig-pixelc; if ((pixela_diff.x<60)&& (pixelc_diff.y<60 )&&(pixelc_diff.z<60 )) {pixel_new+=pixelc;accumWeights++;}\n" +
                    "    uint4 pixeld_diff = (pixeld-pixel_orig>0)?pixeld-pixel_orig:pixel_orig-pixeld; if ((pixela_diff.x<60)&& (pixeld_diff.y<60 )&&(pixeld_diff.z<60 )) {pixel_new+=pixeld;accumWeights++;}\n" +
                    "    write_imageui(outputImage,(int2)(i,j), pixel_new/accumWeights);\n";*/


                //If condition not met, write orignal pixel value
                if (testOrigColor != -1)
                {
                    if (k == kernelFormulas.Length - 1)
                    {
                        KernelString += "    }\n    else\n    {\n   ";
                        KernelString += "    final_output[4*i  +j*get_image_width(outputImage)*4] =  (char) pixel_orig.z;\n";
                        KernelString += "    final_output[4*i+1+j*get_image_width(outputImage)*4] =  (char) pixel_orig.y;\n";
                        KernelString += "    final_output[4*i+2+j*get_image_width(outputImage)*4] =  (char) pixel_orig.x;\n";
                        KernelString += "    final_output[4*i+3+j*get_image_width(outputImage)*4] =  (char) 255;\n    }\n";
                    }
                    else
                    {
                        KernelString += "    }\n    else\n    {\n        write_imagef(outputImage,(int2)(i,j), pixel_orig);\n    }\n";
                    }

                   
                }

                KernelString += "};\n";

                //Dump kernel code
                /*string currentKernelFile = "";
                currentKernelFile = System.IO.Path.GetTempFileName();
                System.IO.StreamWriter newFile = new System.IO.StreamWriter(currentKernelFile, false);
                newFile.WriteLine(KernelString);
                newFile.Close();
                Process.Start("wordpad.exe", currentKernelFile);*/

                //currentOutputBMPFile = System.IO.Path.GetTempFileName() + ".bmp";

                KernStrngs.Add(KernelString);

                numKernels++;
            }
        }        

        public override bool OnCreateOptionsMenu (IMenu menu)
		{
			MenuInflater.Inflate (Resource.Menu.main_menu, menu);
			return base.OnCreateOptionsMenu (menu);
		}

		protected override void OnDestroy ()
		{
			base.OnDestroy();
            unregisterScreenStatusReceiver();

            // Need to call clean-up
            mAttacher.Cleanup();
            this.Window.ClearFlags(WindowManagerFlags.KeepScreenOn);
        }



        public override bool OnPrepareOptionsMenu (IMenu menu)
		{
			IMenuItem zoomToggle = menu.FindItem(Resource.Id.menu_zoom_toggle);
			zoomToggle.SetTitle(mAttacher.CanZoom() ? "menu_zoom_disable" : "menu_zoom_enable");
			return base.OnPrepareOptionsMenu (menu);
		}

		public override bool OnOptionsItemSelected (IMenuItem item)
		{
			switch (item.ItemId) {
			case Resource.Id.menu_zoom_toggle:
				mAttacher.SetZoomable(!mAttacher.CanZoom());
				return true;

			case Resource.Id.menu_scale_fit_center:
				mAttacher.SetScaleType(Android.Widget.ImageView.ScaleType.FitCenter);
				return true;

			case Resource.Id.menu_scale_fit_start:
				mAttacher.SetScaleType(Android.Widget.ImageView.ScaleType.FitStart);
				return true;

			case Resource.Id.menu_scale_fit_end:
				mAttacher.SetScaleType(Android.Widget.ImageView.ScaleType.FitEnd);
				return true;

			case Resource.Id.menu_scale_fit_xy:
				mAttacher.SetScaleType(Android.Widget.ImageView.ScaleType.FitXy);
				return true;

			case Resource.Id.menu_scale_scale_center:
				mAttacher.SetScaleType(Android.Widget.ImageView.ScaleType.Center);
				return true;

			case Resource.Id.menu_scale_scale_center_crop:
				mAttacher.SetScaleType(Android.Widget.ImageView.ScaleType.CenterCrop);
				return true;

			case Resource.Id.menu_scale_scale_center_inside:
				mAttacher.SetScaleType(Android.Widget.ImageView.ScaleType.CenterInside);
				return true;

			case Resource.Id.menu_scale_random_animate:
			case Resource.Id.menu_scale_random:
				Random r = new Random();

				float minScale = mAttacher.GetMinimumScale();
				float maxScale = mAttacher.GetMaximumScale();
				float randomScale = minScale + (float.Parse(r.NextDouble().ToString()) * (maxScale - minScale));
				mAttacher.SetScale(randomScale, item.ItemId == Resource.Id.menu_scale_random_animate);

				ShowToast(SCALE_TOAST_STRING+" "+randomScale);

				return true;
			case Resource.Id.menu_matrix_restore:
				if (mCurrentDisplayMatrix == null)
					ShowToast("You need to capture display matrix first");
				else
					mAttacher.SetDisplayMatrix(mCurrentDisplayMatrix);
				return true;
			case Resource.Id.menu_matrix_capture:
				mCurrentDisplayMatrix = mAttacher.GetDisplayMatrix();
				return true;
			case Resource.Id.extract_visible_bitmap:
				return true;
			}
			return base.OnOptionsItemSelected (item);
		}
		private class MatrixChangeListener:Blurate.PhotoViewAttacher.IOnMatrixChangedListener
		{
			MainActivity context;
			#region IOnMatrixChangedListener implementation
			public MatrixChangeListener(MainActivity context)
			{
				this.context=context;
			}
			public void OnMatrixChanged (RectF rect)
			{
				context.mCurrMatrixTv.Text=(rect.ToString());
			}

			#endregion


		}
		private class PhotoTapListener : Blurate.PhotoViewAttacher.IOnPhotoTapListener {
			MainActivity context;
			#region IOnPhotoTapListener implementation
			public PhotoTapListener(MainActivity context){
				this.context=context;
			}
			public void OnPhotoTap (View view, float x, float y)
			{
				float xPercentage = x * 100f;
				float yPercentage = y * 100f;

				//context.ShowToast(PHOTO_TAP_TOAST_STRING+" "+ xPercentage+" "+yPercentage+" "+ view == null ? "0" : view.Id.ToString());
			}

			#endregion

		}
		public void ShowToast(String text) {
			if (null != mCurrentToast) {
				mCurrentToast.Cancel();
			}

			mCurrentToast = Toast.MakeText(this, text, ToastLength.Short);
			mCurrentToast.Show();
		}

	}
}


