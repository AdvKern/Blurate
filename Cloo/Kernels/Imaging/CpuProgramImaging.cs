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

namespace OpenClooVision.Kernels.Imaging
{
    /// <summary>
    /// CPU implementations of kernels, basically for 
    /// unit testing and comparison between GPU and CPU
    /// </summary>
    [CLSCompliant(false)]
    public class CpuProgramImaging : CpuProgramCore
    {
        /// <summary>
        /// Constructor
        /// </summary>
        protected CpuProgramImaging()
        {
        }

        /// <summary>
        /// Creates an instance
        /// </summary>
        /// <returns></returns>
        public new static CpuProgramImaging Create()
        {
            return new CpuProgramImaging();
        }

        /// <summary>
        /// Makes all values absolute
        /// </summary>
        /// <param name="queue">cloo command queue</param>
        /// <param name="source">image source</param>
        /// <param name="dest">image destination</param>
        public void Abs(IImage2DFloat source, IImage2DFloat dest)
        {
            if (source == null) throw new ArgumentNullException("source");
            if (dest == null) throw new ArgumentNullException("dest");
            if ((source.Width > dest.Width) || (source.Height > dest.Height)) throw new ArgumentException("Destination image (" + dest.Width + "x" + dest.Height + ") must have at least the same size as the source image (" + source.Width + "x" + source.Height + ")");

            int length = source.HostBuffer.Length;
            for (int i = 0; i < length; i++)
                dest.HostBuffer[i] = Math.Abs(source.HostBuffer[i]);
            dest.Normalized = source.Normalized;
        }

        /// <summary>
        /// Adds a constant value to all float values in an image
        /// </summary>
        /// <param name="source">image source</param>
        /// <param name="dest">image destination</param>
        /// <param name="value">value to add</param>
        public void AddValue(IImage2DFloat source, IImage2DFloat dest, float value)
        {
            if (source == null) throw new ArgumentNullException("source");
            if (dest == null) throw new ArgumentNullException("dest");
            if ((source.Width > dest.Width) || (source.Height > dest.Height)) throw new ArgumentException("Destination image (" + dest.Width + "x" + dest.Height + ") must have at least the same size as the source image (" + source.Width + "x" + source.Height + ")");

            int length = source.HostBuffer.Length;
            for (int i = 0; i < length; i++)
                dest.HostBuffer[i] = source.HostBuffer[i] + value;
            dest.Normalized = source.Normalized;
        }

        /// <summary>
        /// Box blur image
        /// </summary>
        /// <param name="source">image source</param>
        /// <param name="dest">image destination</param>
        /// <param name="sampler">sampler to be used for image reading</param>
        /// <param name="offset">offset</param>
        public void BoxBlur(IImage2DFloatRgbA source, IImage2DFloatRgbA dest, int offset)
        {
            if (source == null) throw new ArgumentNullException("source");
            if (dest == null) throw new ArgumentNullException("dest");
            if ((source.Width > dest.Width) || (source.Height > dest.Height)) throw new ArgumentException("Destination image (" + dest.Width + "x" + dest.Height + ") must have at least the same size as the source image (" + source.Width + "x" + source.Height + ")");

            // TODO: Box blur image
            dest.Normalized = source.Normalized;
        }

        /// <summary>
        /// Convert gray byte image to RGB byte image
        /// </summary>
        /// <param name="source">image source</param>
        /// <param name="dest">image destination</param>
        public void ByteAToByteRgbA(IImage2DByteA source, IImage2DByteRgbA dest)
        {
            if (source == null) throw new ArgumentNullException("source");
            if (dest == null) throw new ArgumentNullException("dest");
            if ((source.Width > dest.Width) || (source.Height > dest.Height)) throw new ArgumentException("Destination image (" + dest.Width + "x" + dest.Height + ") must have at least the same size as the source image (" + source.Width + "x" + source.Height + ")");

            int length = source.HostBuffer.Length;
            int pos = 0;
            for (int i = 0; i < length; i++)
            {
                byte val = source.HostBuffer[i];
                dest.HostBuffer[pos++] = val;
                dest.HostBuffer[pos++] = val;
                dest.HostBuffer[pos++] = val;
                dest.HostBuffer[pos++] = 255;
            }
        }

        /// <summary>
        /// Convert byte image to float image
        /// </summary>
        /// <param name="source">image source</param>
        /// <param name="dest">image destination</param>
        public void ByteToFloat(IImage2DByte source, IImage2DFloat dest)
        {
            if (source == null) throw new ArgumentNullException("source");
            if (dest == null) throw new ArgumentNullException("dest");
            if ((source.Width > dest.Width) || (source.Height > dest.Height)) throw new ArgumentException("Destination image (" + dest.Width + "x" + dest.Height + ") must have at least the same size as the source image (" + source.Width + "x" + source.Height + ")");

            int length = source.HostBuffer.Length;
            for (int i = 0; i < length; i++)
                dest.HostBuffer[i] = (float)source.HostBuffer[i];
            dest.Normalized = false;
        }

        /// <summary>
        /// Clamps minimum and maximum value
        /// </summary>
        /// <param name="source">image source</param>
        /// <param name="dest">image destination</param>
        /// <param name="sampler">sampler to be used for image reading</param>
        /// <param name="minValue">minimum value</param>
        /// <param name="maxValue">maximum value</param>
        public void Clamp(IImage2DFloat source, IImage2DFloat dest, float minValue, float maxValue)
        {
            if (source == null) throw new ArgumentNullException("source");
            if (dest == null) throw new ArgumentNullException("dest");
            if ((source.Width > dest.Width) || (source.Height > dest.Height)) throw new ArgumentException("Destination image (" + dest.Width + "x" + dest.Height + ") must have at least the same size as the source image (" + source.Width + "x" + source.Height + ")");

            int length = source.HostBuffer.Length;
            for (int i = 0; i < length; i++)
            {
                float val = source.HostBuffer[i];
                if (val < minValue) val = minValue;
                else
                    if (val > maxValue) val = maxValue;
                dest.HostBuffer[i] = val;
            }
            dest.Normalized = source.Normalized;
        }

