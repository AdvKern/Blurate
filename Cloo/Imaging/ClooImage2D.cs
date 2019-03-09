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
using System.Runtime.InteropServices;
using Cloo;
using Cloo.Bindings;

namespace OpenClooVision.Imaging
{
    /// <summary>
    /// Cloo compute image 2D
    /// </summary>
    [CLSCompliant(false)]
    public class ClooImage2D<T> : ComputeImage2D, IImage2D<T> where T : struct
    {
        private object _lockRead = new object();
        private object _lockWrite = new object();

        /// <summary>
        /// Compute context
        /// </summary>
        protected ClooContext _context;
        /// <summary>
        /// Gets the associated compute context
        /// </summary>
        public new ClooContext Context
        {
            get { return _context; }
        }

        /// <summary>
        /// Host buffer
        /// </summary>
        protected T[] _hostBuffer = null;
        /// <summary>
        /// Host buffer
        /// </summary>
        public T[] HostBuffer
        {
            get { return _hostBuffer; }
            set { _hostBuffer = value; }
        }

        protected bool _modified = false;
        /// <summary>
        /// Get or set state if host buffer has been modified since last operation
        /// </summary>
        public bool Modified
        {
            get { return _modified; }
            set { _modified = value; }
        }

        /// <summary>
        /// Gets the cloo image format
        /// </summary>
        public ClooImageFormat ImageFormat { get; protected set; }

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
        protected ClooImage2D(ClooContext context, ComputeMemoryFlags flags, ClooImageFormat format, int width, int height, long rowPitch, System.IntPtr data)
            : base(context, flags, ClooImageFormatConverter.ToComputeImageFormat(format), width, height, rowPitch, data)
        {
            _context = context;
            ImageFormat = format;
        }

        protected ClooImage2D(ClooContext context, ComputeMemoryFlags flags)
            : base(context, flags, new ComputeImageFormat(), 1, 1, 0, IntPtr.Zero)
        {
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
        public static ClooImage2D<T> CreateFromGLTexture2D(ClooContext context, ComputeMemoryFlags flags, int textureTarget, int mipLevel, int textureId)
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

                ClooImage2D<T> res = new ClooImage2D<T>(context, flags);
                res.Handle = image;
                res.Init();
                res.ImageFormat = ClooImageFormatConverter.FromComputeImage(res, typeof(T));
                res.HostBuffer = new T[res.Width * res.Height * res.ElementSize];

                return res;
            }
        }

        /// <summary>
        /// Reads a buffer from device
        /// </summary>
        /// <param name="image">compute image</param>
        /// <param name="queue">command queue</param>
        public void ReadFromDevice(ClooCommandQueue queue)
        {
            ReadFromDevice(queue, _hostBuffer);
        }

        /// <summary>
        /// Reads a buffer from device
        /// </summary>
        /// <param name="image">compute image</param>
        /// <param name="queue">command queue</param>
        /// <param name="buffer">buffer to read into</param>
        public void ReadFromDevice(ClooCommandQueue queue, T[] buf)
        {
            lock (_lockRead)
            {
                GCHandle handle = GCHandle.Alloc(buf, GCHandleType.Pinned);
                try { queue.Read(this, true, new SysIntX3(), new SysIntX3(Width, Height, 1), RowPitch, SlicePitch, handle.AddrOfPinnedObject(), null); }
                finally {
                    handle.Free();
                }

                _modified = false;
            }
        }

        /// <summary>
        /// Write a buffer to device
        /// </summary>
        /// <param name="image">compute image</param>
        /// <param name="queue">command queue</param>
        public void WriteToDevice(ClooCommandQueue queue)
        {
            WriteToDevice(queue, _hostBuffer);
        }

        /// <summary>
        /// Writes a buffer from device
        /// </summary>
        /// <param name="image">compute image</param>
        /// <param name="queue">command queue</param>
        /// <param name="buffer">buffer to read into</param>
        public void WriteToDevice(ClooCommandQueue queue, T[] buf)
        {
            if (_hostBuffer.Length != buf.Length) throw new ArgumentException("Write buffer size (" + buf.Length + ") does not match the original size (" + _hostBuffer.Length + ")");

            lock (_lockWrite)
            {
                GCHandle handle = GCHandle.Alloc(buf, GCHandleType.Pinned);
                try { queue.Write(this, true, new SysIntX3(), new SysIntX3(Width, Height, 1), RowPitch, 0, handle.AddrOfPinnedObject(), null); }
                finally { handle.Free(); }
                _hostBuffer = buf;

                _modified = false;
            }
        }
    }
}
