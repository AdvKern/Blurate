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

using System.Runtime.InteropServices;

namespace OpenClooVision.Kernels.ViolaJones
{
    /// <summary>
    /// Represents a single Haar rectangle for Viola & Jones algorithm
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct HaarRectangle
    {
        /// <summary>
        /// X start value of rectangle
        /// </summary>
        public int X;

        /// <summary>
        /// Y start value of rectangle
        /// </summary>
        public int Y;

        /// <summary>
        /// Rectangle width
        /// </summary>
        public int Width;

        /// <summary>
        /// Rectangle height
        /// </summary>
        public int Height;

        /// <summary>
        /// Rectangle weight
        /// </summary>
        public float Weight;

        /// <summary>
        /// Scaled X start value of rectangle
        /// </summary>
        public int ScaledX;

        /// <summary>
        /// Scaled Y start value of rectangle
        /// </summary>
        public int ScaledY;

        /// <summary>
        /// Scaled rectangle width
        /// </summary>
        public int ScaledWidth;

        /// <summary>
        /// Scaled rectangle height
        /// </summary>
        public int ScaledHeight;

        /// <summary>
        /// Scaled weight
        /// </summary>
        public float ScaledWeight;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="x">rectangle start x coordinate</param>
        /// <param name="y">rectangle start y coordinate</param>
        /// <param name="w">rectangle width</param>
        /// <param name="h">rectangle height</param>
        /// <param name="weight">rectangle weight</param>
        public HaarRectangle(int x, int y, int w, int h, float weight)
        {
            X = x;
            Y = y;
            Width = w;
            Height = h;
            Weight = weight;
            ScaledX = 0;
            ScaledY = 0;
            ScaledWidth = 0;
            ScaledHeight = 0;
            ScaledWeight = 0;
        }
    }
}