        /// <summary>
        /// Denormalize float image
        /// </summary>
        /// <param name="source">image source</param>
        /// <param name="dest">image destination</param>
        public void Denormalize(IImage2DFloat source, IImage2DFloat dest)
        {
            if (source == null) throw new ArgumentNullException("source");
            if (dest == null) throw new ArgumentNullException("dest");
            if ((source.Width > dest.Width) || (source.Height > dest.Height)) throw new ArgumentException("Destination image (" + dest.Width + "x" + dest.Height + ") must have at least the same size as the source image (" + source.Width + "x" + source.Height + ")");

            int length = source.HostBuffer.Length;
            for (int i = 0; i < length; i++)
                dest.HostBuffer[i] = 255 * source.HostBuffer[i];

            dest.Normalized = false;
        }

        /// <summary>
        /// Extracts a channel of an RGB image
        /// </summary>
        /// <param name="source">image source</param>
        /// <param name="dest">image destination</param>
        /// <param name="offset">offset (0..3)</param>
        public void ExtractChannel(IImage2DByteRgbA source, IImage2DByteA dest, byte offset)
        {
            if (source == null) throw new ArgumentNullException("source");
            if (dest == null) throw new ArgumentNullException("dest");
            if (offset > 3) throw new ArgumentOutOfRangeException("offset", String.Format("offset must be between 0..3 but was {0}", offset));
            if ((source.Width > dest.Width) || (source.Height > dest.Height)) throw new ArgumentException("Destination image (" + dest.Width + "x" + dest.Height + ") must have at least the same size as the source image (" + source.Width + "x" + source.Height + ")");

            int length = source.Width * source.Height;
            int pos = 0;
            for (int i = 0; i < length; i++)
                for (int j = 0; j < 4; j++)
                    if (j == offset) dest.HostBuffer[i] = source.HostBuffer[pos++]; else pos++;
        }

        /// <summary>
        /// Extracts a channel of an RGB image
        /// </summary>
        /// <param name="source">image source</param>
        /// <param name="dest">image destination</param>
        /// <param name="offset">offset (0..3)</param>
        public void ExtractChannel(IImage2DFloatRgbA source, IImage2DFloatA dest, byte offset)
        {
            if (source == null) throw new ArgumentNullException("source");
            if (dest == null) throw new ArgumentNullException("dest");
            if (offset > 3) throw new ArgumentOutOfRangeException("offset", String.Format("offset must be between 0..3 but was {0}", offset));
            if ((source.Width > dest.Width) || (source.Height > dest.Height)) throw new ArgumentException("Destination image (" + dest.Width + "x" + dest.Height + ") must have at least the same size as the source image (" + source.Width + "x" + source.Height + ")");

            int length = source.Width * source.Height;
            int pos = 0;
            for (int i = 0; i < length; i++)
                for (int j = 0; j < 4; j++)
                    if (j == offset) dest.HostBuffer[i] = source.HostBuffer[pos++]; else pos++;

            dest.Normalized = source.Normalized;
        }

        /// <summary>
        /// Flip image X coordinate
        /// </summary>
        /// <param name="source">image source</param>
        /// <param name="dest">image destination</param>
        public void FlipX(IImage2DByteA source, IImage2DByteA dest)
        {
            if (source == null) throw new ArgumentNullException("source");
            if (dest == null) throw new ArgumentNullException("dest");
            if (source == dest) throw new ArgumentException("Flipping kernel is not designed to run inline therefore source and destination must be different images");
            if ((source.Width > dest.Width) || (source.Height > dest.Height)) throw new ArgumentException("Destination image (" + dest.Width + "x" + dest.Height + ") must have at least the same size as the source image (" + source.Width + "x" + source.Height + ")");

            var sBuf = source.HostBuffer;
            var dBuf = dest.HostBuffer;

            int posSource = 0;
            int posDest = source.Width;

            for (int y = 0, h = source.Height; y < h; y++)
            {
                for (int x = 0, w = source.Width; x < w; x++)
                {
                    byte color = sBuf[posSource++];
                    dBuf[--posDest] = color;
                }
                posDest += dest.Width + source.Width;
            }
        }

        /// <summary>
        /// Flip image X coordinate
        /// </summary>
        /// <param name="source">image source</param>
        /// <param name="dest">image destination</param>
        public void FlipX(IImage2DByteRgbA source, IImage2DByteRgbA dest)
        {
            if (source == null) throw new ArgumentNullException("source");
            if (dest == null) throw new ArgumentNullException("dest");
            if (source == dest) throw new ArgumentException("Flipping kernel is not designed to run inline therefore source and destination must be different images");
            if ((source.Width > dest.Width) || (source.Height > dest.Height)) throw new ArgumentException("Destination image (" + dest.Width + "x" + dest.Height + ") must have at least the same size as the source image (" + source.Width + "x" + source.Height + ")");

            var sBuf = source.HostBuffer;
            var dBuf = dest.HostBuffer;

            int posSource = 0;
            int posDest = 4 * source.Width;

            for (int y = 0, h = source.Height; y < h; y++)
            {
                for (int x = 0, w = source.Width; x < w; x++)
                {
                    byte color;
                    posDest -= 4;
                    color = sBuf[posSource++]; dBuf[posDest++] = color;
                    color = sBuf[posSource++]; dBuf[posDest++] = color;
                    color = sBuf[posSource++]; dBuf[posDest++] = color;
                    color = sBuf[posSource++]; dBuf[posDest++] = color;
                    posDest -= 4;
                }
                posDest += 4 * (dest.Width + source.Width);
            }
        }

