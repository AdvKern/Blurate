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
//using System.Drawing;
//using System.Drawing.Imaging;
using Android.Graphics;

namespace OpenClooVision.Imaging
{
    /// <summary>
    /// Compute image 2D for CPU only (RGBA, float)
    /// </summary>
    [CLSCompliant(false)]
    public class CpuImage2DFloatRgbA : CpuImage2DRgbA<float>, IImage2DFloatRgbA
    {
        /// <summary>
        /// Are the float values normalized (between 0 and 1)?
        /// </summary>
        public bool Normalized { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="width">image width</param>
        /// <param name="height">image height</param>
        /// <param name="data">image data</param>
        public CpuImage2DFloatRgbA(int width, int height, float[] data = null)
            : base(width, height, data)
        {
        }

        /// <summary>
        /// Create image
        /// </summary>
        /// <param name="width">image width</param>
        /// <param name="height">image height</param>
        /// <param name="data">initial data</param>
        public static CpuImage2DFloatRgbA Create(int width, int height, float[] data = null)
        {
            if ((width < 1) || (height < 1)) throw new ArgumentException("OpenCL requires the image to have a width and height of at least one pixel", "image");

            if (data == null) data = new float[4 * width * height];
            CpuImage2DFloatRgbA res = new CpuImage2DFloatRgbA(width, height, data);
            return res;
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

            var byteBuffer = Java.Nio.ByteBuffer.Allocate(bitmap.ByteCount);
            bitmap.CopyPixelsToBuffer(byteBuffer);
            data = byteBuffer.ToArray<float>();
            /*
            fixed (float* p = data)
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
        /// Creates a CpuImage2D from bitmap
        /// </summary>
        /// <param name="bitmap">bitmap</param>
        /// <returns>CpuImage2D</returns>
        /// <exception cref="ArgumentNullException">bitmap</exception>
        /// <exception cref="ArgumentException">OpenCL requires the image to have a width and height of at least one pixel</exception>
        public static unsafe CpuImage2DFloatA CreateFromBitmap(Bitmap bitmap)
        {
            if (bitmap == null) throw new ArgumentNullException("image");
            if ((bitmap.Width < 1) || (bitmap.Height < 1)) throw new ArgumentException("OpenCL requires the image to have a width and height of at least one pixel", "image");

            float[] data = BitmapToBuffer(bitmap);

            CpuImage2DFloatA res = new CpuImage2DFloatA(bitmap.Width, bitmap.Height, data);
            return res;
        }

        /// <summary>
        /// Converts host buffer to existing managed bitmap (make sure size matches!)
        /// </summary>
        /// <param name="bitmap">we have already a bitmap to put it in</param>
        /// <returns>managed bitmap</returns>
        /// <exception cref="ArgumentNullException">queue</exception>
        public unsafe Bitmap ToBitmap(Bitmap bitmap)
        {
            //BitmapData bitmapData = null;
            byte[] _hostByteBuffer = new byte[_hostBuffer.Length];
            for (int i = 0; i < _hostBuffer.Length; i++)
                _hostByteBuffer[i] = (byte)(255.0 * _hostBuffer[i]);
            Bitmap retunBmp = BitmapFactory.DecodeByteArray(_hostByteBuffer, 0, _hostBuffer.Length);
            retunBmp.Width = bitmap.Width;
            retunBmp.Height = bitmap.Height;
            return retunBmp;

            /*try
            {
                bitmapData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);

                byte* scan = (byte*)bitmapData.Scan0.ToPointer();
                int nOffset = bitmapData.Stride - bitmap.Width * 4;

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
                            scan[2] = (byte)(_hostBuffer[pos] * 255f);
                            scan[1] = (byte)(_hostBuffer[pos + 1] * 255f);
                            scan[0] = (byte)(_hostBuffer[pos + 2] * 255f);
                            scan[3] = (byte)(_hostBuffer[pos + 3] * 255f);
                            pos += 4;
                            scan += 4;
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
                            scan[2] = (byte)_hostBuffer[pos];
                            scan[1] = (byte)_hostBuffer[pos + 1];
                            scan[0] = (byte)_hostBuffer[pos + 2];
                            scan[3] = (byte)_hostBuffer[pos + 3];
                            pos += 4;
                            scan += 4;
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
        /// Creates a new managed bitmap
        /// </summary>
        /// <returns>managed bitmap</returns>
        public unsafe Bitmap ToBitmap()
        {
            //Bitmap bitmap = new Bitmap(Width, Height, PixelFormat.Format24bppRgb);
            Bitmap bitmap = Bitmap.CreateBitmap(Width, Height, Bitmap.Config.Argb8888);
            ToBitmap(bitmap);
            return bitmap;
        }
    }
}
