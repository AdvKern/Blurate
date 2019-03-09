#region License
/*

Copyright (c) 2010-2011 by Hans Wolff

Permission is hereby granted, free of charge, to any person
obtaining a copy of this software and associated documentation
files (the "Software"), to deal in the Software without
restriction, including without limitation the rights to use,
copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the
Software is furnished to do so, subject to the following
conditions:

The above copyright notice and this permission notice shall be
included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
OTHER DEALINGS IN THE SOFTWARE.

*/
#endregion

using System;
using System.Drawing;
//using System.Drawing.Imaging;
using Android.Graphics;
using System.Runtime.InteropServices;
using Cloo;
using Android.Widget;

namespace OpenClooVision.Imaging
{
    /// <summary>
    /// Cloo compute image 2D
    /// </summary>
    [CLSCompliant(false)]
    public class ClooImage2DFloatRgbA : ClooImage2D<float>, IImage2DFloatRgbA
    {
        /// <summary>
        /// Are the float values normalized (between 0 and 1)?
        /// </summary>
        public bool Normalized { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="context">compute context</param>
        /// <param name="flags">memory flags</param>
        /// <param name="format">image format</param>
        /// <param name="width">image width</param>
        /// <param name="height">image height</param>
        /// <param name="rowPitch">row pitch</param>
        /// <param name="data">image data</param>
        protected ClooImage2DFloatRgbA(ClooContext context, ComputeMemoryFlags flags, int width, int height, long rowPitch, System.IntPtr data)
            : base(context, flags, ClooImageFormat.FloatRgbA, width, height, rowPitch, data)
        {
            _context = context;
        }

        /// <summary>
        /// Converts a bitmap to buffer
        /// </summary>
        /// <param name="bitmap">bitmap</param>
        /// <param name="data">buffer to put the data into</param>
        /// <returns>bitmap as buffer</returns>
        public static unsafe void BitmapToBuffer(Bitmap bitmap, float[] data)
        {
            if (data.Length < 4 * bitmap.Width * bitmap.Height)
                throw new ArgumentException("Buffer size is too small for bitmap");

            var byteBuffer = Java.Nio.ByteBuffer.AllocateDirect(bitmap.ByteCount);
            bitmap.CopyPixelsToBuffer(byteBuffer);
            //var floatBuf = byteBuffer.AsFloatBuffer();

            byte* scan = (byte*)byteBuffer.GetDirectBufferAddress().ToPointer();
            int pos = 0;
            int width = bitmap.Width;
            int height = bitmap.Height;
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    data[pos++] = scan[0];
                    data[pos++] = scan[1];
                    data[pos++] = scan[2];
                    data[pos++] = scan[3];
                    scan += 4;
                }
                //scan += nOffset;
            }
            
            byteBuffer.Dispose();

            //data = floatBuf.ToArray<float>();

            //floatBuf.Rewind();

            //Marshal.Copy(byteBuffer.GetDirectBufferAddress(), data, 0, 4*bitmap.Width * bitmap.Height);

            /*fixed (float* p = data)
            {
                BitmapData bitmapData = null;
                try
                {
                    bitmapData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);

                    byte* scan = (byte*)bitmapData.Scan0.ToPointer();
                    int nOffset = bitmapData.Stride - bitmap.Width * 4;

                    int pos = 0;
                    int width = bitmap.Width;
                    int height = bitmap.Height;
                    for (int y = 0; y < height; y++)
                    {
                        for (int x = 0; x < width; x++)
                        {
                            p[pos++] = scan[2];
                            p[pos++] = scan[1];
                            p[pos++] = scan[0];
                            p[pos++] = scan[3];
                            scan += 4;
                        }
                        scan += nOffset;
                    }
                }
                finally
                {
                    if (bitmapData != null) bitmap.UnlockBits(bitmapData);
                }
            }*/
        }

        /// <summary>
        /// Converts a bitmap to buffer
        /// </summary>
        /// <param name="bitmap">bitmap</param>
        /// <returns>bitmap as buffer</returns>
        public static unsafe float[] BitmapToBuffer(Bitmap bitmap)
        {
            float[] data = new float[4 * bitmap.Width * bitmap.Height];
            BitmapToBuffer(bitmap, data);
            return data;
        }