        /// <summary>
        /// Flip image X coordinate
        /// </summary>
        /// <param name="source">image source</param>
        /// <param name="dest">image destination</param>
        public void FlipX(IImage2DFloatA source, IImage2DFloatA dest)
        {
            if (source == null) throw new ArgumentNullException("source");
            if (dest == null) throw new ArgumentNullException("dest");
            if (source == dest) throw new ArgumentException("Flipping kernel is not designed to run inline therefore source and destination must be different images");
            if ((source.Width > dest.Width) || (source.Height > dest.Height)) throw new ArgumentException("Destination image (" + dest.Width + "x" + dest.Height + ") must have at least the same size as the source image (" + source.Width + "x" + source.Height + ")");

            var sBuf = source.HostBuffer;
            var dBuf = dest.HostBuffer;

            int posSource = 0;
            int posDest = source.Width;

            for (int y = 0, h = source.Height; y < h; y++)
            {
                for (int x = 0, w = source.Width; x < w; x++)
                {
                    float color = sBuf[posSource++];
                    dBuf[--posDest] = color;
                }
                posDest += dest.Width + source.Width;
            }
        }

        /// <summary>
        /// Flip image X coordinate
        /// </summary>
        /// <param name="source">image source</param>
        /// <param name="dest">image destination</param>
        public void FlipX(IImage2DFloatRgbA source, IImage2DFloatRgbA dest)
        {
            if (source == null) throw new ArgumentNullException("source");
            if (dest == null) throw new ArgumentNullException("dest");
            if (source == dest) throw new ArgumentException("Flipping kernel is not designed to run inline therefore source and destination must be different images");
            if ((source.Width > dest.Width) || (source.Height > dest.Height)) throw new ArgumentException("Destination image (" + dest.Width + "x" + dest.Height + ") must have at least the same size as the source image (" + source.Width + "x" + source.Height + ")");

            var sBuf = source.HostBuffer;
            var dBuf = dest.HostBuffer;

            int posSource = 0;
            int posDest = 4 * source.Width;

            for (int y = 0, h = source.Height; y < h; y++)
            {
                for (int x = 0, w = source.Width; x < w; x++)
                {
                    float color;
                    posDest -= 4;
                    color = sBuf[posSource++]; dBuf[posDest++] = color;
                    color = sBuf[posSource++]; dBuf[posDest++] = color;
                    color = sBuf[posSource++]; dBuf[posDest++] = color;
                    color = sBuf[posSource++]; dBuf[posDest++] = color;
                    posDest -= 4;
                }
                posDest += 4 * (dest.Width + source.Width);
            }
        }

        /// <summary>
        /// Flip image Y coordinate
        /// </summary>
        /// <param name="source">image source</param>
        /// <param name="dest">image destination</param>
        public void FlipY(IImage2DByteA source, IImage2DByteA dest)
        {
            if (source == null) throw new ArgumentNullException("source");
            if (dest == null) throw new ArgumentNullException("dest");
            if (source == dest) throw new ArgumentException("Flipping kernel is not designed to run inline therefore source and destination must be different images");
            if ((source.Width > dest.Width) || (source.Height > dest.Height)) throw new ArgumentException("Destination image (" + dest.Width + "x" + dest.Height + ") must have at least the same size as the source image (" + source.Width + "x" + source.Height + ")");

            var sBuf = source.HostBuffer;
            var dBuf = dest.HostBuffer;

            int posSource = 0;
            int posDest = (source.Height - 1) * dest.Width;

            for (int y = 0, h = source.Height; y < h; y++)
            {
                for (int x = 0, w = source.Width; x < w; x++)
                {
                    byte color = sBuf[posSource++];
                    dBuf[posDest++] = color;
                }
                posDest -= dest.Width + source.Width;
            }
        }

        /// <summary>
        /// Flip image Y coordinate
        /// </summary>
        /// <param name="source">image source</param>
        /// <param name="dest">image destination</param>
        public void FlipY(IImage2DByteRgbA source, IImage2DByteRgbA dest)
        {
            if (source == null) throw new ArgumentNullException("source");
            if (dest == null) throw new ArgumentNullException("dest");
            if (source == dest) throw new ArgumentException("Flipping kernel is not designed to run inline therefore source and destination must be different images");
            if ((source.Width > dest.Width) || (source.Height > dest.Height)) throw new ArgumentException("Destination image (" + dest.Width + "x" + dest.Height + ") must have at least the same size as the source image (" + source.Width + "x" + source.Height + ")");

            var sBuf = source.HostBuffer;
            var dBuf = dest.HostBuffer;

            int posSource = 0;
            int posDest = 4 * (source.Height - 1) * dest.Width;

            for (int y = 0, h = source.Height; y < h; y++)
            {
                for (int x = 0, w = source.Width; x < w; x++)
                {
                    byte color;
                    color = sBuf[posSource++]; dBuf[posDest++] = color;
                    color = sBuf[posSource++]; dBuf[posDest++] = color;
                    color = sBuf[posSource++]; dBuf[posDest++] = color;
                    color = sBuf[posSource++]; dBuf[posDest++] = color;
                }
                posDest -= 4 * (dest.Width + source.Width);
            }
        }

        /// <summary>
        /// Flip image Y coordinate
        /// </summary>
        /// <param name="source">image source</param>
        /// <param name="dest">image destination</param>
        public void FlipY(IImage2DFloatA source, IImage2DFloatA dest)
        {
            if (source == null) throw new ArgumentNullException("source");
            if (dest == null) throw new ArgumentNullException("dest");
            if (source == dest) throw new ArgumentException("Flipping kernel is not designed to run inline therefore source and destination must be different images");
            if ((source.Width > dest.Width) || (source.Height > dest.Height)) throw new ArgumentException("Destination image (" + dest.Width + "x" + dest.Height + ") must have at least the same size as the source image (" + source.Width + "x" + source.Height + ")");

            var sBuf = source.HostBuffer;
            var dBuf = dest.HostBuffer;

            int posSource = 0;
            int posDest = (source.Height - 1) * dest.Width;

            for (int y = 0, h = source.Height; y < h; y++)
            {
                for (int x = 0, w = source.Width; x < w; x++)
                {
                    float color = sBuf[posSource++];
                    dBuf[posDest++] = color;
                }
                posDest -= dest.Width + source.Width;
            }
        }

