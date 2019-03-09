#region License
/*

Copyright (c) 2010 Hans Wolff

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

namespace OpenClooVision.Imaging
{
    /// <summary>
    /// Compute image 2D for CPU only
    /// </summary>
    [CLSCompliant(false)]
    public class CpuImage2D<T> : IImage2D<T>, IDisposable where T : struct
    {
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

        /// <summary>
        /// Image height
        /// </summary>
        public int Height { get; protected set; }

        /// <summary>
        /// Image width
        /// </summary>
        public int Width { get; protected set; }

        /// <summary>
        /// Image buffer size in bytes
        /// </summary>
        public long Size { get { return Width * Height * Marshal.SizeOf(typeof(T)); } }

        /// <summary>
        /// Constructor
        /// </summary>
        protected CpuImage2D()
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="width">image width</param>
        /// <param name="height">image height</param>
        /// <param name="data">image data</param>
        protected CpuImage2D(int width, int height, T[] data = null)
        {
            Width = width;
            Height = height;

            if (data != null && data.Length > 0)
            {
                if (data.Length != width * height)
                {
                    // data doesn't have right size, just copy the bytes instead
                    _hostBuffer = new T[width * height];
                    Array.Copy(data, _hostBuffer, (_hostBuffer.Length < data.Length ? _hostBuffer.Length : data.Length));
                }
                else
                {
                    // perfect, it's the size we need, just take the whole array
                    _hostBuffer = data;
                }
            }
            else _hostBuffer = new T[width * height];
        }

        /// <summary>
        /// Free resources being used
        /// </summary>
        public void Dispose()
        {
            _hostBuffer = null;
        }
    }
}
