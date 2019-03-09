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
    /// Compute buffer for CPU only
    /// </summary>
    [CLSCompliant(false)]
    public class CpuBuffer<T> : IBuffer<T>, IDisposable where T : struct
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
        /// Buffer size in bytes
        /// </summary>
        public long Size { get { return _hostBuffer.Length * Marshal.SizeOf(typeof(T)); } }

        /// <summary>
        /// Gets the cloo image format
        /// </summary>
        public ClooImageFormat ImageFormat { get; protected set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="data">image data</param>
        public CpuBuffer(T[] data)
        {
            _hostBuffer = data;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="length">buffer size</param>
        public CpuBuffer(long length)
        {
            _hostBuffer = new T[length];
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