        /// <summary>
        /// Flip image Y coordinate
        /// </summary>
        /// <param name="source">image source</param>
        /// <param name="dest">image destination</param>
        public void FlipY(IImage2DFloatRgbA source, IImage2DFloatRgbA dest)
        {
            if (source == null) throw new ArgumentNullException("source");
            if (dest == null) throw new ArgumentNullException("dest");
            if (source == dest) throw new ArgumentException("Flipping kernel is not designed to run inline therefore source and destination must be different images");
            if ((source.Width > dest.Width) || (source.Height > dest.Height)) throw new ArgumentException("Destination image (" + dest.Width + "x" + dest.Height + ") must have at least the same size as the source image (" + source.Width + "x" + source.Height + ")");

            var sBuf = source.HostBuffer;
            var dBuf = dest.HostBuffer;

            int posSource = 0;
            int posDest = 4 * (source.Height - 1) * dest.Width;

            for (int y = 0, h = source.Height; y < h; y++)
            {
                for (int x = 0, w = source.Width; x < w; x++)
                {
                    float color;
                    color = sBuf[posSource++]; dBuf[posDest++] = color;
                    color = sBuf[posSource++]; dBuf[posDest++] = color;
                    color = sBuf[posSource++]; dBuf[posDest++] = color;
                    color = sBuf[posSource++]; dBuf[posDest++] = color;
                }
                posDest -= 4 * (dest.Width + source.Width);
            }
        }

        /// <summary>
        /// Convert float image to byte image
        /// </summary>
        /// <param name="source">image source</param>
        /// <param name="dest">image destination</param>
        public void FloatToByte(IImage2DFloat source, IImage2DByte dest)
        {
            if (source == null) throw new ArgumentNullException("source");
            if (dest == null) throw new ArgumentNullException("dest");
            if ((source.Width > dest.Width) || (source.Height > dest.Height)) throw new ArgumentException("Destination image (" + dest.Width + "x" + dest.Height + ") must have at least the same size as the source image (" + source.Width + "x" + source.Height + ")");

            if (source.Normalized)
            {
                // use normalized kernel
                int length = source.HostBuffer.Length;
                for (int i = 0; i < length; i++)
                {
                    float val = 255f * source.HostBuffer[i];
                    if (val < 0) val = 0;
                    else
                        if (val > 255) val = 255;
                    dest.HostBuffer[i] = (byte)val;
                }
            }
            else
            {
                // use normal kernel
                int length = source.HostBuffer.Length;
                for (int i = 0; i < length; i++)
                {
                    float val = source.HostBuffer[i];
                    if (val < 0) val = 0;
                    else
                        if (val > 255) val = 255;
                    dest.HostBuffer[i] = (byte)val;
                }
            }
        }

        /// <summary>
        /// GrayScale image
        /// </summary>
        /// <param name="source">image source</param>
        /// <param name="dest">image destination</param>
        public void GrayScale(IImage2DByteRgbA source, IImage2DByteA dest)
        {
            if (source == null) throw new ArgumentNullException("source");
            if (dest == null) throw new ArgumentNullException("dest");
            if ((source.Width > dest.Width) || (source.Height > dest.Height)) throw new ArgumentException("Destination image (" + dest.Width + "x" + dest.Height + ") must have at least the same size as the source image (" + source.Width + "x" + source.Height + ")");

            int length = source.Width * source.Height;
            int pos = 0;
            for (int i = 0; i < length; i++)
            {
                float gray = 0.2989f * (float)source.HostBuffer[pos++] + 0.5870f * (float)source.HostBuffer[pos++] + 0.1140f * (float)source.HostBuffer[pos++];
                pos++;
                dest.HostBuffer[i] = (byte)gray;
            }
        }

        /// <summary>
        /// GrayScale image
        /// </summary>
        /// <param name="source">image source</param>
        /// <param name="dest">image destination</param>
        public void GrayScale(IImage2DFloatRgbA source, IImage2DFloatA dest)
        {
            if (source == null) throw new ArgumentNullException("source");
            if (dest == null) throw new ArgumentNullException("dest");
            if ((source.Width > dest.Width) || (source.Height > dest.Height)) throw new ArgumentException("Destination image (" + dest.Width + "x" + dest.Height + ") must have at least the same size as the source image (" + source.Width + "x" + source.Height + ")");

            int length = source.Width * source.Height;
            int pos = 0;
            for (int i = 0; i < length; i++)
            {
                float gray = 0.2989f * source.HostBuffer[pos++] + 0.5870f * source.HostBuffer[pos++] + 0.1140f * source.HostBuffer[pos++];
                pos++;
                dest.HostBuffer[i] = gray;
            }
            dest.Normalized = source.Normalized;
        }

        /// <summary>
        /// Create histogram
        /// </summary>
        /// <param name="source">image source</param>
        /// <param name="histogram">byte buffer (256 bytes)</param>
        public void Histogram256(IImage2DByteA source, IBuffer<uint> histogram)
        {
            Histogram256(source, histogram.HostBuffer);
        }

        /// <summary>
        /// Create histogram
        /// </summary>
        /// <param name="source">image source</param>
        /// <param name="histogram">byte buffer (256 bytes)</param>
        public void Histogram256(IImage2DByteA source, uint[] histogram)
        {
            if (source == null) throw new ArgumentNullException("source");
            if (histogram == null) throw new ArgumentNullException("histogram");

            if (histogram.Length < 256) throw new ArgumentException("Buffer size for histogram must be at least 256 bytes", "histogram");

            for (int i = 0; i < 256; i++) histogram[i] = 0;
            for (int i = 0, length = source.HostBuffer.Length; i < length; i++)
                histogram[source.HostBuffer[i]]++;
        }

