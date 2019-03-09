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

namespace OpenClooVision.Imaging
{
    /// <summary>
    /// Cloo compute image 2D (gray, float)
    /// </summary>
    [CLSCompliant(false)]
    public class ClooImage2DFloatA : ClooImage2D<float>, IImage2DFloatA
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
        protected ClooImage2DFloatA(ClooContext context, ComputeMemoryFlags flags, int width, int height, long rowPitch, System.IntPtr data)
            : base(context, flags, ClooImageFormat.FloatA, width, height, rowPitch, data)
        {
        }

        /// <summary>
        /// Converts a bitmap to buffer
        /// </summary>
        /// <param name="bitmap">bitmap</param>
        /// <param name="data">buffer to put the data into</param>
        public static unsafe void BitmapToBuffer(Bitmap bitmap, float[] data)
        {
            if (data.Length < bitmap.Width * bitmap.Height)
                throw new ArgumentException("Buffer size is too small for bitmap");

            var byteBuffer = Java.Nio.ByteBuffer.Allocate(bitmap.ByteCount);
            bitmap.CopyPixelsToBuffer(byteBuffer);
            data = byteBuffer.ToArray<float>();


            /*fixed (float* p = data)
            {
                BitmapData bitmapData = null;
                try
                {
                    bitmapData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);

                    byte* scan = (byte*)bitmapData.Scan0.ToPointer();
                    int nOffset = bitmapData.Stride - bitmap.Width * 3;

                    int pos = 0;
                    int width = bitmap.Width;
                    int height = bitmap.Height;
                    for (int y = 0; y < height; y++)
                    {
                        for (int x = 0; x < width; x++)
                        {
                            p[pos++] = (float)scan[2] * 0.3f + (float)scan[1] * 0.59f + (float)scan[0] * 0.11f;
                            scan += 3;
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
            float[] data = new float[bitmap.Width * bitmap.Height];
            BitmapToBuffer(bitmap, data);
            return data;
        }

        /// <summary>
        /// Create image
        /// </summary>
        /// <param name="context">compute context</param>
        /// <param name="flags">memory flags</param>
        /// <param name="width">image width</param>
        /// <param name="height">image height</param>
        /// <exception cref="ArgumentException">OpenCL requires the image to have a width and height of at least one pixel</exception>
        public static ClooImage2DFloatA Create(ClooContext context, ComputeMemoryFlags flags, int width, int height)
        {
            if ((width < 1) || (height < 1)) throw new ArgumentException("OpenCL requires the image to have a width and height of at least one pixel", "image");

            float[] data = new float[width * height];
            GCHandle handle = GCHandle.Alloc(data, GCHandleType.Pinned);
            try
            {
                IntPtr addr = handle.AddrOfPinnedObject();
                ClooImage2DFloatA res = new ClooImage2DFloatA(context, flags, width, height, 0, addr);
                res._hostBuffer = data;
                return res;
            }
            finally { handle.Free(); }
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
        public static unsafe ClooImage2DFloatA CreateFromBitmap(ClooContext context, ComputeMemoryFlags flags, Bitmap bitmap)
        {
            if (bitmap == null) throw new ArgumentNullException("image");
            if ((bitmap.Width < 1) || (bitmap.Height < 1)) throw new ArgumentException("OpenCL requires the image to have a width and height of at least one pixel", "image");

            float[] data = BitmapToBuffer(bitmap);

            GCHandle handle = GCHandle.Alloc(data, GCHandleType.Pinned);
            try
            {
                IntPtr addr = handle.AddrOfPinnedObject();
                ClooImage2DFloatA res = new ClooImage2DFloatA(context, flags, bitmap.Width, bitmap.Height, 0, addr);
                res._hostBuffer = data;
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

            // read image data from command queue
            if (Modified) ReadFromDevice(queue);

            //return BitmapFactory.DecodeByteArray(_hostBuffer, 0, _hostBuffer.Length);

            for (int j = 0; j < bitmap.Height; j++)
            {
                for (int i = 0; i < bitmap.Width; i++)
                {
                    Android.Graphics.Color newColor = Android.Graphics.Color.Rgb((int)_hostBuffer[i + j * bitmap.Width], (int)_hostBuffer[i + j * bitmap.Width], (int)_hostBuffer[i + j * bitmap.Width]);
                    bitmap.SetPixel(i, j, newColor);
                }
            }
            return bitmap;


            /*try
            {
                bitmapData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);

                byte* scan = (byte*)bitmapData.Scan0.ToPointer();
                int nOffset = bitmapData.Stride - bitmap.Width * 3;

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
                            float val = _hostBuffer[pos++] * 255f; // denormalize values
                            if (val < 0) val = 0;
                            if (val > 255) val = 255;
                            scan[2] = (byte)val;
                            scan[1] = (byte)val;
                            scan[0] = (byte)val;
                            scan += 3;
                        }
                        scan += nOffset;
                    }
                }
                else
                {
                    // not normalized
                    for (int y = 0; y < height; y++)
                    {
                        for (int x = 0; x < width; x++)
                        {
                            float val = _hostBuffer[pos++];
                            if (val < 0) val = 0;
                            if (val > 255) val = 255;
                            scan[2] = (byte)val;
                            scan[1] = (byte)val;
                            scan[0] = (byte)val;
                            scan += 3;
                        }
                        scan += nOffset;
                    }
                }
            }
            finally
            {
                if (bitmapData != null) bitmap.UnlockBits(bitmapData);
            }
            return bitmap;*/
        }

