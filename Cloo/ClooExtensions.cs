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
using System.Collections.Generic;
using System.Drawing;
//using System.Drawing.Imaging;
using Android.Graphics;

namespace OpenClooVision
{
    /// <summary>
    /// Extensions methods
    /// </summary>
    [CLSCompliant(false)]
    public static class ClooExtensions
    {
        /// <summary>
        /// Clears the buffer
        /// </summary>
        /// <param name="buffer">buffer to clear</param>
        public static void Clear(this byte[] buffer)
        {
            if (buffer == null) return;
            Array.Clear(buffer, 0, buffer.Length);
        }

        /// <summary>
        /// Clears the buffer
        /// </summary>
        /// <param name="buffer">buffer to clear</param>
        public static void Clear(this uint[] buffer)
        {
            if (buffer == null) return;
            Array.Clear(buffer, 0, buffer.Length);
        }

        /// <summary>
        /// Clears the buffer
        /// </summary>
        /// <param name="buffer">buffer to clear</param>
        public static void Clear(this float[] buffer)
        {
            if (buffer == null) return;
            Array.Clear(buffer, 0, buffer.Length);
        }

        /// <summary>
        /// Sets a constant value to all cells of a buffer
        /// </summary>
        /// <param name="buffer">buffer</param>
        /// <param name="value">constant value</param>
        public static void Clear(this byte[] buffer, byte value)
        {
            if (buffer == null) return;
            int length = buffer.Length;
            for (int i = 0; i < length; i++)
                buffer[i] = value;
        }

        /// <summary>
        /// Sets a constant value to all cells of a buffer
        /// </summary>
        /// <param name="buffer">buffer</param>
        /// <param name="value">constant value</param>
        public static void Clear(this float[] buffer, float value)
        {
            if (buffer == null) return;
            int length = buffer.Length;
            for (int i = 0; i < length; i++)
                buffer[i] = value;
        }

        /// <summary>
        /// Sets a constant value to all cells of a buffer
        /// </summary>
        /// <param name="buffer">buffer</param>
        /// <param name="value">constant value</param>
        public static void Clear(this uint[] buffer, uint value)
        {
            if (buffer == null) return;
            int length = buffer.Length;
            for (int i = 0; i < length; i++)
                buffer[i] = value;
        }

        /// <summary>
        /// Gets the hash code for all array elements
        /// </summary>
        /// <typeparam name="T">type</typeparam>
        /// <param name="items">items</param>
        /// <returns>hash code</returns>
        public static int GetArrayHashCode<T>(this IEnumerable<T> items)
        {
            if (items == null) return 0;
            int hashCode = 0;
            foreach (var value in items)
            {
                if (value == null) continue;
                hashCode ^= value.GetHashCode();
            }
            return hashCode;
        }

        /// <summary>
        /// Gets minimum of a buffer
        /// </summary>
        /// <param name="buffer">buffer</param>
        /// <returns>minimum value</returns>
        public static float GetMaxValue(this float[] buffer)
        {
            float max = float.MinValue;
            if ((buffer == null) || (buffer.Length == 0)) return max;
            int length = buffer.Length;
            for (int i = 0; i < length; i++)
            {
                float value = buffer[i];
                if (value > max) max = value;
            }
            return max;
        }

        /// <summary>
        /// Gets minimum value of a buffer
        /// </summary>
        /// <param name="buffer">buffer</param>
        /// <returns>minimum value</returns>
        public static float GetMinValue(this float[] buffer)
        {
            float min = float.MaxValue;
            if ((buffer == null) || (buffer.Length == 0)) return min;
            int length = buffer.Length;
            for (int i = 0; i < length; i++)
            {
                float value = buffer[i];
                if (value < min) min = value;
            }
            return min;
        }

        /// <summary>
        /// Creates a bitmap out of a histogram buffer (linear scale)
        /// </summary>
        /// <param name="buffer">histogram buffer</param>
        /// <param name="imageHeight">image height</param>
        /// <param name="lineColor">line color</param>
        /// <param name="backgroundColor">background color</param>
        /// <param name="factor">scale factor</param>
        /// <returns>histogram bitmap</returns>
        /*public unsafe static Bitmap HistogramBufferToBitmap(this uint[] buffer, int imageHeight, byte lineColor, byte backgroundColor, float factor = 0)
        {
            int width = buffer.Length;

            BitmapData bitmapData = null;
            Bitmap bitmap = new Bitmap(width, imageHeight, PixelFormat.Format24bppRgb);
            try
            {
                bitmapData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);

                byte* scan = (byte*)bitmapData.Scan0.ToPointer();
                int nStride = bitmapData.Stride;

                // determine maximum value
                if (factor <= 0)
                {
                    float maxValue = 1;
                    for (int x = 0; x < width; x++)
                    {
                        uint value = buffer[x];
                        if (value > maxValue) maxValue = value;
                    }
                    factor = maxValue;
                }

                // draw lines
                for (int x = 0; x < width; x++)
                {
                    float value = (float)buffer[x];
                    int lineHeight = (int)(value * (float)imageHeight / factor);

                    int pos = 3 * x;
                    for (int y = 0; y < imageHeight; y++)
                    {
                        byte color = (y > imageHeight - lineHeight) ? lineColor : backgroundColor;
                        scan[pos++] = color;
                        scan[pos++] = color;
                        scan[pos++] = color;
                        pos += nStride - 3;
                    }
                }
            }
            finally
            {
                if (bitmapData != null) bitmap.UnlockBits(bitmapData);
            }
            return bitmap;
        }*/
    }
}