        /// <summary>
        /// Create histogram
        /// </summary>
        /// <param name="source">image source</param>
        /// <param name="histogram">byte buffer (256 bytes)</param>
        public void Histogram256(IImage2DByteA source, IBuffer<uint> histogram, int startX, int startY = 0, int width = 0, int height = 0)
        {
            Histogram256(source, histogram.HostBuffer, startX, startY, width, height);
        }

        /// <summary>
        /// Create histogram
        /// </summary>
        /// <param name="source">image source</param>
        /// <param name="histogram">byte buffer (256 bytes)</param>
        public void Histogram256(IImage2DByteA source, uint[] histogram, int startX, int startY = 0, int width = 0, int height = 0)
        {
            if (source == null) throw new ArgumentNullException("source");
            if (histogram == null) throw new ArgumentNullException("histogram");

            if (histogram.Length < 256) throw new ArgumentException("Buffer size for histogram must be at least 256 bytes", "histogram");

            if (width == 0) width = source.Width - startX;
            if (height == 0) height = source.Height - startY;

            for (int i = 0; i < 256; i++) histogram[i] = 0;

            for (int y = startY, yh = startY + height; y < yh; y++)
            {
                int pos = y * source.Width + startX;
                for (int x = startX, xw = startX + width; x < xw; x++)
                    histogram[source.HostBuffer[pos++]]++;
            }
        }

        /// <summary>
        /// Create color histogram with N bins
        /// </summary>
        /// <param name="source">image source</param>
        /// <param name="histogram">byte buffer (256 bytes)</param>
        /// <param name="bins">number of bins</param>
        public void HistogramN(IImage2DByteRgbA source, IBuffer<uint> histogram,
            byte bins)
        {
            HistogramN(source, histogram.HostBuffer, bins);
        }

        /// <summary>
        /// Create color histogram with N bins
        /// </summary>
        /// <param name="source">image source</param>
        /// <param name="histogram">byte buffer (256 bytes)</param>
        /// <param name="bins">number of bins</param>
        public void HistogramN(IImage2DByteRgbA source, uint[] histogram,
            byte bins)
        {
            if (source == null) throw new ArgumentNullException("source");
            if (histogram == null) throw new ArgumentNullException("histogram");

            int size = bins * bins * bins;
            if (histogram.Length < size) throw new ArgumentException("Buffer size for histogram must be at least " + size + " bytes", "histogram");

            for (int i = 0; i < size; i++) histogram[i] = 0;

            int div = 256 / bins;
            int pos = 0;
            int bins2 = bins * bins;
            for (int i = 0, length = source.Width * source.Height; i < length; i++)
            {
                byte r = (byte)(source.HostBuffer[pos++] / div);
                byte g = (byte)(source.HostBuffer[pos++] / div);
                byte b = (byte)(source.HostBuffer[pos++] / div);
                pos++;
                histogram[r + g * bins + b * bins2]++;
            }
        }

                /// <summary>
        /// Create color histogram with N bins
        /// </summary>
        /// <param name="source">image source</param>
        /// <param name="histogram">byte buffer (256 bytes)</param>
        /// <param name="bins">number of bins</param>
        /// <param name="startX">start from this X coordinate</param>
        /// <param name="startY">start from this Y coordinate</param>
        /// <param name="width">width</param>
        /// <param name="height">height</param>
        public void HistogramN(IImage2DByteRgbA source, IBuffer<uint> histogram,
            byte bins, int startX, int startY = 0, int width = 0, int height = 0)
        {
            HistogramN(source, histogram.HostBuffer, bins, startX, startY, width, height);
        }

        /// <summary>
        /// Create color histogram with N bins
        /// </summary>
        /// <param name="source">image source</param>
        /// <param name="histogram">byte buffer (256 bytes)</param>
        /// <param name="bins">number of bins</param>
        /// <param name="startX">start from this X coordinate</param>
        /// <param name="startY">start from this Y coordinate</param>
        /// <param name="width">width</param>
        /// <param name="height">height</param>
        public void HistogramN(IImage2DByteRgbA source, uint[] histogram,
            byte bins, int startX, int startY = 0, int width = 0, int height = 0)
        {
            if (source == null) throw new ArgumentNullException("source");
            if (histogram == null) throw new ArgumentNullException("histogram");

            int size = bins * bins * bins;
            if (histogram.Length < size) throw new ArgumentException("Buffer size for histogram must be at least " + size + " bytes", "histogram");
            if (width == 0) width = source.Width - startX;
            if (height == 0) height = source.Height - startY;
            if (width < 0) throw new ArgumentOutOfRangeException("width");
            if (height < 0) throw new ArgumentOutOfRangeException("height");

            for (int i = 0; i < size; i++) histogram[i] = 0;

            int div = 256 / bins;
            int bins2 = bins * bins;
            for (int y = startY, yh = startY + height; y < yh; y++)
            {
                int pos = (y * source.Width + startX) * 4;
                for (int x = startX, xw = startX + width; x < xw; x++)
                {
                    byte r = (byte)(source.HostBuffer[pos++] / div);
                    byte g = (byte)(source.HostBuffer[pos++] / div);
                    byte b = (byte)(source.HostBuffer[pos++] / div);
                    pos++;
                    histogram[r + g * bins + b * bins2]++;
                }
            }
        }

