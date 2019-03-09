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
using Cloo;

namespace OpenClooVision.Imaging
{
    /// <summary>
    /// Converter class for cloo image format
    /// </summary>
    public static class ClooImageFormatConverter
    {
        /// <summary>
        /// Converts a compute image format to cloo image format
        /// </summary>
        /// <param name="imageFormat">compute image format</param>
        /// <returns>cloo image format</returns>
        /// <exception cref="NotSupportedException">Specified image format is not supported by OpenClooVision</exception>
        public static ClooImageFormat ToClooImageFormat(ComputeImageFormat imageFormat)
        {
            switch (imageFormat.ChannelType)
            {
                case ComputeImageChannelType.Float:
                    {
                        if (imageFormat.ChannelOrder == ComputeImageChannelOrder.A) return ClooImageFormat.FloatA;
                        if (imageFormat.ChannelOrder == ComputeImageChannelOrder.Rgba) return ClooImageFormat.FloatRgbA;
                        break;
                    }
                case ComputeImageChannelType.UnsignedInt8:
                    {
                        if (imageFormat.ChannelOrder == ComputeImageChannelOrder.A) return ClooImageFormat.ByteA;
                        if (imageFormat.ChannelOrder == ComputeImageChannelOrder.Rgba) return ClooImageFormat.ByteRgbA;
                        break;
                    }
                case ComputeImageChannelType.UnsignedInt32:
                    {
                        if (imageFormat.ChannelOrder == ComputeImageChannelOrder.A) return ClooImageFormat.UIntA;
                        break;
                    }
            }
            throw new NotSupportedException("Specified image format is not supported by OpenClooVision");
        }

        /// <summary>
        /// Converts a cloo image format to compute image format
        /// </summary>
        /// <param name="clooFormat">cloo image format</param>
        /// <returns>compute image format</returns>
        /// <exception cref="NotSupportedException">Specified image format is not supported</exception>
        public static ComputeImageFormat ToComputeImageFormat(ClooImageFormat clooFormat)
        {
            switch (clooFormat)
            {
                case ClooImageFormat.ByteA:
                    return new ComputeImageFormat(ComputeImageChannelOrder.A, ComputeImageChannelType.UnsignedInt8);
                case ClooImageFormat.ByteRgbA:
                    return new ComputeImageFormat(ComputeImageChannelOrder.Rgba, ComputeImageChannelType.UnsignedInt8);
                case ClooImageFormat.FloatA:
                    return new ComputeImageFormat(ComputeImageChannelOrder.A, ComputeImageChannelType.Float);
                case ClooImageFormat.FloatRgbA:
                    return new ComputeImageFormat(ComputeImageChannelOrder.Rgba, ComputeImageChannelType.Float);
                case ClooImageFormat.UIntA:
                    return new ComputeImageFormat(ComputeImageChannelOrder.A, ComputeImageChannelType.UnsignedInt32);
                default:
                    throw new NotSupportedException("Specified image format is not supported by OpenClooVision");
            }
        }

        /// <summary>
        /// Gets the ClooImageFormat of a ComputeImage for a specific type
        /// </summary>
        /// <param name="image">image to read format from</param>
        /// <param name="type">format type</param>
        /// <returns></returns>
        public static ClooImageFormat FromComputeImage(ComputeImage image, Type type)
        {
            ClooImageFormat format = default(ClooImageFormat);

            // create host buffer
            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Byte:
                    if (image.ElementSize == 4) format = ClooImageFormat.ByteRgbA; else format = ClooImageFormat.ByteA;
                    break;
                case TypeCode.Single:
                    if (image.ElementSize == 4) format = ClooImageFormat.FloatRgbA; else format = ClooImageFormat.FloatA;
                    break;
                case TypeCode.UInt32:
                    format = ClooImageFormat.UIntA;
                    break;
                default:
                    throw new NotSupportedException("Texture format is not supported by OpenClooVision");
            }

            return format;
        }
    }
}
