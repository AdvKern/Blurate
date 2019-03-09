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
using Cloo.Bindings;

namespace OpenClooVision.Imaging
{
    /// <summary>
    /// Cloo compute image 2D (gray, byte)
    /// </summary>
    [CLSCompliant(false)]
    public class ClooImage2DByteA : ClooImage2D<byte>, IImage2DByteA
    {
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
        protected ClooImage2DByteA(ClooContext context, ComputeMemoryFlags flags, int width, int height, long rowPitch, System.IntPtr data)
            : base(context, flags, ClooImageFormat.ByteA, width, height, rowPitch, data)
        {
        }

        protected ClooImage2DByteA(ClooContext context, ComputeMemoryFlags flags)
            : base(context, flags, ClooImageFormat.ByteA, 1, 1, 0, IntPtr.Zero)
        {
        }

        /// <summary>
        /// Converts a bitmap to buffer
        /// </summary>
        /// <param name="bitmap">bitmap</param>
        /// <param name="data">buffer to put the data into</param>
        public static unsafe void BitmapToBuffer(Bitmap bitmap, byte[] data)
        {
            var byteBuffer = Java.Nio.ByteBuffer.Allocate(bitmap.ByteCount);
            bitmap.CopyPixelsToBuffer(byteBuffer);
            data = byteBuffer.ToArray<byte>();

            /*if (data.Length < bitmap.Width * bitmap.Height)
                throw new ArgumentException("Buffer size is too small for bitmap");

            using (var stream = new System.IO.MemoryStream())
            {
                bitmap.Compress(Bitmap.CompressFormat., 0, stream);
                data = stream.ToArray();
            }

            fixed (byte* p = data)
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
                            p[pos++] = (byte)((float)scan[2] * 0.2989f + (float)scan[1] * 0.5870f + (float)scan[0] * 0.1140f);
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
        public static unsafe byte[] BitmapToBuffer(Bitmap bitmap)
        {
            byte[] data = new byte[bitmap.Width * bitmap.Height];
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
        public static ClooImage2DByteA Create(ClooContext context, ComputeMemoryFlags flags, int width, int height)
        {
            if ((width < 1) || (height < 1)) throw new ArgumentException("OpenCL requires the image to have a width and height of at least one pixel", "image");

            byte[] data = new byte[width * height];
            GCHandle handle = GCHandle.Alloc(data, GCHandleType.Pinned);
            try
            {
                IntPtr addr = handle.AddrOfPinnedObject();
                ClooImage2DByteA res = new ClooImage2DByteA(context, flags, width, height, 0, addr);
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
        public static unsafe ClooImage2DByteA CreateFromBitmap(ClooContext context, ComputeMemoryFlags flags, Bitmap bitmap)
        {
            if (bitmap == null) throw new ArgumentNullException("image");
            if ((bitmap.Width < 1) || (bitmap.Height < 1)) throw new ArgumentException("OpenCL requires the image to have a width and height of at least one pixel", "image");

            byte[] data = BitmapToBuffer(bitmap);

            GCHandle handle = GCHandle.Alloc(data, GCHandleType.Pinned);
            try
            {
                IntPtr addr = handle.AddrOfPinnedObject();
                ClooImage2DByteA res = new ClooImage2DByteA(context, flags, bitmap.Width, bitmap.Height, 0, addr);
                res._hostBuffer = data;
                return res;
            }
            finally { handle.Free(); }
        }

        /// <summary>
        /// Creates a new <see cref="ComputeImage2D"/> from an OpenGL 2D texture object.
        /// </summary>
        /// <param name="context"> A <see cref="ComputeContext"/> with enabled CL/GL sharing. </param>
        /// <param name="flags"> A bit-field that is used to specify usage information about the <see cref="ComputeImage2D"/>. Only <c>ComputeMemoryFlags.ReadOnly</c>, <c>ComputeMemoryFlags.WriteOnly</c> and <c>ComputeMemoryFlags.ReadWrite</c> are allowed. </param>
        /// <param name="textureTarget"> One of the following values: GL_TEXTURE_2D, GL_TEXTURE_CUBE_MAP_POSITIVE_X, GL_TEXTURE_CUBE_MAP_POSITIVE_Y, GL_TEXTURE_CUBE_MAP_POSITIVE_Z, GL_TEXTURE_CUBE_MAP_NEGATIVE_X, GL_TEXTURE_CUBE_MAP_NEGATIVE_Y, GL_TEXTURE_CUBE_MAP_NEGATIVE_Z, or GL_TEXTURE_RECTANGLE. Using GL_TEXTURE_RECTANGLE for texture_target requires OpenGL 3.1. Alternatively, GL_TEXTURE_RECTANGLE_ARB may be specified if the OpenGL extension GL_ARB_texture_rectangle is supported. </param>
        /// <param name="mipLevel"> The mipmap level of the OpenGL 2D texture object to be used. </param>
        /// <param name="textureId"> The OpenGL 2D texture object id to use. </param>
        /// <returns> The created <see cref="ComputeImage2D"/>. </returns>
        public static new ClooImage2DByteA CreateFromGLTexture2D(ClooContext context, ComputeMemoryFlags flags, int textureTarget, int mipLevel, int textureId)
        {
            unsafe
            {
                ComputeErrorCode error = ComputeErrorCode.Success;
                IntPtr image = CL10.CreateFromGLTexture2D(
                    context.Handle,
                    flags,
                    textureTarget,
                    mipLevel,
                    textureId,
                    &error);
                ComputeException.ThrowOnError(error);

                ClooImage2DByteA res = new ClooImage2DByteA(context, flags);
                res.Handle = image;
                res.Init();
                res.ImageFormat = ClooImageFormatConverter.FromComputeImage(res, typeof(byte));
                if (res.ImageFormat != ClooImageFormat.ByteA) throw new ArgumentException("Incorrect texture format");
                res.HostBuffer = new byte[res.Width * res.Height * res.ElementSize];

                return res;
            }
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

            return BitmapFactory.DecodeByteArray(_hostBuffer, 0, _hostBuffer.Length);

            /*try
            {
                bitmapData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);

                byte* scan = (byte*)bitmapData.Scan0.ToPointer();
                int nOffset = bitmapData.Stride - bitmap.Width * 3;

                int pos = 0;
                int width = bitmap.Width;
                int height = bitmap.Height;
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        byte val = _hostBuffer[pos++];
                        scan[2] = val;
                        scan[1] = val;
                        scan[0] = val;
                        scan += 3;
                    }
                    scan += nOffset;
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

            if (Modified) ReadFromDevice(queue);

            return BitmapFactory.DecodeByteArray(_hostBuffer, 0, _hostBuffer.Length);


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
                for (int y = 0; y < rect.Height; y++)
                {
                    for (int x = 0; x < rect.Width; x++)
                    {
                        byte val = _hostBuffer[pos++];
                        scan[2] = val;
                        scan[1] = val;
                        scan[0] = val;
                        scan += 3;
                    }
                    scan += bOffset;
                    pos += hOffset;
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
        [Android.Runtime.Register("createBitmap", "(Landroid/util/DisplayMetrics;IILandroid/graphics/Bitmap$Config;)Landroid/graphics/Bitmap;", "")]
        public unsafe Bitmap ToBitmap(ClooCommandQueue queue)
        {
            
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
        public unsafe Bitmap ToBitmap(ClooCommandQueue queue, Rectangle rect)
        {
            Bitmap bitmap = Bitmap.CreateBitmap(rect.Width, rect.Height, Bitmap.Config.Argb8888);
            ToBitmap(queue, bitmap, rect);
            return bitmap;
        }
    }
}