        /// <summary>
        /// Convert an float image to an integral image
        /// </summary>
        /// <remarks>
        /// We skip the last line to keep the original size (better performance)
        /// </remarks>
        /// <param name="source">image source</param>
        /// <param name="dest">image destination</param>
        public void Integral(IImage2DFloatA source, IImage2DFloatA dest)
        {
            if (source == null) throw new ArgumentNullException("source");
            if (dest == null) throw new ArgumentNullException("dest");
            if ((source.Width > dest.Width) || (source.Height > dest.Height)) throw new ArgumentException("Destination image (" + dest.Width + "x" + dest.Height + ") must have at least the source image width and height but is only " + source.Width + "x" + source.Height);

            int height = source.Height;
            int width = source.Width;
            float[] sBuf = source.HostBuffer;
            float[] dBuf = dest.HostBuffer;

            for (int x = 0; x < width; x++) dest.HostBuffer[x] = 0; // zero first line
            for (int y = 0, a = width * height; y < a; y += width) dest.HostBuffer[y] = 0; // zero first column

            // for each line
            for (int y = 1; y < height; y++)
            {
                int yIndex = y * width;
                int y1Index = yIndex - width; // last

                // for each pixel
                for (int x = 1; x < width; x++)
                {
                    float p = sBuf[y1Index + x - 1];
                    dBuf[yIndex + x] = p + dBuf[yIndex + x - 1] + dBuf[y1Index + x] - dBuf[y1Index + x - 1];
                }
            }
            dest.Normalized = false;
        }

        /// <summary>
        /// Convert an float image to an integral image
        /// </summary>
        /// <remarks>
        /// We skip the last line to keep the original size (better performance)
        /// </remarks>
        /// <param name="source">image source</param>
        /// <param name="dest">image destination</param>
        public void Integral(IImage2DByteA source, IImage2DUIntA dest)
        {
            if (source == null) throw new ArgumentNullException("source");
            if (dest == null) throw new ArgumentNullException("dest");
            if ((source.Width > dest.Width) || (source.Height > dest.Height)) throw new ArgumentException("Destination image (" + dest.Width + "x" + dest.Height + ") must have at least the source image width and height but is only " + source.Width + "x" + source.Height);

            int heightS = source.Height;
            int widthS = source.Width;
            int heightD = dest.Height;
            int widthD = dest.Width;
            byte[] sBuf = source.HostBuffer;
            uint[] dBuf = dest.HostBuffer;

            for (int x = 0; x < widthD; x++) dest.HostBuffer[x] = 0; // zero first line
            for (int y = 0, a = widthD * heightD; y < a; y += widthD) dest.HostBuffer[y] = 0; // zero first column

            int y1IndexS = 0;
            int yIndexD = widthD;
            int y1IndexD = 0; // last

            // for each line
            for (int y = 1; y < heightS; y++)
            {
                // for each pixel
                for (int x = 1; x < widthS; x++)
                {
                    uint p = sBuf[y1IndexS + x - 1];
                    dBuf[yIndexD + x] = p + dBuf[yIndexD + x - 1] + dBuf[y1IndexD + x] - dBuf[y1IndexD + x - 1];
                }

                yIndexD += widthD;
                y1IndexS += widthS;
                y1IndexD += widthD;
            }
        }

        /// <summary>
        /// Convert an float image to a squared integral image
        /// </summary>
        /// <remarks>
        /// We skip the last line to keep the original size (better performance)
        /// </remarks>
        /// <param name="source">image source</param>
        /// <param name="dest">image destination</param>
        public void IntegralSquare(IImage2DFloatA source, IImage2DFloatA dest)
        {
            if (source == null) throw new ArgumentNullException("source");
            if (dest == null) throw new ArgumentNullException("dest");
            if ((source.Width > dest.Width) || (source.Height > dest.Height)) throw new ArgumentException("Destination image (" + dest.Width + "x" + dest.Height + ") must have at least the source image width and height but is only " + source.Width + "x" + source.Height);

            int heightS = source.Height;
            int widthS = source.Width;
            int heightD = dest.Height;
            int widthD = dest.Width;
            float[] sBuf = source.HostBuffer;
            float[] dBuf = dest.HostBuffer;

            for (int x = 0; x < widthD; x++) dest.HostBuffer[x] = 0; // zero first line
            for (int y = 0, a = widthD * heightD; y < a; y += widthD) dest.HostBuffer[y] = 0; // zero first column

            int y1IndexS = 0;
            int yIndexD = widthD;
            int y1IndexD = 0; // last

            // for each line
            for (int y = 1; y < heightS; y++)
            {
                // for each pixel
                for (int x = 1; x < widthS; x++)
                {
                    float p = sBuf[y1IndexS + x - 1];
                    dBuf[yIndexD + x] = p * p + dBuf[yIndexD + x - 1] + dBuf[y1IndexD + x] - dBuf[y1IndexD + x - 1];
                }

                yIndexD += widthD;
                y1IndexS += widthS;
                y1IndexD += widthD;
            }
            dest.Normalized = false;
        }

        /// <summary>
        /// Convert an float image to a squared integral image
        /// </summary>
        /// <remarks>
        /// We skip the last line to keep the original size (better performance)
        /// </remarks>
        /// <param name="source">image source</param>
        /// <param name="dest">image destination</param>
        public void IntegralSquare(IImage2DByteA source, IImage2DUIntA dest)
        {
            if (source == null) throw new ArgumentNullException("source");
            if (dest == null) throw new ArgumentNullException("dest");
            if ((source.Width > dest.Width) || (source.Height > dest.Height)) throw new ArgumentException("Destination image (" + dest.Width + "x" + dest.Height + ") must have at least the source image width and height but is only " + source.Width + "x" + source.Height);

            int height = source.Height;
            int width = source.Width;
            byte[] sBuf = source.HostBuffer;
            uint[] dBuf = dest.HostBuffer;

            for (int x = 0; x < width; x++) dest.HostBuffer[x] = 0; // zero first line
            for (int y = 0, a = width * height; y < a; y += width) dest.HostBuffer[y] = 0; // zero first column

            // for each line
            int ySumIndex = 0;
            int y1SumIndex;
            int yIndex = 0;
            for (int y = 1; y < height; y++)
            {
                y1SumIndex = ySumIndex;
                ySumIndex += width;

                // for each pixel
                for (int x = 1; x < width; x++)
                {
                    uint p = sBuf[yIndex + x - 1];
                    dBuf[ySumIndex + x] = p * p + dBuf[ySumIndex + x - 1] + dBuf[y1SumIndex + x] - dBuf[y1SumIndex + x - 1];
                }
                yIndex += width;
            }
        }

