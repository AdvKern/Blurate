#region License
/*

Copyright (c) 2010-2011 Hans Wolff

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
using OpenClooVision.Imaging;
using OpenClooVision.Kernels.Imaging;

namespace OpenClooVision.Kernels.ViolaJones
{
    /// <summary>
    /// CPU implementations of kernels, basically for 
    /// unit testing and comparison between GPU and CPU
    /// </summary>
    [CLSCompliant(false)]
    public class CpuProgramViolaJones : CpuProgramImaging
    {
        /// <summary>
        /// Associated Object Detector
        /// </summary>
        public BaseHaarObjectDetector ObjectDetector { get; protected set; }

        /// <summary>
        /// Constructor
        /// </summary>
        protected CpuProgramViolaJones()
        {
        }

        /// <summary>
        /// Creates an instance
        /// </summary>
        /// <returns></returns>
        public new static CpuProgramViolaJones Create()
        {
            return new CpuProgramViolaJones();
        }

        /// <summary>
        /// Creates an instance
        /// </summary>
        /// <param name="imageWidth">image width</param>
        /// <param name="imageHeight">image height</param>
        /// <returns></returns>
        public static CpuProgramViolaJones CreateFaceDetector(int imageWidth, int imageHeight)
        {
            var res = new CpuProgramViolaJones();
            res.ObjectDetector = CpuHaarObjectDetector.CreateFaceDetector(imageWidth, imageHeight);
            return res;
        }
    }
}