        /// <summary>
        /// Create image
        /// </summary>
        /// <param name="context">compute context</paramn>
        /// <param name="flags">memory flags</param>
        /// <param name="width">image width</param>
        /// <param name="height">image height</param>
        /// <exception cref="ArgumentException">OpenCL requires the image to have a width and height of at least one pixel</exception>
        public static ClooImage2DFloatRgbA Create(ClooContext context, ComputeMemoryFlags flags, int width, int height)
        {
            if ((width < 1) || (height < 1)) throw new ArgumentException("OpenCL requires the image to have a width and height of at least one pixel", "image");

            float[] data = new float[width * height * 4];
            GCHandle handle = GCHandle.Alloc(data, GCHandleType.Pinned);
            try
            {
                IntPtr addr = handle.AddrOfPinnedObject();
                ClooImage2DFloatRgbA res = new ClooImage2DFloatRgbA(context, flags, width, height, 0, addr);
                res._hostBuffer = data;
                return res;
            }
            catch (Exception ex)
            {
                //System.Windows.Forms.MessageBox.Show("Either the image size is too large for your selected OpenCL device or you ran out of memory.\nMake sure latest drivers are installed.", "Image Load Error", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                //Toast.MakeText(null, ex.Message, ToastLength.Short);
                return null;
            }
            finally { handle.Free(); }
        }

        /// <summary>
        /// Create image
        /// </summary>
        /// <param name="context">compute context</param>
        /// <param name="flags">memory flags</param>
        /// <param name="width">image width</param>
        /// <param name="height">image height</param>
        /// <exception cref="ArgumentException">OpenCL requires the image to have a width and height of at least one pixel</exception>
        public static ClooImage2DFloatRgbA CreateHostNoAccess(ClooContext context, ComputeMemoryFlags flags, int width, int height)
        {
            if ((width < 1) || (height < 1)) throw new ArgumentException("OpenCL requires the image to have a width and height of at least one pixel", "image");

            try
            {
                IntPtr addr = IntPtr.Zero;
                ClooImage2DFloatRgbA res = new ClooImage2DFloatRgbA(context, flags, width, height, 0, addr);
                return res;
            }
            catch
            {
                //System.Windows.Forms.MessageBox.Show("Either the image size is too large for your selected OpenCL device or you ran out of memory.\nMake sure latest drivers are installed.", "Image Load Error", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                return null;
            }
            finally { }
        }

        /// <summary>
        /// Creates a ClooImage2D from bitmap
        /// </summary>
        /// <param name="context">compute context</param>
        /// <param name="flags">memory flags</param>
        /// <param name="bitmap">bitmap</param>
        /// <returns>ClooImage2D</returns>
        /// <exception cref="ArgumentNullException">bitmap</exception>
        /// <exception cref="ArgumentException">OpenCL requires the image to have a width and height of at least one pixel</exception>
        public static unsafe ClooImage2DFloatRgbA CreateFromBitmap(ClooContext context, ComputeMemoryFlags flags, Bitmap bitmap)
        {
            if (bitmap == null) throw new ArgumentNullException("image");
            if ((bitmap.Width < 1) || (bitmap.Height < 1)) throw new ArgumentException("OpenCL requires the image to have a width and height of at least one pixel", "image");

            float[] data = BitmapToBuffer(bitmap);

            GCHandle handle = GCHandle.Alloc(data, GCHandleType.Pinned);
            try
            {
                IntPtr addr = handle.AddrOfPinnedObject();
                ClooImage2DFloatRgbA res = new ClooImage2DFloatRgbA(context, flags, bitmap.Width, bitmap.Height, 0, addr);
                res._hostBuffer = data;
                return res;
            }
            catch
            {
                //System.Windows.Forms.MessageBox.Show("Either the image size is too large for your selectedOpenCL device or you ran out of memory.\nMake sure latest drivers are installed.", "Image Load Error", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                return null;
            }
            finally { handle.Free(); }
        }
        /// <summary>
        /// Creates a ClooImage2D from float array
        /// </summary>
        /// <param name="context">compute context</param>
        /// <param name="flags">memory flags</param>
        /// <param name="bitmap">bitmap</param>
        /// <returns>ClooImage2D</returns>
        /// <exception cref="ArgumentNullException">bitmap</exception>
        /// <exception cref="ArgumentException">OpenCL requires the image to have a width and height of at least one pixel</exception>
        public static unsafe ClooImage2DFloatRgbA CreateFromFloatArray(ClooContext context, ComputeMemoryFlags flags, float[] dataArray, int Width, int Height)
        {
            //if (bitmap == null) throw new ArgumentNullException("image");
            if ((Width < 1) || (Height < 1)) throw new ArgumentException("OpenCL requires the image to have a width and height of at least one pixel", "image");

            //float[] data = BitmapToBuffer(bitmap);

            GCHandle handle = GCHandle.Alloc(dataArray, GCHandleType.Pinned);
            try
            {
                IntPtr addr = handle.AddrOfPinnedObject();
                ClooImage2DFloatRgbA res = new ClooImage2DFloatRgbA(context, flags, Width, Height, 0, addr);
                res._hostBuffer = dataArray;
                return res;
            }
            finally { handle.Free(); }
        }