        /// <summary>
        /// HSL to RGB image
        /// </summary>
        /// <param name="source">image source</param>
        /// <param name="dest">image destination</param>
        public void HslToRgb(IImage2DFloatRgbA source, IImage2DFloatRgbA dest)
        {
            if (source == null) throw new ArgumentNullException("source");
            if (dest == null) throw new ArgumentNullException("dest");
            if ((source.Width > dest.Width) || (source.Height > dest.Height)) throw new ArgumentException("Destination image (" + dest.Width + "x" + dest.Height + ") must have at least the same size as the source image (" + source.Width + "x" + source.Height + ")");

            if (source.Normalized)
            {
                // use normalized kernel
                // TODO: HSL to RGB image (normalized)
            }
            else
            {
                // use normal kernel
                // TODO: HSL to RGB image
            }
            dest.Normalized = source.Normalized;
        }

        /// <summary>
        /// Multiplies a constant value to all float values in an image
        /// </summary>
        /// <param name="source">image source</param>
        /// <param name="dest">image destination</param>
        /// <param name="factor">factor to multiply</param>
        public void MultiplyValue(IImage2DFloat source, IImage2DFloat dest, float factor)
        {
            if (source == null) throw new ArgumentNullException("source");
            if (dest == null) throw new ArgumentNullException("dest");
            if ((source.Width > dest.Width) || (source.Height > dest.Height)) throw new ArgumentException("Destination image (" + dest.Width + "x" + dest.Height + ") must have at least the same size as the source image (" + source.Width + "x" + source.Height + ")");

            int length = source.HostBuffer.Length;
            for (int i = 0; i < length; i++)
                dest.HostBuffer[i] = source.HostBuffer[i] * factor;
            dest.Normalized = source.Normalized;
        }

        /// <summary>
        /// Normalize float image
        /// </summary>
        /// <param name="source">image source</param>
        /// <param name="dest">image destination</param>
        public void Normalize(IImage2DFloat source, IImage2DFloat dest)
        {
            if (source == null) throw new ArgumentNullException("source");
            if (dest == null) throw new ArgumentNullException("dest");
            if ((source.Width > dest.Width) || (source.Height > dest.Height)) throw new ArgumentException("Destination image (" + dest.Width + "x" + dest.Height + ") must have at least the same size as the source image (" + source.Width + "x" + source.Height + ")");

            int length = source.HostBuffer.Length;
            for (int i = 0; i < length; i++)
                dest.HostBuffer[i] = source.HostBuffer[i] / 255f;
            dest.Normalized = true;
        }

        /// <summary>
        /// RGB to HSL image
        /// </summary>
        /// <param name="source">image source</param>
        /// <param name="dest">image destination</param>
        public void RgbToHsl(IImage2DFloatRgbA source, IImage2DFloatRgbA dest)
        {
            if (source == null) throw new ArgumentNullException("source");
            if (dest == null) throw new ArgumentNullException("dest");
            if ((source.Width > dest.Width) || (source.Height > dest.Height)) throw new ArgumentException("Destination image (" + dest.Width + "x" + dest.Height + ") must have at least the same size as the source image (" + source.Width + "x" + source.Height + ")");

            if (source.Normalized)
            {
                // use normalized kernel
                // TODO: RGB to HSL image (normalized)
            }
            else
            {
                // use normal kernel
                // TODO: RGB to HSL image
            }
            dest.Normalized = source.Normalized;
        }

        /// <summary>
        /// Sets channel of a RGBA image
        /// </summary>
        /// <param name="source">source image</param>
        /// <param name="mask">mask image</param>
        /// <param name="dest">destination image</param>
        /// <param name="offset">offset (0..3)</param>
        public void SetChannel(IImage2DByteRgbA source, IImage2DByteA mask, IImage2DByteRgbA dest, byte offset)
        {
            if (source == null) throw new ArgumentNullException("source");
            if (mask == null) throw new ArgumentNullException("mask");
            if (dest == null) throw new ArgumentNullException("dest");
            if (offset > 3) throw new ArgumentOutOfRangeException("offset", String.Format("offset must be between 0..3 but was {0}", offset));

            if ((source.Width > mask.Width) || (source.Height > mask.Height)) throw new ArgumentException("Image mask (" + dest.Width + "x" + dest.Height + ") must have at least the same size as the source image (" + source.Width + "x" + source.Height + ")");
            if ((source.Width > dest.Width) || (source.Height > dest.Height)) throw new ArgumentException("Destination image (" + dest.Width + "x" + dest.Height + ") must have at least the same size as the source image (" + source.Width + "x" + source.Height + ")");

            int length = source.Width * source.Height;
            int pos = 0;
            for (int i = 0; i < length; i++)
                for (int j = 0; j < 4; j++)
                {
                    dest.HostBuffer[pos] = (j == offset) ? mask.HostBuffer[i] : source.HostBuffer[pos];
                    pos++;
                }
        }

