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
    public class CpuImage2DRgbA<T> : CpuImage2D<T> where T : struct
    {
        /// <summary>
        /// Image buffer size in bytes
        /// </summary>
        public new long Size { get { return 4 * Width * Height * Marshal.SizeOf(typeof(T)); } }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="width">image width</param>
        /// <param name="height">image height</param>
        /// <param name="data">image data</param>
        protected CpuImage2DRgbA(int width, int height, T[] data = null)
        {
            Width = width;
            Height = height;

            if (data != null && data.Length > 0)
            {
                if (data.Length != 4 * width * height)
                {
                    // data doesn't have right size, just copy the bytes instead
                    _hostBuffer = new T[4 * width * height];
                    Array.Copy(data, _hostBuffer, (_hostBuffer.Length < data.Length ? _hostBuffer.Length : data.Length));
                }
                else
                {
                    // perfect, it's the size we need, just take the whole array
                    _hostBuffer = data;
                }
            }
            else _hostBuffer = new T[4 * width * height];
        }
    }
}