        /// <summary>
        /// Converts part of host buffer to existing managed bitmap (make sure size matches!)
        /// </summary>
        /// <param name="queue">compute command queue</param>
        /// <param name="bitmap">we have already a bitmap to put it in</param>
        /// <param name="rect">part of rectangle to copy</param>
        /// <returns>managed bitmap</returns>
        /// <exception cref="ArgumentNullException">queue</exception>
        public unsafe Bitmap ToBitmap(ClooCommandQueue queue, Bitmap bitmap, Rectangle rect)
        {
            if (queue == null) throw new ArgumentNullException("queue");

            //BitmapData bitmapData = null;

            // read image data from command queue
            if (Modified) ReadFromDevice(queue);

            //return BitmapFactory.DecodeByteArray(_hostBuffer, 0, _hostBuffer.Length);

            for (int j = 0; j < bitmap.Height; j++)
            {
                for (int i = 0; i < bitmap.Width; i++)
                {
                    Android.Graphics.Color newColor = Android.Graphics.Color.Rgb((int)_hostBuffer[i + j * bitmap.Width], (int)_hostBuffer[i + j * bitmap.Width], (int)_hostBuffer[i + j * bitmap.Width]);
                    bitmap.SetPixel(i, j, newColor);
                }
            }
            return bitmap;


            /*if (queue == null) throw new ArgumentNullException("queue");
            if (bitmap.Width != rect.Width) throw new ArgumentException("Bitmap width must be " + rect.Width);
            if (bitmap.Height != rect.Height) throw new ArgumentException("Bitmap height must be " + rect.Height);

            BitmapData bitmapData = null;

            // read image data from command queue
            if (Modified) ReadFromDevice(queue);

            try
            {
                bitmapData = bitmap.LockBits(new Rectangle(0, 0, rect.Width, rect.Height), ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);

                byte* scan = (byte*)bitmapData.Scan0.ToPointer();
                int bOffset = bitmapData.Stride - rect.Width * 3;

                int width = Width;
                int height = Height;
                int pos = rect.Top * Width + rect.Left;
                int hOffset = Width - rect.Width;

                if (Normalized)
                {
                    // normalized values
                    for (int y = 0; y < rect.Height; y++)
                    {
                        for (int x = 0; x < rect.Width; x++)
                        {
                            float val = _hostBuffer[pos++] * 255f; // denormalize values
                            if (val < 0) val = 0;
                            if (val > 255) val = 255;
                            scan[2] = (byte)val;
                            scan[1] = (byte)val;
                            scan[0] = (byte)val;
                            scan += 3;
                        }
                        scan += bOffset;
                        pos += hOffset;
                    }
                }
                else
                {
                    // not normalized
                    for (int y = 0; y < rect.Height; y++)
                    {
                        for (int x = 0; x < rect.Width; x++)
                        {
                            float val = _hostBuffer[pos++];
                            if (val < 0) val = 0;
                            if (val > 255) val = 255;
                            scan[2] = (byte)val;
                            scan[1] = (byte)val;
                            scan[0] = (byte)val;
                            scan += 3;
                        }
                        scan += bOffset;
                        pos += hOffset;
                    }
                }
            }
            finally
            {
                if (bitmapData != null) bitmap.UnlockBits(bitmapData);
            }
            return bitmap;*/
        }

        /// <summary>
        /// Creates a new managed bitmap
        /// </summary>
        /// <param name="queue">compute command queue</param>
        /// <returns>managed bitmap</returns>
        public unsafe Bitmap ToBitmap(ClooCommandQueue queue)
        {

            /*Bitmap bitmap = new Bitmap(Width, Height, PixelFormat.Format24bppRgb);
            ToBitmap(queue, bitmap);
            return bitmap;*/

            Bitmap bitmap = Bitmap.CreateBitmap(Width, Height, Bitmap.Config.Argb8888);
            ToBitmap(queue, bitmap);
            return bitmap;
        }

        /// <summary>
        /// Creates a new managed bitmap
        /// </summary>
        /// <param name="queue">compute command queue</param>
        /// <param name="rect">part of rectangle to copy</param>
        /// <returns>managed bitmap</returns>
        public unsafe Bitmap ToBitmap(ClooCommandQueue queue, System.Drawing.Rectangle rect)
        {
            //Bitmap bitmap = new Bitmap(rect.Width, rect.Height, PixelFormat.Format24bppRgb);
            Bitmap bitmap = Bitmap.CreateBitmap(Width, Height, Bitmap.Config.Argb8888);
            ToBitmap(queue, bitmap, rect);
            return bitmap;
        }
    }
}