        /// <summary>
        /// Converts host buffer to existing managed bitmap (make sure size matches!)
        /// </summary>
        /// <param name="queue">compute command queue</param>
        /// <param name="bitmap">we have already a bitmap to put it in</param>
        /// <returns>managed bitmap</returns>
        /// <exception cref="ArgumentNullException">queue</exception>
        public unsafe Bitmap ToBitmap(ClooCommandQueue queue, Bitmap bitmap)

        {
            if (queue == null) throw new ArgumentNullException("queue");

            //BitmapData bitmapData = null;

            if (_hostBuffer == null)
            {
                try { _hostBuffer = new float[bitmap.Width * bitmap.Height * 4]; }
                catch
                {
                    string msg = string.Format("Out of memory.");
                    //Android.Widget.Toast.MakeText(this, msg, ToastLength.Long).Show();
                    return null;
                }
            }

            if (Modified) ReadFromDevice(queue);

            byte[] scan = new byte[_hostBuffer.Length];
            //for (int i = 0; i < _hostBuffer.Length; i++)
            //    scan[i] = (byte)(255.0 * _hostBuffer[i]);

            try
            {
                //bitmapData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);

                int scan_ptr = 0;// _hostByteBuffer.s //(byte*)bitmapData.Scan0.ToPointer();
                int nOffset = (int)bitmap.GetBitmapInfo().Stride  - bitmap.Width * 4;

                int pos = 0;
                int width = bitmap.Width;
                int height = bitmap.Height;
                if (Normalized)
                {
                    // normalized values
                    for (int y = 0; y < height; y++)
                    {
                        for (int x = 0; x < width; x++)
                        {
                            // denormalize
                            scan[scan_ptr+2] = (byte)(_hostBuffer[pos] * 255f);
                            scan[scan_ptr+1] = (byte)(_hostBuffer[pos + 1] * 255f);
                            scan[scan_ptr+0] = (byte)(_hostBuffer[pos + 2] * 255f);
                            scan[scan_ptr+3] = (byte)(_hostBuffer[pos + 3] * 255f);
                            pos += 4;
                            scan_ptr += 4;
                        }
                        scan_ptr += nOffset;
                    }
                }
                else
                {
                    // not normalized
                    for (int y = 0; y < height; y++)
                    {
                        for (int x = 0; x < width; x++)
                        {
                            scan[scan_ptr+2] = (byte)_hostBuffer[pos];
                            scan[scan_ptr+1] = (byte)_hostBuffer[pos + 1];
                            scan[scan_ptr+0] = (byte)_hostBuffer[pos + 2];
                            scan[scan_ptr+3] = (byte)_hostBuffer[pos + 3];
                            pos += 4;
                            scan_ptr += 4;
                        }
                        scan_ptr += nOffset;
                    }
                }
            }
            finally
            {
                //if (bitmapData != null) bitmap.UnlockBits(bitmapData);
            }

            //var imageBitmap = BitmapFactory.DecodeByteArray(scan, 0, _hostBuffer.Length);

            var byteBuffer = Java.Nio.ByteBuffer.AllocateDirect(bitmap.Width * bitmap.Height * 4);
            Marshal.Copy(scan, 0, byteBuffer.GetDirectBufferAddress(), bitmap.Width * bitmap.Height * 4);
            bitmap.CopyPixelsFromBuffer(byteBuffer);
            byteBuffer.Dispose();
            return bitmap;


            //bitmap.CopyPixelsFromBuffer(_hostBuffer)

            //return BitmapFactory.DecodeByteArray(scan, 0, _hostBuffer.Length);

        }

        /// <summary>
        /// Creates a new managed bitmap
        /// </summary>
        /// <param name="queue">compute command queue</param>
        /// <returns>managed bitmap</returns>
        public unsafe Bitmap ToBitmap(ClooCommandQueue queue)
        {
            //Bitmap bitmap = new Bitmap(Width, Height, PixelFormat.Format32bppArgb);
            Bitmap bitmap = Bitmap.CreateBitmap(Width, Height, Bitmap.Config.Argb8888);
            ToBitmap(queue, bitmap);
            return bitmap;
        }
    }
}
