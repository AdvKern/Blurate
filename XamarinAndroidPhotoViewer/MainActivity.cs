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

    [Activity (Label = "Blurate", MainLauncher = true, AlwaysRetainTaskState = true, Icon = "@drawable/icon", ConfigurationChanges = Android.Content.PM.ConfigChanges.ScreenLayout |  Android.Content.PM.ConfigChanges.ScreenSize | Android.Content.PM.ConfigChanges.Orientation)]
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
        static Android.Hardware.Camera curCamera = null;
        static uint crntCamId = 0;
        static bool cameraLive = false;

        static ClooBuffer<byte> last_yuv;
        //static ClooBuffer<float> last_rgb;
        static ClooBuffer<int> last_yuv_sizes;
        static int[] last_yuv_sizesV = new int[] { 0, 0 };
        static public float[] decodeYUV420SP(byte[] yuv420sp, float[] rgba, int width,
                              int height)
        {


            if (_selectedDevice != null)
            {

                if (_yuv_rgb_kernel == null)
                {
                    string KernelString_ = "__kernel void specialKernel0" + "(__global char* yuv420sp, __global float* rgba, __global int* size)\n" +
                        "{\n" +
                        "    int j = get_global_id(0);\n" +
                        "    int width = size[0];\n" +
                        "    int height = size[1];\n" +
                        "    int frameSize = width * height;\n" +
                        "    int r, g, b, y1192, y, i, uvp, u, v;\n" +

                    //"    for (int j = 0, yp = 0; j < height; j++)\n" +
                    "{\n" +
                        "uvp = frameSize + (j >> 1) * width;\n" +
                        "u = 0;\n" +
                        "v = 0;\n" +
                        "int yp = j*width;\n" +
                        "for (i = 0; i < width; i++, yp++)\n" +
                        "{\n" +
                            "y = (0xff & ((int)yuv420sp[yp])) - 16;\n" +
                            "if (y < 0)\n" +
                                "y = 0;\n" +
                            "if ((i & 1) == 0)\n" +
                            "{\n" +
                                "v = (0xff & yuv420sp[uvp++]) - 128;\n" +
                                "u = (0xff & yuv420sp[uvp++]) - 128;\n" +
                            "}\n" +

                            "y1192 = 1192 * y;\n" +
                            "r = (y1192 + 1634 * v);\n" +
                            "g = (y1192 - 833 * v - 400 * u);\n" +
                            "b = (y1192 + 2066 * u);\n" +

                            // Java's functions are faster then 'IFs'
                            //"r = max(0, min(r, 262143));\n" +
                            //"g = max(0, min(g, 262143));\n" +
                            //"b = max(0, min(b, 262143));\n" +

                            // rgb[yp] = 0xff000000 | ((r << 6) & 0xff0000) | ((g >> 2) &
                            // 0xff00) | ((b >> 10) & 0xff);
                            // rgba, divide 2^10 ( >> 10)
                            "rgba[yp * 4 + 0] = (256.0f * r) / 262143.0f;\n" +
                            "rgba[yp * 4 + 1] = (256.0f * g) / 262143.0f;\n" +
                            "rgba[yp * 4 + 2] = (256.0f * b) / 262143.0f;\n" +
                            "rgba[yp * 4 + 3] = (float)(255);\n" +
                        "}\n" +
                    "}\n" +
                    "};";

                    string KernelString = "__kernel void specialKernel0" + "(__global char* yuv420sp, __write_only image2d_t  rgba, __global int* size)\n" +
                        "{\n" +
                        "    int j = get_global_id(0);\n" +
                        "    int i = get_global_id(1);\n" +
                        "    int width = size[0];\n" +
                        "    int height = size[1];\n" +
                        "    int frameSize = width * height;\n" +
                        "    int r, g, b, y1192, y, uvp, u, v;\n" +

                    //"    for (int j = 0, yp = 0; j < height; j++)\n" +
                    "{\n" +
                        "uvp = frameSize + (j >> 1) * width + i + (i%2);\n" +
                        "u = 0;\n" +
                        "v = 0;\n" +
                        "int yp = j*width+i;\n" +
                        //"for (int i = 0; i < width; i++, yp++)\n" +
                        "{\n" +
                            "y = (0xff & ((int)yuv420sp[yp])) - 16;\n" +
                            "if (y < 0)\n" +
                                "y = 0;\n" +
                            "if ((i & 1) == 0)\n" +
                            "{\n" +
                                "v = (0xff & yuv420sp[uvp++]) - 128;\n" +
                                "u = (0xff & yuv420sp[uvp++]) - 128;\n" +
                            "}\n" +

                            "y1192 = 1192 * y;\n" +
                            "r = (y1192 + 1634 * v);\n" +
                            "g = (y1192 - 833 * v - 400 * u);\n" +
                            "b = (y1192 + 2066 * u);\n" +

                            // Java's functions are faster then 'IFs'
                            //"r = max(0, min(r, 262143));\n" +
                            //"g = max(0, min(g, 262143));\n" +
                            //"b = max(0, min(b, 262143));\n" +

                            // rgb[yp] = 0xff000000 | ((r << 6) & 0xff0000) | ((g >> 2) &
                            // 0xff00) | ((b >> 10) & 0xff);
                            // rgba, divide 2^10 ( >> 10)
                            /*"rgba[yp ] =(float4)((256.0f * r) / 262143.0f,\n" +
                            "  (256.0f * g) / 262143.0f,\n" +
                            "  (256.0f * b) / 262143.0f,\n" +
                            "  (float)(255));\n" +*/
                            "    write_imagef(rgba ,(int2)(i,j), (float4)((256.0f * r) / 262143.0f,(256.0f * g) / 262143.0f, (256.0f * b) / 262143.0f, 255.0f));\n" +

                        "}\n" +
                    "}\n" +
                    "};";

                    List<string> _yuv_kernel = new List<string>();
                    _yuv_kernel.Add(KernelString);
                    _yuv_rgb_kernel = ClooProgramViolaJones.CreateSpecial(_context, _yuv_kernel);
                }
                if ((last_yuv_sizesV[0] != width) || (last_yuv_sizesV[1] != height))
                {
                    last_yuv_sizesV[0] = width;
                    last_yuv_sizesV[1] = height;
                    last_yuv = new ClooBuffer<byte>(_context, ComputeMemoryFlags.ReadOnly | ComputeMemoryFlags.UseHostPointer, yuv420sp);
                    //last_rgb = new ClooBuffer<float>(_context, ComputeMemoryFlags.WriteOnly | ComputeMemoryFlags.UseHostPointer, rgba);
                    last_yuv_sizes = new ClooBuffer<int>(_context, ComputeMemoryFlags.ReadOnly | ComputeMemoryFlags.UseHostPointer, last_yuv_sizesV);
                    _yuv_rgb_kernel.yuvtorgb(_queue, last_yuv_sizesV, last_yuv, _clooImageByteOriginal, last_yuv_sizes);
                    //last_rgb.ReadFromDevice(_queue);
                }
                else
                {
                    last_yuv.WriteToDevice(_queue, yuv420sp);// = new ClooBuffer<byte>(_context, ComputeMemoryFlags.ReadOnly | ComputeMemoryFlags.UseHostPointer, yuv420sp);
                    _yuv_rgb_kernel.yuvtorgb(_queue, last_yuv_sizesV, last_yuv, _clooImageByteOriginal, last_yuv_sizes);
                    //last_rgb.ReadFromDevice(_queue);
                }

            }
            else
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
                        rgba[yp * 4 + 0] = (256.0f * r) / 262143.0f;
                        rgba[yp * 4 + 1] = (256.0f * g) / 262143.0f;
                        rgba[yp * 4 + 2] = (256.0f * b) / 262143.0f;
                        rgba[yp * 4 + 3] = (float)(255);
                    }
                }
            }
            return rgba;
        }

        static public void decodeYUV420SP_(byte[] yuv420sp, float[] rgb, int width, int height)
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

                    rgb[yp * 4 + 0] = (256.0f * r) / 262143.0f;
                    rgb[yp * 4 + 1] = (256.0f * g) / 262143.0f;
                    rgb[yp * 4 + 2] = (256.0f * b) / 262143.0f;
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

        private byte[] ConvertYuvToJpeg(byte[] yuvData, Android.Hardware.Camera thisCamera)
        {
            var cameraParameters = thisCamera.GetParameters();
            var width = cameraParameters.PreviewSize.Width;
            var height = cameraParameters.PreviewSize.Height;
            var yuv = new YuvImage(yuvData, cameraParameters.PreviewFormat, width, height, null);
            var ms = new System.IO.MemoryStream();
            var quality = 80;   // adjust this as needed
            yuv.CompressToJpeg(new Rect(0, 0, width, height), quality, ms);
            var jpegData = ms.ToArray();

            return jpegData;
        }

        public static Bitmap bytesToBitmap(byte[] imageBytes)
        {
            Bitmap bitmap = BitmapFactory.DecodeByteArray(imageBytes, 0, imageBytes.Length);

            return bitmap;
        }

        static long prevRuntime = 12345;
        float[] vidData;
        byte[] lastYuvData;
        int lastOrient = 1;
        int vidWidth=0;
        int vidHight=0;
        SurfaceOrientation vidRotation = 0;
        static bool terminatCam = false;
        static bool restartedCamPend = false;
        static System.ComponentModel.BackgroundWorker _capThread=null;
        void Android.Hardware.Camera.IPreviewCallback.OnPreviewFrame(byte[] data, Android.Hardware.Camera thisCamera)
        {

            if (((_capThread != null) && _capThread.IsBusy) || (thisCamera == null))
            {
                thisCamera.AddCallbackBuffer(data);
                return;
            }


            curCamera = thisCamera;
            lastYuvData = data;
            LogManager.GetLogger().i("start", (SystemClock.ElapsedRealtime() - prevRuntime).ToString());

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
                    if (thisCamera != null) thisCamera.AddCallbackBuffer(data);
                    //if (args.Error != null)  // if an exception occurred during DoWork,
                    //    MessageBox.Show(args.Error.ToString());  // do your error handling here
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
                    result = (info.Orientation - degrees + 360) % 360;
                    int nowOrient = -1;
                    if (info.Facing == Android.Hardware.CameraFacing.Front)
                    {
                        nowOrient = (result == 0) ? 4 : 
                                    (result == 90) ? 5 :
                                        (result == 180) ? 2 :
                                        (result == 180) ? 2 :
                                        (result == 270) ? 7 : 1;
                    }
                    else
                    {  // back-facing
                        nowOrient = (result == 90) ? 6 :
                                        (result == 180) ? 3 :
                                        (result == 180) ? 3 :
                                        (result == 270) ? 8 : 1;
                    }
                    //camera.SetDisplayOrientation(result + 90);


                    lastVidBitmapImage = _bitmapImage1 = new BitmapDrawable(rotateBitmap(curImg, lastOrient));
                    //lastVidBitmapImage = _bitmapImage1 = new BitmapDrawable(curImg);

                    if (curCamera != null)
                    {
                        if (nowOrient != lastOrient)
                        {
                            lastOrient = nowOrient;
                            mImageView.SetPhotoViewRotation(lastOrient);
                        }
                        mImageView.SetImageDrawable(lastVidBitmapImage);
                    }
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
            LogManager.GetLogger().i("CAMPROC", 0.ToString());

            var cameraParameters = curCamera.GetParameters();
            bool imgResize = ((vidWidth != cameraParameters.PreviewSize.Width ||
            (vidHight != cameraParameters.PreviewSize.Height))); // TODO: can be done more efficently

            LogManager.GetLogger().i("CAMPROC-s", (SystemClock.ElapsedRealtime() - prevRuntime).ToString());

            for (int i=0; i< cameraParameters.SupportedPreviewFormats.Count; i++)
            { LogManager.GetLogger().i("preview"+i, cameraParameters.SupportedPreviewFormats[i].ToString()); }

            vidWidth = cameraParameters.PreviewSize.Width;
            vidHight = cameraParameters.PreviewSize.Height;
            vidRotation = this.WindowManager.DefaultDisplay.Rotation;
            if (imgResize)
            {
                if (_selectedDevice == null)
                {
                    vidData = new float[vidWidth * vidHight * 4];
                    //flushAlpha(vidData, vidWidth, vidHight);
                }
                _clooImageByteOriginal = ClooImage2DFloatRgbA.CreateHostNoAccess(_context, ComputeMemoryFlags.ReadWrite, vidWidth, vidHight);

            }
            LogManager.GetLogger().i("CAMPROC-y", (SystemClock.ElapsedRealtime() - prevRuntime).ToString());
            decodeYUV420SP(lastYuvData, vidData, vidWidth, vidHight);
            LogManager.GetLogger().i("CAMPROC-r", (SystemClock.ElapsedRealtime() - prevRuntime).ToString());

            Android.Graphics.Bitmap mBitmap = null;
            if (_selectedDevice == null)
            {
                //await _TesseractApi.SetImage(data); /// this hangs                
                //string text = _Api.Text;
                //string msg = string.Format("No Input Specified");
                //Toast.MakeText(this, msg, ToastLength.Long).Show();

                byte[] jpegData = ConvertYuvToJpeg(lastYuvData, curCamera);
                
                //LogManager.GetLogger().i("CAMPROC-0", (SystemClock.ElapsedRealtime() - prevRuntime).ToString());
                mBitmap = bytesToBitmap(jpegData);

                    curImg = mBitmap;
                    return;

                //LogManager.GetLogger().i("CAMPROC-1",  (SystemClock.ElapsedRealtime() - prevRuntime).ToString());
            }

            
            var imageView =
                    FindViewById<ImageView>(Resource.Id.iv_photo);

            try
            {


                //exif = new Android.Media.ExifInterface(GetPathToImage(data.Data));
                //int orientation = exif.GetAttributeInt(Android.Media.ExifInterface.TagOrientation, 0);
                //mBitmap = rotateBitmap(mBitmap, 90);
                // = currentImage = new BitmapDrawable(Resources, mBitmap);

                

                LogManager.GetLogger().i("CAMPROC-a", (SystemClock.ElapsedRealtime() - prevRuntime).ToString());
                //if (orientation != -1) 
                if (_selectedDevice != null)
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

        void Android.Hardware.Camera.IPictureCallback.OnPictureTaken(byte[] data, Android.Hardware.Camera thisCamera)
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

        public static System.Timers.Timer shutdownTimer=null;
        public void SurfaceDestroyed(ISurfaceHolder holder)
        {
            try
            {
                curCamera.Unlock();
                curCamera.StopPreview();
                ((SurfaceView)FindViewById(Resource.Id.iv_surf)).Holder.RemoveCallback(this);
                holder.RemoveCallback(this);
                curCamera.SetPreviewCallback(null);
                curCamera.Reconnect(); // these are needed to correctly release
            }
            catch { }
            try { curCamera.Release(); } catch { }
            curCamera = null;
            //System.Threading.Thread.Sleep(500); // was crashing without this on retoating camera

            //cameraLive = false;
            //shutdown(null, null);
            /*if (shutdownTimer == null)
            {
                shutdownTimer = new System.Timers.Timer();
            }
            else {
                shutdownTimer.Stop();
            }
            shutdownTimer.Interval = 3000;
            shutdownTimer.Elapsed += new System.Timers.ElapsedEventHandler(shutdown);
            shutdownTimer.Start();*/
            //((SurfaceView)FindViewById(Resource.Id.iv_surf)).Holder.

            //SurfaceView mSurfaceView = new SurfaceView(Android.App.Application.Context);
            //mSurfaceView.Holder.AddCallback(this);

        }

        bool restartTimer = false;
        protected void shutdown(object sender, System.Timers.ElapsedEventArgs e)
        {
            /*shutdownTimer.Stop();
            if (!restartTimer) return;
            restartTimer = false;
            startCam();*/
            /*restartedCamPend = false;

            //holder.RemoveCallback(this);
            curCamera.SetPreviewCallback(null);
            ((SurfaceView)FindViewById(Resource.Id.iv_surf)).Holder.RemoveCallback(this);
            curCamera.Release();
            curCamera = null;
            //System.Threading.Thread.Sleep(500); // was crashing without this on retoating camera
            terminatCam = false;
            if (_capThread != null)
            {
                System.ComponentModel.BackgroundWorker lastCapThread = _capThread;
                _capThread = null;
                lastCapThread.CancelAsync();
                lastCapThread.Dispose();
            }*/
        }

        public void SurfaceChanged(ISurfaceHolder holder, Android.Graphics.Format f, int i, int j)
        {
            if (holder.Surface == null)
            {
                return;
            }

            try
            {
                curCamera.StopPreview();
            }
            catch (Exception e)
            {
                // ignore: tried to stop a non-existent preview
            }

            try
            {
                Android.Hardware.Camera.Parameters p = curCamera.GetParameters();
                p.PictureFormat = Android.Graphics.ImageFormatType.Jpeg;
                if (p.SupportedFocusModes.Contains(
                    Android.Hardware.Camera.Parameters.FocusModeContinuousPicture))
                {
                    p.FocusMode = Android.Hardware.Camera.Parameters.FocusModeContinuousPicture;
                }
                curCamera.SetParameters(p);
                curCamera.SetPreviewCallbackWithBuffer(this);
                camBuff1 = new byte[p.PreviewSize.Width * p.PreviewSize.Width * ImageFormat.GetBitsPerPixel(p.PreviewFormat)/8];
                //camBuff2 = new byte[p.PreviewSize.Width * p.PreviewSize.Width * ImageFormat.GetBitsPerPixel(p.PreviewFormat)/8];
                curCamera.AddCallbackBuffer(camBuff1);
                //curCamera.AddCallbackBuffer(camBuff2);
                curCamera.SetPreviewDisplay(holder);
                curCamera.StartPreview();
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
        static ClooProgramViolaJones _yuv_rgb_kernel;
        static ClooProgramViolaJones _specialkernel;

        ClooSampler _sampler;
        ClooBuffer<uint> _histogram;
        static ClooBuffer<byte> final_output;
        static byte[] final_out_host;
        static System.Runtime.InteropServices.GCHandle final_out_ptr;
        static ClooImage2DFloatRgbA _clooImageByteOriginal;
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
        static BitmapDrawable lastVidBitmapImage;
        static List<string> KernStrngs = new List<string>();
        static List<string> VisibDeviceNameCombobox = new List<string>();
        static List<int> kernelInputBuffers = new List<int>();
        static int[] secondaryInputBuffers = null;
        static string[] secondaryInputFunc = null;
        // nesting : [] -> () -> {} -> <>
        static int numKernels = 0;

        private string GetPathToImage2(Android.Net.Uri uri)
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

        readonly string[] PermissionRead =
        {
            Android.Manifest.Permission.ReadExternalStorage
        };
        private string GetPathToImage(Android.Net.Uri uri)
        {
            if ((int)Build.VERSION.SdkInt < 23)
                return GetPathToImage2(uri);
            else // need to request permission
            {
                const string permission = Android.Manifest.Permission.ReadExternalStorage;
                if (CheckSelfPermission(permission) == (int)Android.Content.PM.Permission.Granted)
                {
                    return GetPathToImage2(uri);
                }
                else
                {
                    RequestPermissions(PermissionRead, requestReadExId);
                }
            }

            return "";
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
                Bitmap bmRotated = Bitmap.CreateBitmap(bitmap, 0, 0, bitmap.Width, bitmap.Height, matrix, false);
                //bitmap.Recycle();
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

                    newBitmap = Android.Provider.MediaStore.Images.Media.GetBitmap(this.ContentResolver, data.Data);
                    newImgData = data.Data;
                    newImgPath = GetPathToImage(newImgData);

                    if (newImgPath.Length==0) return; // let loading happen after permission granted

                    rotatAndDisplay();
                }
                catch
                {
                    imageView.SetImageURI(data.Data);
                }
            }
        }

        Android.Graphics.Bitmap newBitmap = null;
        string newImgPath = "";
        Android.Net.Uri newImgData = null;
        public void rotatAndDisplay()
        {
            Android.Media.ExifInterface exif = null;
            try
            {
                exif = new Android.Media.ExifInterface(newImgPath);
                int orientation = exif.GetAttributeInt(Android.Media.ExifInterface.TagOrientation, 0);
                newBitmap = rotateBitmap(newBitmap, orientation);
            }
            catch (IOException e)
            {
                //e.printStackTrace();
            }
            finally
            {
                currentOrigImage = currentImage = new BitmapDrawable(Resources, newBitmap);
            }



            //System.ComponentModel.BackgroundWorker _filterThread = new System.ComponentModel.BackgroundWorker();
            //_filterThread.WorkerSupportsCancellation = true;
            //_filterThread.DoWork += (sender, args) =>
            {
                prevRuntime = SystemClock.ElapsedRealtime();
                ApplyFilter();
            };
            //_filterThread.RunWorkerCompleted += (sender, args) =>
            {
                //this.Window.AddFlags(WindowManagerFlags.KeepScreenOn);
                if (null != mAttacher)
                {
                    mAttacher.Update();
                }
            };
            //_filterThread.RunWorkerAsync();

            //if (orientation != -1) 
            //ApplyFilter();

            //mAttacher.Update();
            //mAttacher = new PhotoViewAttacher(mImageView);
            //mAttacher.SetOnMatrixChangeListener(new MatrixChangeListener(this));
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
            if (cameraLive) // camera enabled
            {
                if (curCamera != null)
                {
                    try
                    {
                        curCamera.Reconnect();
                        curCamera.StartPreview();
                    }
                    catch { startCam(); }
                }
                else
                {
                    startCam();
                }
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

        static System.ComponentModel.BackgroundWorker _cameraThread = null;
        public void startCam()
        {
            vidWidth = -1;
            vidHight = -1;
            if (_cameraThread == null)
            {
                /*if (_cameraThread.IsBusy)
                    return; // bullshit OnRestore

                _cameraThread.Dispose();
                _cameraThread = null;*/



                _cameraThread = new System.ComponentModel.BackgroundWorker();
                _cameraThread.WorkerSupportsCancellation = true;

                _cameraThread.DoWork += (sender, args) =>
                {
                    // do your lengthy stuff here -- this will happen in a separate thread
                    CamSetup();
                };
                _cameraThread.RunWorkerCompleted += (sender, args) =>
                {
                        this.Window.AddFlags(WindowManagerFlags.KeepScreenOn);
                };
            }
            try { _cameraThread.RunWorkerAsync(); } catch { }
            cambutton.SetBackgroundResource(/*Android.*/Resource.Drawable.rotCam3);
            sharebutton.Visibility = ViewStates.Gone;
            searchButton.Visibility = ViewStates.Gone;
            mAttacher.SetZoomable(false);
            snapButton.Visibility = ViewStates.Visible;
        }

        readonly string[] PermissionsCamera =
        {
            Android.Manifest.Permission.Camera,
            Android.Manifest.Permission.WriteExternalStorage
        };
        const int requestCamId = (int)5432543;
        const int requestWriteExId = (int)5432542;
        const int requestReadExId = (int)5432541;

        public void CamSetup()
        {
            //if (cameraLive) System.Threading.Thread.Sleep(500);
            for (int go= 0; go< 6; go++)
            {
                try
                {
                    if ((int)Build.VERSION.SdkInt < 23)
                        curCamera = Android.Hardware.Camera.Open((int)crntCamId);
                    else // need to request permission
                    {

                        const string permission = Android.Manifest.Permission.Camera;
                        if (CheckSelfPermission(permission) == (int)Android.Content.PM.Permission.Granted)
                        {
                            //Android.Hardware.Camera2.CameraManager cameraManager = ((Android.Hardware.Camera2.CameraManager)Context.GetSystemService(Context.CameraService));
                            //curCamera = cameraManager.OpenCamera(cameraManager.GetCameraIdList()[0], null, null);
                            curCamera = Android.Hardware.Camera.Open((int)crntCamId);
                        }
                        else
                        {
                            RequestPermissions(PermissionsCamera, requestCamId);
                            return;
                        }
                    }

                    startCam2();

                    break;
                }
                catch (Exception ex)
                {
                    //crntCamId++;
                    //base.OnCreate(null);

                    LogManager.GetLogger().i("Cam connect error:", ex.Message.ToString());
                    //System.Threading.Thread.Sleep(500);
                    //string msg = string.Format("error switching camera on (" + ex.Message + ")");
                    //Toast.MakeText(this, msg, ToastLength.Long).Show();
                    //Wait(5);
                }
            }
        }

        byte[] camBuff1;
        byte[] camBuff2;
        public void startCam2()
        {
            try
            {
                SurfaceView surface = (SurfaceView)FindViewById(Resource.Id.iv_surf);
                //surface.Visibility = ViewStates.Invisible;
                var holder = surface.Holder;
                holder.AddCallback(this);
                holder.SetType(Android.Views.SurfaceType.PushBuffers);

                curCamera.Lock();
                Android.Hardware.Camera.Parameters p = curCamera.GetParameters();
                p.PictureFormat = Android.Graphics.ImageFormatType.Jpeg;
                if (p.SupportedFocusModes.Contains(
                    Android.Hardware.Camera.Parameters.FocusModeContinuousPicture)) {
                    p.FocusMode = Android.Hardware.Camera.Parameters.FocusModeContinuousPicture;
                }
                curCamera.SetParameters(p);
                curCamera.SetPreviewCallbackWithBuffer(this);
                camBuff1 = new byte[p.PreviewSize.Width * p.PreviewSize.Width * ImageFormat.GetBitsPerPixel(p.PreviewFormat) / 8];
                //camBuff2 = new byte[p.PreviewSize.Width * p.PreviewSize.Width * ImageFormat.GetBitsPerPixel(p.PreviewFormat) / 8];
                curCamera.AddCallbackBuffer(camBuff1);
                //curCamera.AddCallbackBuffer(camBuff2);
                curCamera.SetPreviewDisplay(holder);

                //int orientation = Exif.GetAttributeInt(Android.Media.ExifInterface.TagOrientation, 0);
                curCamera.StartPreview();
                cameraLive = true;
            }
            catch (IOException e)
            {
                //e.printStackTrace();
            }

        }

        bool snapIfPermissionGranted = false;
        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Android.Content.PM.Permission[] grantResults)
        {
            switch (requestCode)
            {
                case requestCamId:
                    {
                        if (grantResults[0] == Android.Content.PM.Permission.Granted)
                        {
                            //Permission granted to start Cam...
                            curCamera = Android.Hardware.Camera.Open((int)crntCamId);
                            startCam2();
                        }
                        else
                        {
                            //Permission Denied :(
                            //Disabling location functionality
                            /*var snack = Snackbar.Make(layout, "Location permission is denied.", Snackbar.LengthShort);
                            snack.Show();*/
                        }
                    }
                    break;
                case requestWriteExId:
                    if (grantResults[0] == Android.Content.PM.Permission.Granted)
                        if (!cameraLive && snapIfPermissionGranted)
                            takeSnap();
                    snapIfPermissionGranted = false;
                    break;
                case requestReadExId:
                    if (grantResults[0] == Android.Content.PM.Permission.Granted)
                    {
                        newImgPath = GetPathToImage2(newImgData);
                        rotatAndDisplay();
                    }
                    break;
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
        static bool CameraActive = false;
        ImageButton searchButton = null;
        ImageButton sharebutton = null;
        ImageButton cambutton = null;
        ImageButton snapButton = null;
        ImageButton imptButton = null;
        protected override void OnCreate (Bundle bundle) // this fucker gets called on fucking rotates
		{
            var dllDirectory = @"/system/vendor/lib64/;/system/vendor/lib/;/system/vendor/lib64/egl/;/system/vendor/lib/egl;";
            System.Environment.SetEnvironmentVariable("PATH", System.Environment.GetEnvironmentVariable("PATH") + ";" + dllDirectory);

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


            ((SurfaceView)FindViewById(Resource.Id.iv_blank)).Visibility = ViewStates.Gone;
            //((SurfaceView)FindViewById(Resource.Id.camButton)).Visibility = ViewStates.Invisible; // don't show until initial draw complete


            //GC.Collect();
            if (last_index==-1)
                currentOrigImage = currentImage = Resources.GetDrawable(Resource.Drawable.Milkmaid);

            mImageView.SetImageDrawable(currentImage);

            if (mainKrnls.Length == 0)
            {
                if (System.IO.File.Exists(Android.App.Application.Context.FilesDir.AbsolutePath + @"/main.krnls"))
                {
                    mainKrnls = System.IO.File.ReadAllLines(Android.App.Application.Context.FilesDir.AbsolutePath + @"/main.krnls");
                }
                if ( /* !mainKrnls[0].StartsWith("HandSketch") | */ !System.IO.File.Exists(Android.App.Application.Context.FilesDir.AbsolutePath + @"/main.krnls"))
                {
                    mainKrnls = Resources.GetString(Resource.String.main_krnls).Split('\n');
                    //Toast.MakeText(this, msg, ToastLength.Long).Show();

                    System.IO.File.WriteAllLines(Android.App.Application.Context.FilesDir.AbsolutePath + @"/main.krnls", mainKrnls);
                }

            }

            Spinner spinner = FindViewById<Spinner>(Resource.Id.spinner1);
            spinner.ItemSelected += new EventHandler<AdapterView.ItemSelectedEventArgs>(spinner_ItemSelected);
            spinner.LongClick += (Ssender, args) =>
            {
                if (filt_list.Count < 2) return;
                AlertDialog.Builder alert = new AlertDialog.Builder(this);
                alert.SetTitle("Confirm deletion");
                alert.SetMessage("Are you sure you want to delete the " + spinner.SelectedItem.ToString() + " filter?");
                alert.SetPositiveButton("Delete", (senderAlert, argsAlert) => {
                    string nameRem = spinner.SelectedItem.ToString();

                if (filt_list.Contains(nameRem))
                {
                    filt_list.Remove(nameRem);
                }

                try
                {
                   KernelsList.Remove(nameRem);
                }
                catch {  }


                    var adapterN = new ArrayAdapter(this,
                            Android.Resource.Layout.SimpleSpinnerItem, filt_list.ToArray());

                    adapterN.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);
                    spinner.Adapter = adapterN;

                    string kernFilePath = Android.App.Application.Context.FilesDir.AbsolutePath + @"/main.krnls";
                    string[] curLines = System.IO.File.ReadAllLines(kernFilePath);
                    List<string> newLines = new List<string>();
                    for (int i = 0; i < curLines.Length; i++)
                        if ((curLines[i].Trim().Length > 0) && (!curLines[i].Trim().ToLower().StartsWith(nameRem.ToLower())))
                            newLines.Add(curLines[i].Trim());
                    System.IO.File.Delete(kernFilePath);
                    System.IO.File.WriteAllLines(kernFilePath, newLines.ToArray());
                    spinner.SetSelection(filt_list.Count - 1);
                    Toast.MakeText(this, "Deleted " + nameRem + "!", ToastLength.Short).Show();
                });

                alert.SetNegativeButton("Cancel", (senderAlert, argsAlert) => {
                    Toast.MakeText(this, "Cancelled Delete", ToastLength.Short).Show();
                });

                Dialog dialog = alert.Create();
                dialog.Show();
            };


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


            searchButton = FindViewById<ImageButton>(Resource.Id.myButton);
            snapButton = FindViewById<ImageButton>(Resource.Id.snapButton);
            imptButton = FindViewById<ImageButton>(Resource.Id.importButton);
            snapButton.Visibility = ViewStates.Gone;

            imptButton.Click += delegate
            {
                var uri = Android.Net.Uri.Parse("http://www.advancedkernels.com/blurate/filters");
                var intent = new Intent(Intent.ActionView, uri);
                StartActivity(intent);

                //Application.Context.StartActivity(intent);
                //WebBrowser.OpenPage(this, "http://www.advancedkernels.com/blurate/filters");
            };
            imptButton.LongClick += delegate
            {
                Intent intent = new Intent();
                intent.SetAction(Android.Content.Intent.ActionView);
                var data = Android.Net.Uri.Parse(Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryDownloads).AbsolutePath);
                String type = "*/*";
                intent.SetDataAndType(data, type);
                StartActivity(intent);
            };

            snapButton.Click += delegate
        {
            if ((int)Build.VERSION.SdkInt >= 23)
            {
                const string permission = Android.Manifest.Permission.WriteExternalStorage;
                if (CheckSelfPermission(permission) != (int)Android.Content.PM.Permission.Granted)
                {
                    if (cameraLive) OnBackPressed();// turn off camera while requesting permission
                    snapIfPermissionGranted = true;
                    RequestPermissions(new String[] { Android.Manifest.Permission.WriteExternalStorage }, requestWriteExId);
                    return;
                }
            }
            takeSnap();
        };

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

            };

            cambutton = FindViewById<ImageButton>(Resource.Id.camButton);

            cambutton.LayoutChange += delegate
            {
                /*ViewGroup.LayoutParams lparams = cambutton.LayoutParameters;
                lparams.Width = cambutton.Height;
                cambutton.LayoutParameters = lparams;*/
            };

            cambutton.Click += delegate
            {
                if ((!cameraLive) || (curCamera==null))
                {
                    startCam();
                }
                else
                {
                    crntCamId = (uint)((crntCamId + 1) % Android.Hardware.Camera.NumberOfCameras);
                    curCamera.StopPreview();
                    curCamera.SetPreviewCallback(null);
                    curCamera.Unlock();
                    try { curCamera.Reconnect(); }
                    catch
                    {
                        System.Threading.Thread.Sleep(500);
                    }
                    curCamera.Release();
                    curCamera.UnregisterFromRuntime();
                    curCamera = null;
                    ((SurfaceView)FindViewById(Resource.Id.iv_surf)).Holder.RemoveCallback(this);
                    //SurfaceDestroyed(((SurfaceView)FindViewById(Resource.Id.iv_surf)).Holder);

                    // Remove and re-Add SurfaceView
                    FrameLayout rootLayout = (FrameLayout)(FindViewById(Resource.Id.iv_surf)).Parent;
                    
                    rootLayout.RemoveView(FindViewById(Resource.Id.iv_surf));
                    SurfaceView surface = new SurfaceView(Android.App.Application.Context );
                    surface.Id = Resource.Id.iv_surf;
                    surface.Layout(10, 10, 10, 10);
                    rootLayout.AddView(surface, 1, 1);

                    //surfaceCreated(holder);
                    //surfaceChanged(holder, format, width, height);

                    if (_capThread != null)
                    {
                        _capThread.CancelAsync();
                        _capThread.Dispose();
                        _capThread = null;
                    }
                    if (_cameraThread != null)
                    {
                        _cameraThread.CancelAsync();
                        _cameraThread.Dispose();
                        _cameraThread = null;
                    }
                    //System.Threading.Thread.Sleep(500);
                    //cameraLive = false;
                    startCam();

                    /*if (shutdownTimer == null)
                    {
                        shutdownTimer = new System.Timers.Timer();
                    }
                    else
                    {
                        shutdownTimer.Stop();
                    }
                    restartTimer = true;
                    shutdownTimer.Interval = 300;
                    shutdownTimer.Elapsed += new System.Timers.ElapsedEventHandler(shutdown);
                    shutdownTimer.Start();*/

                    /*String message = "Text I want to share.";
                    Intent share = new Intent(Intent.ActionSend);*/
                }
            };

            sharebutton = FindViewById<ImageButton>(Resource.Id.myShareButton);

            //sharebutton.s = spinner.Height;
            //sharebutton.SetMinimumHeight(spinner.Height*2);
            //sharebutton.SetMinimumWidth(spinner.Height*2);

            //sharebutton.LayoutParameters = new RelativeLayout.LayoutParams(sharebutton.Height, sharebutton.Height);



            sharebutton.LayoutChange += delegate
            {
                squareButtons();
            };

            sharebutton.Click += delegate {
                /*String message = "Text I want to share.";
                Intent share = new Intent(Intent.ActionSend);
                share.SetType("image/jpeg");
                share.PutExtra(Intent.ExtraText, message);
                //share.PutExtra(Intent.ima, message);
                StartActivity(Intent.CreateChooser(share, "Title of the dialog the system will open"));*/
                File imagesFolder = new File(CacheDir, "images");
                imagesFolder.Mkdirs();
                File file = new File(imagesFolder, "blurateShare.jpg");
                Android.Net.Uri uri;
                
                try
                {
                    
                    using (var fOut = new System.IO.FileStream(file.Path, System.IO.FileMode.Create))
                    {
                        try
                        {
                            Bitmap currntBmp = drawableToBitmap(currentImage);
                            currntBmp.Compress(Bitmap.CompressFormat.Jpeg, 95, fOut);

                        }
                        catch (Exception ex)
                        {
                            ;
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

                    //uri = Android.Support.V4.Content.FileProvider.GetUriForFile(this, this.PackageName + ".provider", file);
                    //intent.PutExtra(Intent.ExtraStream, uri);
                    
                    //intent.SetDataAndType(uri, "image/jpeg");
                    intent.SetFlags(ActivityFlags.GrantReadUriPermission);

                    //string atribs = System.IO.File.GetAttributes(fileName).ToString();
                    //using (var fd = ContentResolver.OpenFileDescriptor(Android.Net.Uri.Parse(fileName), "r"))
                    //    atribs = fd.StatSize.ToString();

                    //if (atribs.Equals(""))
                    intent.SetType("image/jpeg");
                    StartActivityForResult(intent, 0);

                }
                catch (Exception e)
                {
                    try
                    {
                        Intent intent = new Intent(Android.Content.Intent.ActionSend);
                        intent.SetFlags(ActivityFlags.NewTask);
                        intent.AddFlags(ActivityFlags.GrantReadUriPermission);
                        intent.PutExtra(Intent.ExtraSubject, "");
                        intent.PutExtra(Intent.ExtraText, "#" + spinner.SelectedItem.ToString());
                        //intent.PutExtra(Intent.ExtraStream, Android.Net.Uri.FromFile(file));

                        uri = Android.Support.V4.Content.FileProvider.GetUriForFile(this, this.PackageName + ".provider", file);
                        intent.PutExtra(Intent.ExtraStream, uri);
                        //intent.SetDataAndType(uri, "image/jpeg");
                        intent.SetFlags(ActivityFlags.GrantReadUriPermission);

                        //string atribs = System.IO.File.GetAttributes(fileName).ToString();
                        //using (var fd = ContentResolver.OpenFileDescriptor(Android.Net.Uri.Parse(fileName), "r"))
                        //    atribs = fd.StatSize.ToString();

                        //if (atribs.Equals(""))
                        intent.SetType("image/jpeg");
                        StartActivityForResult(intent, 0);
                    }
                    catch
                    {
                    }
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

            if (cameraLive)
            {
                _cameraThread = null;
                _capThread = null;
            }
        }

        public void takeSnap()
        {

            ((SurfaceView)FindViewById(Resource.Id.iv_blank)).BringToFront();
            ((SurfaceView)FindViewById(Resource.Id.iv_blank)).Visibility = ViewStates.Visible;

            byte[] jpegData = ConvertYuvToJpeg(lastYuvData, curCamera);
            Bitmap mBitmap = bytesToBitmap(jpegData);
            currentOrigImage = new BitmapDrawable(Resources, rotateBitmap(mBitmap, lastOrient));
            currentImage = lastVidBitmapImage;

            System.ComponentModel.BackgroundWorker _snapThread = new System.ComponentModel.BackgroundWorker();
            _snapThread.WorkerSupportsCancellation = true;
            _snapThread.DoWork += (sender, args) =>
            {
                System.IO.FileStream fs = null;
                var documentsFolder = Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryPictures);

                Random Rndm = new Random((int)DateTime.Now.Ticks);
                var filename = System.IO.Path.Combine(documentsFolder.AbsolutePath, "blurate" + Rndm.Next().ToString() + ".jpg");
                try
                {
                    using (fs = new System.IO.FileStream(filename, System.IO.FileMode.Create))
                    {
                        lastVidBitmapImage.Bitmap.Compress(Bitmap.CompressFormat.Jpeg, 8, fs);
                    }
                }
                catch (Exception e)
                {
                    //Console.WriteLine("SaveImage exception: " + e.Message);
                }
                finally
                {
                    if (fs != null)
                        fs.Close();

                    //Android.Media.MediaActionSound sound = new Android.Media.MediaActionSound();
                    //sound.Play(Android.Media.MediaActionSoundType.ShutterClick);

                    Intent mediaScanIntent = new Intent(Intent.ActionCameraButton);
                    SendBroadcast(mediaScanIntent);

                    mediaScanIntent = new Intent(Intent.ActionMediaScannerScanFile);
                    filename = System.IO.Path.GetFullPath(filename);
                    Java.IO.File JF = new File(filename);
                    Android.Net.Uri contentUri = Android.Net.Uri.FromFile(JF);
                    mediaScanIntent.SetData(contentUri);
                    SendBroadcast(mediaScanIntent);
                }

            };
            _snapThread.RunWorkerCompleted += (sender, args) =>
            {
                ((SurfaceView)FindViewById(Resource.Id.iv_blank)).Visibility = ViewStates.Gone;
            };

            _snapThread.RunWorkerAsync();

        }

        void squareButtons()
        {
            Spinner spin = FindViewById<Spinner>(Resource.Id.spinner1);

            ViewGroup.LayoutParams lparams = searchButton.LayoutParameters;
            if (lparams.Width == spin.Height) return; //TODO: This sucks. Need better way of trigger button adjustments once.
            lparams.Width = spin.Height;
            lparams.Height = spin.Height;
            searchButton.LayoutParameters = lparams;

            lparams = sharebutton.LayoutParameters;
            lparams.Width = spin.Height;
            lparams.Height = spin.Height;
            sharebutton.LayoutParameters = lparams;

            //Make camera button same size
            ViewGroup.LayoutParams lparamsCam = cambutton.LayoutParameters;
            lparamsCam.Width = spin.Height;
            lparamsCam.Height = spin.Height;
            cambutton.LayoutParameters = lparamsCam;

            ViewGroup.LayoutParams lparamsImp = imptButton.LayoutParameters;
            lparamsImp.Width = spin.Height;
            lparamsImp.Height = spin.Height;
            imptButton.LayoutParameters = lparamsImp;
            imptButton.SetPadding(0, 0, sharebutton.Height, 0);
            //imptButton.SetX(sharebutton.GetX()  );

            ViewGroup.LayoutParams lparamsSnap = snapButton.LayoutParameters;
            lparamsSnap.Width = (int)(spin.Height * 1.5);
            lparamsSnap.Height = (int)(spin.Height * 1.5);
            //snapButton.Layout(sharebutton.Height*2, sharebutton.Height*2, sharebutton.Height, sharebutton.Height);
            snapButton.LayoutParameters = lparamsSnap;
            snapButton.SetX(snapButton.GetX()+ spin.Height / 2);
            snapButton.SetY(snapButton.GetY()-3* spin.Height / 4);
            snapButton.SetBackgroundResource(/*Android.*/Resource.Drawable.snapCam2);
        }


        [Android.Runtime.Register("onPause", "()V", "GetOnPauseHandler")]
        protected override void OnPause()
        {
            base.OnPause();
            LogManager.GetLogger().i("PASSED!", (SystemClock.ElapsedRealtime() - prevRuntime).ToString());
            //System.Threading.Thread.Sleep(500);
            if (curCamera!=null) curCamera.StopPreview();
        }

        [Android.Runtime.Register("onStop", "()V", "GetOnStopHandler")]
        protected override void OnStop()
        {
            base.OnStop();
        }

        [Android.Runtime.Register("onRestart", "()V", "GetOnRestartHandler")]
        protected override void OnRestart()
        {
            base.OnRestart();
            //System.Threading.Thread.Sleep(100);

        }

        public override void OnBackPressed()
        {
            if (cameraLive)
            {
                curCamera.Unlock();
                curCamera.StopPreview();
                ((SurfaceView)FindViewById(Resource.Id.iv_surf)).Holder.RemoveCallback(this);
                curCamera.SetPreviewCallback(null);
                try { curCamera.Reconnect(); } catch { }
                curCamera.Release();
                cameraLive = false;
                curCamera = null;
                if (_capThread != null)
                {
                    _capThread.CancelAsync();
                    _capThread.Dispose();
                    _capThread = null;
                }
                if (_cameraThread != null)
                {
                    _cameraThread.CancelAsync();
                    _cameraThread.Dispose();
                    _cameraThread = null;
                }
                cambutton.SetBackgroundResource(Android.Resource.Drawable.IcMenuCamera);
                sharebutton.Visibility = ViewStates.Visible;
                searchButton.Visibility = ViewStates.Visible;
                mAttacher.SetZoomable(true);
                snapButton.Visibility = ViewStates.Gone;
                currentOrigImage = currentImage;
                mImageView.SetImageDrawable(currentImage);

                releaseBuffs();
            }
            else
            {
                this.MoveTaskToBack(true);
                base.OnBackPressed();
            }
        }

        public override void OnWindowFocusChanged(bool hasFocus)
        {
            base.OnWindowFocusChanged(hasFocus);
            squareButtons();

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
                last_index = spinner.SelectedItemId;
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
                    string msg = string.Format("ERROR: No OpenCL support detected");

                    if (System.IO.File.Exists("/vendor/lib64/egl/libGLES_mali.so") ||
                        System.IO.File.Exists("/vendor/lib64/libOpenCL.so"))
                        msg = string.Format("ERROR: OpenCL driver is present,\nbut " + Android.OS.Build.Manufacturer + " hasn't enabled it");
                    //Toast.MakeText(this, msg, ToastLength.Long).Show();
                    mCurrMatrixTv.Visibility = ViewStates.Visible;
                    mCurrMatrixTv.Text = msg;
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

                int curWidth = (!useVid) ? ((BitmapDrawable)currentImage).Bitmap.Width : vidWidth;
                int curHeight = (!useVid) ? ((BitmapDrawable)currentImage).Bitmap.Height : vidHight;

                //try
                {
                    if (!useVid) _bitmapImage1 = (BitmapDrawable)currentImage;

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

                        sizeChanged = (final_output == null) || (final_output.Count != curWidth * curHeight * 4);
                        if (sizeChanged)
                        {
                            if (final_output != null)
                            {
                                final_output.Dispose();
                                final_out_ptr.Free();
                            }
                            int size = curWidth * curHeight * 4;
                            final_out_host = new byte[size];
                            final_out_ptr = System.Runtime.InteropServices.GCHandle.Alloc(final_out_host, System.Runtime.InteropServices.GCHandleType.Pinned);
                            final_output = new ClooBuffer<byte>(_context, ComputeMemoryFlags.UseHostPointer, size, final_out_ptr.AddrOfPinnedObject());
                            //final_output._hostBufferByte = final_out_host; // todo: this sucks
                        }
#if DEBUG
                        LogManager.GetLogger().i("CAMPROC-b", (SystemClock.ElapsedRealtime() - prevRuntime).ToString());
#endif
                        if (!useVid)//!IsHDR)//trackBar1.Visible)
                        {
                            _clooImageByteOriginal = ClooImage2DFloatRgbA.CreateFromBitmap(_context, ComputeMemoryFlags.UseHostPointer , _bitmapImage1.Bitmap);
                        }
                        else
                        {
                            //_clooImageByteOriginal = ClooImage2DFloatRgbA.CreateFromFloatArray(_context, ComputeMemoryFlags.UseHostPointer, vidData, vidWidth, vidHight);
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
                        //_queue.Finish();
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
                        for (int j = 0; !ReusableFound && (j < i) && (i != numKernels - 1); j++)
                        {
                            if (_clooImageByteIntermediate[j] != null)
                            {
                                bool Used = useVid; // if in video mode resue buffers across frames
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
#if DEBUG
                        LogManager.GetLogger().i("CAMPROC-h:" + i + ":", (SystemClock.ElapsedRealtime() - prevRuntime).ToString());
#endif
                        if (!ReusableFound)
                        {
                            //currentImage.p
                            //float[]  newHostBuf = new float[_bitmapImage1.Width*_bitmapImage1.Height*4];
                            if (i != numKernels - 1)
                            {
                                LogManager.GetLogger().i("CAMPROC-H:" + i + ":", (SystemClock.ElapsedRealtime() - prevRuntime).ToString());
                                //long cT = SystemClock.ElapsedRealtime();
                                _clooImageByteIntermediate[i] = ClooImage2DFloatRgbA.CreateHostNoAccess(_context, ComputeMemoryFlags.HostNoAccess, curWidth, curHeight);
                                //System.Threading.Thread.Sleep((int)(SystemClock.ElapsedRealtime()-cT));
                                if (_clooImageByteIntermediate[i] == null) // older ocl versions will need this alternative...
                                    _clooImageByteIntermediate[i] = ClooImage2DFloatRgbA.CreateHostNoAccess(_context, ComputeMemoryFlags.ReadWrite, curWidth, curHeight);
                                if (_clooImageByteIntermediate[i] == null)
                                    return;
                                LogManager.GetLogger().i("CAMPROC-HH:" + i + ":", (SystemClock.ElapsedRealtime() - prevRuntime).ToString());
                            }
                            else
                            {
                                // final layer writes to buffer, but pass dummy output buffer so the checks pass
                                _clooImageByteIntermediate[i] = _clooImageByteOriginal;// ClooImage2DFloatRgbA.Create(_context, ComputeMemoryFlags.UseHostPointer , curWidth, curHeight);
                            }
                            //_clooImageByteIntermediate[i] = ClooImage2DFloatRgbA.CreateFromFloatArray(_context, ComputeMemoryFlags.ReadWrite , newHostBuf,  _bitmapImage1.Width, _bitmapImage1.Height);

                            
                        }

                        //_queue.Finish();
#if DEBUG
                        LogManager.GetLogger().i("CAMPROC-m:" + i + ":", (SystemClock.ElapsedRealtime() - prevRuntime).ToString());
#endif
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
#if DEBUG
                        LogManager.GetLogger().i("CAMPROC-j:"+i+":", (SystemClock.ElapsedRealtime() - prevRuntime).ToString());
#endif
                    }

                    //label6.Content = stopwatch.ElapsedMilliseconds + " ms - sobel";
                    //currentOrigImage.Dispose();
                    //currentOrigImage = currentImage;
                    //toolStripStatusLabel1.Text = stopwatch.ElapsedMilliseconds + " ms - " + toolStripComboBox1.Text;
                    GC.Collect();
                    _queue.Finish();
                    LogManager.GetLogger().i("CAMPROC-r:", (SystemClock.ElapsedRealtime() - prevRuntime).ToString());
                    //_queue.CopyImageToBuffer(_clooImageByteIntermediate[0], final_output, null);
                    //currentImage.Dispose();

                    //Bitmap curImg = _clooImageByteIntermediate[numKernels - 1].ToBitmap(_queue);
                    if (sizeChanged || !useVid)
                    {
                        curImg = Bitmap.CreateBitmap(curWidth, curHeight, Bitmap.Config.Argb8888);
                        curImg = final_output.ByteToBitmap(_queue, curImg);
                    }
                    else
                    {
                        curImg = final_output.ByteToBitmap(_queue, curImg);
                        //currentImage = new BitmapDrawable(Resources, curImg);
                        //curImg = rotateBitmap(curImg, 90);
                    }

                    //_queue.Finish();
#if DEBUG
                    LogManager.GetLogger().i("CAMPROC-q:", (SystemClock.ElapsedRealtime() - prevRuntime).ToString());
#endif
                    if (useVid) return;

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
                    releaseBuffs();
                }

                //mImageView. ResetMatrix();
                if (!useVid) mImageView.SetImageDrawable(currentImage);

                LogManager.GetLogger().i("CAMPROC-w:", (SystemClock.ElapsedRealtime() - prevRuntime).ToString());

                if (!useVid) GC.Collect();

            }
            //catch (Exception ex)
            {
                //System.Windows.Forms.MessageBox.Show(ex.Message);
                //VisibDeviceNameCombobox_SelectedIndexChanged(null, null);
            }
        }

        void releaseBuffs()
        {
            if (_clooImageByteOriginal != null) { _clooImageByteOriginal.HostBuffer = null; _clooImageByteOriginal.Dispose(); }
            if (_clooImageByteGrayOriginal != null) { _clooImageByteGrayOriginal.HostBuffer = null; _clooImageByteGrayOriginal.Dispose(); }
            if (_clooImageByteResult != null) { _clooImageByteResult.HostBuffer = null; _clooImageByteResult.Dispose(); }
            for (int i = 0; i < _clooImageByteIntermediate.Count-1 /*last one has no img*/; i++)
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
                        //find best place to insert...
                        /*int xIdx = int.Parse(comBreakdown[0]);
                        int yIdx = int.Parse(comBreakdown[1]);
                        int flatIdx = yIdx + 1000 * xIdx;
                        int bIdx = convolvLis.Count;
                        for (int fidx = 0; fidx < convolvLis.Count; fidx++)
                            if (flatIdx < convolvLis[fidx][1] + 1000 * convolvLis[fidx][0])
                            { bIdx = fidx; break; }
                        convolvLis.Insert(bIdx, new int[3] { xIdx, yIdx, int.Parse(comBreakdown[2]) });*/

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
                KernelString += "__kernel void specialKernel" + k + "(read_only image2d_t inputImage, write_only image2d_t outputImage, __global int* starting";

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
                                 + "    if ((pixel_diffR<" + origColorCheckLimits[0] + ")&&(pixel_diffG<" + origColorCheckLimits[1] + ")&&(pixel_diffB<" + origColorCheckLimits[2] + " )" /*&&(pixel_diffA<" + origColorCheckLimits[3] + ")"*/  + "){\n\t";
                    }
                    else if (testOrigColor == 2) //PrcntDiffThrsh
                    {
                        KernelString +=
                                  "    float pixel_prcntDiffR = fabs((pixel_orig.x-(float)" + compX + ")/" + compX + ")*100;\n"
                                + "    float pixel_prcntDiffG = fabs((pixel_orig.y-(float)" + compY + ")/" + compY + ")*100;\n"
                                + "    float pixel_prcntDiffB = fabs((pixel_orig.z-(float)" + compZ + ")/" + compZ + ")*100;\n"
                                + "    float pixel_prcntDiffA = fabs((pixel_orig.w-(float)" + compW + ")/" + compW + ")*100;\n"
                                + "    if ((pixel_prcntDiffR<" + origColorCheckLimits[0] + ")&&(pixel_prcntDiffG<" + origColorCheckLimits[1] + ")&&(pixel_prcntDiffB<" + origColorCheckLimits[2] + " )" /*"&&(pixel_prcntDiffA<" + origColorCheckLimits[3] + ")" */ + "){\n\t";
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
                   // }
                   // for (int c = 0; c < convolvLis.Count; c++)
                   // {
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
                                             + "   if ((pixel" + c + "_diffR<" + srcColorCheckLimits[0] + ")&& (pixel" + c + "_diffG<" + srcColorCheckLimits[1] + ")&&(pixel" + c + "_diffB<" + srcColorCheckLimits[2] + " )" /*&&(pixel" + c + "_diffA<" + srcColorCheckLimits[3] + ")"*/ + "){\n\t";
                            }
                            else if (testSrcColor == 2) //PrcntDiffThrsh
                            {
                                KernelString += "    float pixel" + c + "_prcntDiffR = fabs((pixel" + c + ".x-(float)" + compX + ")/" + compX + ")*100;\n"
                                                + "    float pixel" + c + "_prcntDiffG = fabs((pixel" + c + ".y-(float)" + compY + ")/" + compY + ")*100;\n"
                                                + "    float pixel" + c + "_prcntDiffB = fabs((pixel" + c + ".z-(float)" + compZ + ")/" + compZ + ")*100;\n"
                                                + "    float pixel" + c + "_prcntDiffA = fabs((pixel" + c + ".w-(float)" + compW + ")/" + compW + ")*100;\n"
                                                + "   if ((pixel" + c + "_prcntDiffR<" + srcColorCheckLimits[0] + ")&& (pixel" + c + "_prcntDiffG<" + srcColorCheckLimits[1] + ")&&(pixel" + c + "_prcntDiffB<" + srcColorCheckLimits[2] + " )" /*&&(pixel" + c + "_prcntDiffA<" + srcColorCheckLimits[3] + ")" */ + "){\n\t";
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
                                             + "   if ((pixel" + c + "_diffR<" + srcColorCheckLimits[0] + ")&& (pixel" + c + "_diffG<" + srcColorCheckLimits[1] + ")&&(pixel" + c + "_diffB<" + srcColorCheckLimits[2] + " )" /*"&&(pixel" + c + "_diffA<" + srcColorCheckLimits[3] + ")" */ + "){\n\t";
                            }
                            else if (testSrcColor == 2) //PrcntDiffThrsh
                            {
                                KernelString += "    float pixel" + c + "_prcntDiffR = fabs((pixel" + c + ".x-(float)" + compX + ")/" + compX + ")*100;\n"
                                                + "    float pixel" + c + "_prcntDiffG = fabs((pixel" + c + ".y-(float)" + compY + ")/" + compY + ")*100;\n"
                                                + "    float pixel" + c + "_prcntDiffB = fabs((pixel" + c + ".z-(float)" + compZ + ")/" + compZ + ")*100;\n"
                                                + "    float pixel" + c + "_prcntDiffA = fabs((pixel" + c + ".w-(float)" + compW + ")/" + compW + ")*100;\n"
                                                + "   if ((pixel" + c + "_prcntDiffR<" + srcColorCheckLimits[0] + ")&& (pixel" + c + "_prcntDiffG<" + srcColorCheckLimits[1] + ")&&(pixel" + c + "_prcntDiffB<" + srcColorCheckLimits[2] + " )" /*"&&(pixel" + c + "_prcntDiffA<" + srcColorCheckLimits[3] + ")*/ + "){\n\t";
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
                    KernelString += "    final_output[4*i  +j*get_image_width(outputImage)*4] =  (char) pixel_new.x;\n";
                    KernelString += "    final_output[4*i+1+j*get_image_width(outputImage)*4] =  (char) pixel_new.y;\n";
                    KernelString += "    final_output[4*i+2+j*get_image_width(outputImage)*4] =  (char) pixel_new.z;\n";
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
                        KernelString += "    }\n    else\n    {\n   " + ((k == kernelFormulas.Length - 1) ? "pixel_orig=clamp(fabs(pixel_orig), 0.0f, 255.0f);\n" : "");
                        KernelString += "    final_output[4*i  +j*get_image_width(outputImage)*4] =  (char) pixel_orig.x;\n";
                        KernelString += "    final_output[4*i+1+j*get_image_width(outputImage)*4] =  (char) pixel_orig.y;\n";
                        KernelString += "    final_output[4*i+2+j*get_image_width(outputImage)*4] =  (char) pixel_orig.z;\n";
                        KernelString += "    final_output[4*i+3+j*get_image_width(outputImage)*4] =  (char) 255;\n   }\n";
                    }
                    else
                    {
                        KernelString += "    }\n    else\n    {\n      " + ((k == kernelFormulas.Length - 1) ? "pixel_orig=clamp(fabs(pixel_orig), 0.0f, 255.0f);" : "") + " write_imagef(outputImage,(int2)(i,j), pixel_orig);\n    }\n";
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
                        KernelString += "    }\n    else\n    {\n   " + ((k == kernelFormulas.Length - 1) ? "pixel_orig=clamp(fabs(pixel_orig), 0.0f, 255.0f);\n" : "");
                        KernelString += "    final_output[4*i  +j*get_image_width(outputImage)*4] =  (char) pixel_orig.x;\n";
                        KernelString += "    final_output[4*i+1+j*get_image_width(outputImage)*4] =  (char) pixel_orig.y;\n";
                        KernelString += "    final_output[4*i+2+j*get_image_width(outputImage)*4] =  (char) pixel_orig.z;\n";
                        KernelString += "    final_output[4*i+3+j*get_image_width(outputImage)*4] =  (char) 255;\n    }\n";
                    }
                    else
                    {
                        KernelString += "    }\n    else\n    {\n      " + ((k == kernelFormulas.Length - 1) ? "pixel_orig=clamp(fabs(pixel_orig), 0.0f, 255.0f);" : "") + " write_imagef(outputImage,(int2)(i,j), pixel_orig);\n    }\n";
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
            mAttacher = null;
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
				//context.mCurrMatrixTv.Text=(rect.ToString());
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