        /// <summary>
        /// Sets channel of a RGBA image
        /// </summary>
        /// <param name="source">source image</param>
        /// <param name="dest">destination image</param>
        /// <param name="offset">offset (0..3)</param>
        /// <param name="value">constant mask value</param>
        public void SetChannel(IImage2DByteRgbA source, IImage2DByteRgbA dest, byte offset, byte value)
        {
            if (source == null) throw new ArgumentNullException("source");
            if (dest == null) throw new ArgumentNullException("dest");
            if (offset > 3) throw new ArgumentOutOfRangeException("offset", String.Format("offset must be between 0..3 but was {0}", offset));

            if ((source.Width > dest.Width) || (source.Height > dest.Height)) throw new ArgumentException("Destination image (" + dest.Width + "x" + dest.Height + ") must have at least the same size as the source image (" + source.Width + "x" + source.Height + ")");

            int length = source.Width * source.Height;
            int pos = 0;
            for (int i = 0; i < length; i++)
                for (int j = 0; j < 4; j++)
                {
                    dest.HostBuffer[pos] = (j == offset) ? value : source.HostBuffer[pos];
                    pos++;
                }
        }

        /// <summary>
        /// Sets channel of a RGBA image
        /// </summary>
        /// <param name="source">source image</param>
        /// <param name="mask">mask image</param>
        /// <param name="dest">destination image</param>
        /// <param name="offset">offset (0..3)</param>
        public void SetChannel(IImage2DFloatRgbA source, IImage2DFloatA mask, IImage2DFloatRgbA dest, byte offset)
        {
            if (source == null) throw new ArgumentNullException("source");
            if (mask == null) throw new ArgumentNullException("mask");
            if (dest == null) throw new ArgumentNullException("dest");
            if (offset > 3) throw new ArgumentOutOfRangeException("offset", String.Format("offset must be between 0..3 but was {0}", offset));

            if ((source.Width > mask.Width) || (source.Height > mask.Height)) throw new ArgumentException("Image mask (" + dest.Width + "x" + dest.Height + ") must have at least the same size as the source image (" + source.Width + "x" + source.Height + ")");
            if ((source.Width > dest.Width) || (source.Height > dest.Height)) throw new ArgumentException("Destination image (" + dest.Width + "x" + dest.Height + ") must have at least the same size as the source image (" + source.Width + "x" + source.Height + ")");

            int length = source.Width * source.Height;
            int pos = 0;
            for (int i = 0; i < length; i++)
                for (int j = 0; j < 4; j++)
                {
                    dest.HostBuffer[pos] = (j == offset) ? mask.HostBuffer[i] : source.HostBuffer[pos];
                    pos++;
                }
        }

        /// <summary>
        /// Sets channel of a RGBA image
        /// </summary>
        /// <param name="source">source image</param>
        /// <param name="dest">destination image</param>
        /// <param name="offset">offset (0..3)</param>
        /// <param name="value">constant mask value</param>
        public void SetChannel(IImage2DFloatRgbA source, IImage2DFloatRgbA dest, byte offset, float value)
        {
            if (source == null) throw new ArgumentNullException("source");
            if (dest == null) throw new ArgumentNullException("dest");
            if (offset > 3) throw new ArgumentOutOfRangeException("offset", String.Format("offset must be between 0..3 but was {0}", offset));

            if ((source.Width > dest.Width) || (source.Height > dest.Height)) throw new ArgumentException("Destination image (" + dest.Width + "x" + dest.Height + ") must have at least the same size as the source image (" + source.Width + "x" + source.Height + ")");

            int length = source.Width * source.Height;
            int pos = 0;
            for (int i = 0; i < length; i++)
                for (int j = 0; j < 4; j++)
                {
                    dest.HostBuffer[pos] = (j == offset) ? value : source.HostBuffer[pos];
                    pos++;
                }
        }

        /// <summary>
        /// Sets a constant value to all cells in an image
        /// </summary>
        /// <param name="image">image</param>
        /// <param name="value">value to set</param>
        public void SetValue(IImage2DByte image, byte value)
        {
            if (image == null) throw new ArgumentNullException("image");

            image.HostBuffer.Clear(value);
        }

        /// <summary>
        /// Sets a constant value to all cells in an image
        /// </summary>
        /// <param name="image">image</param>
        /// <param name="value">value to set</param>
        public void SetValue(IImage2DByteRgbA image, uint value)
        {
            if (image == null) throw new ArgumentNullException("image");

            byte r = (byte)((value >> 24) % 256);
            byte g = (byte)((value >> 16) % 256);
            byte b = (byte)((value >> 8) % 256);
            byte a = (byte)((value) % 256);

            int length = image.Width * image.Height;
            int pos = 0;
            for (int i = 0; i < length; i++)
            {
                image.HostBuffer[pos++] = r;
                image.HostBuffer[pos++] = g;
                image.HostBuffer[pos++] = b;
                image.HostBuffer[pos++] = a;
            }
        }

        /// <summary>
        /// Sets a constant value to all cells in an image
        /// </summary>
        /// <param name="image">image</param>
        /// <param name="value">value to set</param>
        public void SetValue(IImage2DFloat image, float value)
        {
            if (image == null) throw new ArgumentNullException("image");

            image.HostBuffer.Clear(value);
        }

        /// <summary>
        /// Sets a constant value to all cells in an image
        /// </summary>
        /// <param name="image">image</param>
        /// <param name="value">value to set</param>
        public void SetValue(IImage2DUInt image, uint value)
        {
            if (image == null) throw new ArgumentNullException("image");

            image.HostBuffer.Clear(value);
        }

        /// <summary>
        /// Sobel filter image
        /// </summary>
        /// <param name="source">image source</param>
        /// <param name="dest">image destination</param>
        public void Sobel(IImage2DFloatA source, IImage2DFloatA dest)
        {
            if (source == null) throw new ArgumentNullException("source");
            if (dest == null) throw new ArgumentNullException("dest");
            if ((source.Width > dest.Width) || (source.Height > dest.Height)) throw new ArgumentException("Destination image (" + dest.Width + "x" + dest.Height + ") must have at least the same size as the source image (" + source.Width + "x" + source.Height + ")");

            // TODO: Sobel filter image
            dest.Normalized = source.Normalized;
        }
    }
}
