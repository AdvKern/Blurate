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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Cloo;
using OpenClooVision;
using OpenClooVision.Imaging;
using OpenClooVision.Kernels.Core;

namespace OpenClooVision.Kernels.Imaging
{
    /// <summary>
    /// Cloo program for imaging kernels
    /// </summary>
    [CLSCompliant(false)]
    public class ClooProgramImaging : ClooProgramCore
    {
        /// <summary>
        /// Box blur
        /// </summary>
        protected ClooKernel KernelImageByteBoxBlur { get; set; }

        /// <summary>
        /// Extracts a channel
        /// </summary>
        protected ClooKernel KernelImageByteExtractChannel { get; set; }

        /// <summary>
        /// Flip image X coordinate
        /// </summary>
        protected ClooKernel KernelImageByteFlipX { get; set; }

        /// <summary>
        /// Flip image Y coordinate
        /// </summary>
        protected ClooKernel KernelImageByteFlipY { get; set; }

        /// <summary>
        /// GrayScale kernel
        /// </summary>
        protected ClooKernel KernelImageByteGrayScale { get; set; }

        /// <summary>
        /// GrayScale kernel
        /// </summary>
        protected ClooKernel KernelImageByteGrayScaleFloat { get; set; }

        /// <summary>
        /// Create histogram 256
        /// </summary>
        protected ClooKernel KernelImageByteHistogram256 { get; set; }

        /// <summary>
        /// HSL to RGB conversion kernel
        /// </summary>
        protected ClooKernel KernelImageByteHslToRgb { get; set; }

        /// <summary>
        /// RGB to HSL conversion kernel
        /// </summary>
        protected ClooKernel KernelImageByteRgbToHsl { get; set; }

        /// <summary>
        /// Sets the alpha channel of a RGBA image
        /// </summary>
        protected ClooKernel KernelImageByteSetChannel { get; set; }

        /// <summary>
        /// Sets the alpha channel of a RGBA image
        /// </summary>
        protected ClooKernel KernelImageByteSetChannelConstant { get; set; }

        /// <summary>
        /// Sets a constant value to all cells in an image
        /// </summary>
        protected ClooKernel KernelImageByteSetValueA { get; set; }

        /// <summary>
        /// Sets a constant value to all cells in an image
        /// </summary>
        protected ClooKernel KernelImageByteSetValueRgbA { get; set; }

        /// <summary>
        /// Sobel filter kernel
        /// </summary>
        protected ClooKernel KernelImageByteSobel { get; set; }

        /// <summary>
        /// Converts a gray byte image to RGB byte image
        /// </summary>
        protected ClooKernel KernelImageByteAToByteRgbA { get; set; }

        /// <summary>
        /// Create color histogram with N bins
        /// </summary>
        protected ClooKernel KernelImageByteRgbHistogramN { get; set; }

        /// <summary>
        /// Swaps two channels in an image
        /// </summary>
        protected ClooKernel KernelImageByteSwapChannel { get; set; }

        /// <summary>
        /// Byte to Float conversion kernel
        /// </summary>
        protected ClooKernel KernelImageByteToFloat { get; set; }

        /// <summary>
        /// Makes all values absolute
        /// </summary>
        protected ClooKernel KernelImageFloatAbs { get; set; }

        /// <summary>
        /// Adds a constant value to all float values in an image
        /// </summary>
        protected ClooKernel KernelImageFloatAddValue { get; set; }

        /// <summary>
        /// Box blur kernel
        /// </summary>
        protected ClooKernel KernelImageFloatBoxBlur { get; set; }

        /// <summary>
        /// Clamps minimum and maximum value
        /// </summary>
        protected ClooKernel KernelImageFloatClamp { get; set; }

        /// <summary>
        /// Calculates the difference between two images (e.g. for background subtraction)
        /// </summary>
        protected ClooKernel KernelImageFloatDiff { get; set; }

        /// <summary>
        /// Extracts a channel
        /// </summary>
        protected ClooKernel KernelImageFloatExtractChannel { get; set; }

        /// <summary>
        /// Flip image X coordinate
        /// </summary>
        protected ClooKernel KernelImageFloatFlipX { get; set; }

        /// <summary>
        /// Flip image Y coordinate
        /// </summary>
        protected ClooKernel KernelImageFloatFlipY { get; set; }

        /// <summary>
        /// GrayScale kernel
        /// </summary>
        protected ClooKernel KernelImageFloatGrayScale { get; set; }

        /// <summary>
        /// HSL to RGB conversion kernel
        /// </summary>
        protected ClooKernel KernelImageFloatHslToRgb { get; set; }

        /// <summary>
        /// HSL to RGB conversion kernel (using normalized float values)
        /// </summary>
        protected ClooKernel KernelImageFloatHslToRgbNorm { get; set; }

        /// <summary>
        /// Convert float image to integral image (step 1)
        /// </summary>
        protected ClooKernel KernelImageFloatIntegralStep1 { get; set; }

        /// <summary>
        /// Convert float image to integral image (step 2)
        /// </summary>
        protected ClooKernel KernelImageFloatIntegral { get; set; }

        /// <summary>
        /// Convert float image to squared integral image (step 1)
        /// </summary>
        protected ClooKernel KernelImageFloatIntegralSquareStep1 { get; set; }

        /// <summary>
        /// Convert float image to squared integral image (step 2)
        /// </summary>
        protected ClooKernel KernelImageFloatIntegralSquare { get; set; }

        /// <summary>
        /// Multiplies a constant value to all float values in an image
        /// </summary>
        protected ClooKernel KernelImageFloatMultiplyValue { get; set; }

        /// <summary>
        /// RGB to HSL conversion kernel
        /// </summary>
        protected ClooKernel KernelImageFloatRgbToHsl { get; set; }

        /// <summary>
        /// RGB to HSL conversion kernel (using normalized float values)
        /// </summary>
        protected ClooKernel KernelImageFloatRgbToHslNorm { get; set; }

        /// <summary>
        /// Sets the alpha channel of a RGBA image
        /// </summary>
        protected ClooKernel KernelImageFloatSetChannel { get; set; }

        /// <summary>
        /// Sets the alpha channel of a RGBA image
        /// </summary>
        protected ClooKernel KernelImageFloatSetChannelConstant { get; set; }

        /// <summary>
        /// Sets a constant value to all float values in an image
        /// </summary>
        protected ClooKernel KernelImageFloatSetValue { get; set; }

        /// <summary>
        /// Sobel filter kernel
        /// </summary>
        protected ClooKernel KernelImageFloatSobel { get; set; }

        /// <summary>
        /// Swaps two channels in an image
        /// </summary>
        protected ClooKernel KernelImageFloatSwapChannel { get; set; }

        /// <summary>
        /// Float to Byte conversion kernel
        /// </summary>
        protected ClooKernel KernelImageFloatToByte { get; set; }

        /// <summary>
        /// Float to Byte conversion kernel (using normalized float values)
        /// </summary>
        protected ClooKernel KernelImageFloatToByteNorm { get; set; }

        /// <summary>
        /// Convert uint image to squared integral image (step 1)
        /// </summary>
        protected ClooKernel KernelImageUIntIntegralStep1 { get; set; }

        /// <summary>
        /// Convert uint image to integral image (step 2)
        /// </summary>
        protected ClooKernel KernelImageUIntIntegral { get; set; }

        /// <summary>
        /// Convert uint image to squared integral image (step 1)
        /// </summary>
        protected ClooKernel KernelImageUIntIntegralSquareStep1 { get; set; }

        /// <summary>
        /// Convert uint image to squared integral image (step 2)
        /// </summary>
        protected ClooKernel KernelImageUIntIntegralSquare { get; set; }

        /// <summary>
        /// Sets a constant value to all cells in an image
        /// </summary>
        protected ClooKernel KernelImageUIntSetValueA { get; set; }

        /// <summary>
        /// Special user-defined kernels
        /// </summary>
        protected List<ClooKernel> KernelSpecialKernel { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="context">compute context</param>
        /// <param name="lines">lines of source code</param>
        protected ClooProgramImaging(ClooContext context, string[] lines) :
            base(context, lines)
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="context">compute context</param>
        /// <param name="lines">lines of source code</param>
        protected ClooProgramImaging(ClooContext context, IList<byte[]> binaries, IEnumerable<ClooDevice> devices) :
            base(context, binaries, devices)
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="context">compute context</param>
        /// <param name="useBinaryCache">use cache for binaries</param>
        public static new ClooProgramImaging Create(ClooContext context, bool useBinaryCache = false)
        {
            var dict = GetEmbeddedSourceLines(typeof(ClooProgramImaging));
            string header = null;
            if (dict.TryGetValue("Header", out header)) dict.Remove("Header"); else header = "";
            List<string> lines = dict.Values.ToList();
            string source = header + "\r\n" + String.Join("\r\n\r\n", lines.ToArray());

            ClooProgramImaging res = null;
            if (useBinaryCache)
            {
                // check if we already have binaries
                int hashCode = lines.GetArrayHashCode();
                foreach (ClooDevice device in context.Devices)
                    hashCode ^= device.GetHashCode();

                IList<byte[]> binaries;
                if (ClooProgram.BinariesCache.TryGetValue(hashCode, out binaries))
                {
                    // create from cached binaries
                    res = new ClooProgramImaging(context, binaries, context.Devices);
                }
            }

#if DEBUG
            try
            {
#endif
                if (res == null)
                {
                    // ok not in cache, so rebuilt from source
                    res = new ClooProgramImaging(context, new string[] { source });
                    res.Build(context.Devices, null, useBinaryCache);
                }
#if DEBUG
            }
            catch (Exception exception)
            {
                StringBuilder sb = new StringBuilder();
                foreach (ComputeDevice device in context.Devices)
                {
                    string info = "DEVICE: " + device.Name;
                    sb.AppendLine(info);

                    StringReader reader = new StringReader(res.GetBuildLog(device));
                    string line = reader.ReadLine();
                    while (line != null)
                    {
                        sb.AppendLine(line);
                        line = reader.ReadLine();
                    }

                    sb.AppendLine("");
                    sb.AppendLine(exception.Message);
                }
                throw new ApplicationException(sb.ToString());
            }
#endif
            res.InitializeKernels(typeof(ClooProgramImaging));
            return res;
        }

        /// <summary>
        /// Makes all values absolute
        /// </summary>
        /// <param name="queue">cloo command queue</param>
        /// <param name="source">image source</param>
        /// <param name="dest">image destination</param>
        public void Abs(ClooCommandQueue queue, ClooImage2DFloatA source, ClooImage2DFloatA dest)
        {
            if (queue == null) throw new ArgumentNullException("queue");
            if (source == null) throw new ArgumentNullException("source");
            if (dest == null) throw new ArgumentNullException("dest");
            if ((source.Width > dest.Width) || (source.Height > dest.Height)) throw new ArgumentException("Destination image (" + dest.Width + "x" + dest.Height + ") must have at least the same size as the source image (" + source.Width + "x" + source.Height + ")");

            KernelImageFloatAbs.SetArguments(source, dest);
            queue.Execute(KernelImageFloatAbs, null, new long[] { source.Width, source.Height }, null, null);
            dest.Modified = true;
            dest.Normalized = source.Normalized;
        }

        /// <summary>
        /// Makes all values absolute
        /// </summary>
        /// <param name="queue">cloo command queue</param>
        /// <param name="source">image source</param>
        public void Abs(ClooCommandQueue queue, ClooImage2DFloatA image)
        {
            Abs(queue, image, image);
        }

        /// <summary>
        /// Makes all values absolute
        /// </summary>
        /// <param name="queue">cloo command queue</param>
        /// <param name="source">image source</param>
        /// <param name="dest">image destination</param>
        public void Abs(ClooCommandQueue queue, ClooImage2DFloatRgbA source, ClooImage2DFloatRgbA dest)
        {
            if (queue == null) throw new ArgumentNullException("queue");
            if (source == null) throw new ArgumentNullException("source");
            if (dest == null) throw new ArgumentNullException("dest");
            if ((source.Width > dest.Width) || (source.Height > dest.Height)) throw new ArgumentException("Destination image (" + dest.Width + "x" + dest.Height + ") must have at least the same size as the source image (" + source.Width + "x" + source.Height + ")");

            KernelImageFloatAbs.SetArguments(source, dest);
            queue.Execute(KernelImageFloatAbs, null, new long[] { source.Width, source.Height }, null, null);
            dest.Modified = true;
            dest.Normalized = source.Normalized;
        }

        /// <summary>
        /// Makes all values absolute
        /// </summary>
        /// <param name="queue">cloo command queue</param>
        /// <param name="source">image</param>
        public void Abs(ClooCommandQueue queue, ClooImage2DFloatRgbA image)
        {
            Abs(queue, image, image);
        }

        /// <summary>
        /// Adds a constant value to all float values in an image
        /// </summary>
        /// <param name="queue">cloo command queue</param>
        /// <param name="source">image source</param>
        /// <param name="dest">image destination</param>
        /// <param name="value">value to add</param>
        public void AddValue(ClooCommandQueue queue, ClooImage2DFloatA source, ClooImage2DFloatA dest, float value)
        {
            if (queue == null) throw new ArgumentNullException("queue");
            if (source == null) throw new ArgumentNullException("source");
            if (dest == null) throw new ArgumentNullException("dest");
            if ((source.Width > dest.Width) || (source.Height > dest.Height)) throw new ArgumentException("Destination image (" + dest.Width + "x" + dest.Height + ") must have at least the same size as the source image (" + source.Width + "x" + source.Height + ")");

            KernelImageFloatAddValue.SetArguments(source, dest, value);
            queue.Execute(KernelImageFloatAddValue, null, new long[] { source.Width, source.Height }, null, null);
            dest.Modified = true;
            dest.Normalized = source.Normalized;
        }

        /// <summary>
        /// Adds a constant value to all float values in an image
        /// </summary>
        /// <param name="queue">cloo command queue</param>
        /// <param name="image">image</param>
        /// <param name="value">value to add</param>
        public void AddValue(ClooCommandQueue queue, ClooImage2DFloatA image, float value)
        {
            AddValue(queue, image, image, value);
        }

        /// <summary>
        /// Adds a constant value to all float values in an image
        /// </summary>
        /// <param name="queue">cloo command queue</param>
        /// <param name="source">image source</param>
        /// <param name="dest">image destination</param>
        /// <param name="value">value to add</param>
        public void AddValue(ClooCommandQueue queue, ClooImage2DFloatRgbA source, ClooImage2DFloatRgbA dest, float value)
        {
            if (queue == null) throw new ArgumentNullException("queue");
            if (source == null) throw new ArgumentNullException("source");
            if (dest == null) throw new ArgumentNullException("dest");
            if ((source.Width > dest.Width) || (source.Height > dest.Height)) throw new ArgumentException("Destination image (" + dest.Width + "x" + dest.Height + ") must have at least the same size as the source image (" + source.Width + "x" + source.Height + ")");

            KernelImageFloatAddValue.SetArguments(source, dest, value);
            queue.Execute(KernelImageFloatAddValue, null, new long[] { source.Width, source.Height }, null, null);
            dest.Modified = true;
            dest.Normalized = source.Normalized;
        }

        /// <summary>
        /// Adds a constant value to all float values in an image
        /// </summary>
        /// <param name="queue">cloo command queue</param>
        /// <param name="image">image</param>
        /// <param name="value">value to add</param>
        public void AddValue(ClooCommandQueue queue, ClooImage2DFloatRgbA image, float value)
        {
            AddValue(queue, image, image, value);
        }

        /// <summary>
        /// Convert gray byte image to RGB byte image
        /// </summary>
        /// <param name="queue">cloo command queue</param>
        /// <param name="source">image source</param>
        /// <param name="dest">image destination</param>
        public void ByteAToByteRgbA(ClooCommandQueue queue, ClooImage2DByteA source, ClooImage2DByteRgbA dest)
        {
            if (queue == null) throw new ArgumentNullException("queue");
            if (source == null) throw new ArgumentNullException("source");
            if (dest == null) throw new ArgumentNullException("dest");
            if ((source.Width > dest.Width) || (source.Height > dest.Height)) throw new ArgumentException("Destination image (" + dest.Width + "x" + dest.Height + ") must have at least the same size as the source image (" + source.Width + "x" + source.Height + ")");

            KernelImageByteAToByteRgbA.SetArguments(source, dest);
            queue.Execute(KernelImageByteAToByteRgbA, null, new long[] { source.Width, source.Height }, null, null);
            dest.Modified = true;
        }

        /// <summary>
        /// Convert byte image to float image
        /// </summary>
        /// <param name="queue">cloo command queue</param>
        /// <param name="source">image source</param>
        /// <param name="dest">image destination</param>
        public void ByteToFloat(ClooCommandQueue queue, ClooImage2DByteRgbA source, ClooImage2DFloatRgbA dest)
        {
            if (queue == null) throw new ArgumentNullException("queue");
            if (source == null) throw new ArgumentNullException("source");
            if (dest == null) throw new ArgumentNullException("dest");
            if ((source.Width > dest.Width) || (source.Height > dest.Height)) throw new ArgumentException("Destination image (" + dest.Width + "x" + dest.Height + ") must have at least the same size as the source image (" + source.Width + "x" + source.Height + ")");

            KernelImageByteToFloat.SetArguments(source, dest);
            queue.Execute(KernelImageByteToFloat, null, new long[] { source.Width, source.Height }, null, null);
            dest.Modified = true;
            dest.Normalized = false;
        }

        /// <summary>
        /// Convert byte image to float image
        /// </summary>
        /// <param name="queue">cloo command queue</param>
        /// <param name="source">image source</param>
        /// <param name="dest">image destination</param>
        public void ByteToFloat(ClooCommandQueue queue, ClooImage2DByteA source, ClooImage2DFloatA dest)
        {
            if (queue == null) throw new ArgumentNullException("queue");
            if (source == null) throw new ArgumentNullException("source");
            if (dest == null) throw new ArgumentNullException("dest");
            if ((source.Width > dest.Width) || (source.Height > dest.Height)) throw new ArgumentException("Destination image (" + dest.Width + "x" + dest.Height + ") must have at least the same size as the source image (" + source.Width + "x" + source.Height + ")");

            KernelImageByteToFloat.SetArguments(source, dest);
            queue.Execute(KernelImageByteToFloat, null, new long[] { source.Width, source.Height }, null, null);
            dest.Modified = true;
            dest.Normalized = false;
        }

        /// <summary>
        /// Box blur image
        /// </summary>
        /// <param name="queue">cloo command queue</param>
        /// <param name="source">image source</param>
        /// <param name="dest">image destination</param>
        /// <param name="sampler">sampler to be used for image reading</param>
        /// <param name="offset">offset</param>
        public void BoxBlur(ClooCommandQueue queue, ClooImage2DByteA source, ClooImage2DByteA dest, ClooSampler sampler, int offset)
        {
            if (queue == null) throw new ArgumentNullException("queue");
            if (source == null) throw new ArgumentNullException("source");
            if (dest == null) throw new ArgumentNullException("dest");
            if (sampler == null) throw new ArgumentNullException("sampler");
            if ((source.Width > dest.Width) || (source.Height > dest.Height)) throw new ArgumentException("Destination image (" + dest.Width + "x" + dest.Height + ") must have at least the same size as the source image (" + source.Width + "x" + source.Height + ")");

            KernelImageByteBoxBlur.SetArguments(source, dest, sampler, offset);
            queue.Execute(KernelImageByteBoxBlur, null, new long[] { source.Width, source.Height }, null, null);
            dest.Modified = true;
        }

        /// <summary>
        /// Box blur image
        /// </summary>
        /// <param name="queue">cloo command queue</param>
        /// <param name="source">image source</param>
        /// <param name="dest">image destination</param>
        /// <param name="sampler">sampler to be used for image reading</param>
        /// <param name="offset">offset</param>
        public void BoxBlur(ClooCommandQueue queue, ClooImage2DByteRgbA source, ClooImage2DByteRgbA dest, ClooSampler sampler, int offset)
        {
            if (queue == null) throw new ArgumentNullException("queue");
            if (source == null) throw new ArgumentNullException("source");
            if (dest == null) throw new ArgumentNullException("dest");
            if (sampler == null) throw new ArgumentNullException("sampler");
            if ((source.Width > dest.Width) || (source.Height > dest.Height)) throw new ArgumentException("Destination image (" + dest.Width + "x" + dest.Height + ") must have at least the same size as the source image (" + source.Width + "x" + source.Height + ")");

            KernelImageByteBoxBlur.SetArguments(source, dest, sampler, offset);
            queue.Execute(KernelImageByteBoxBlur, null, new long[] { source.Width, source.Height }, null, null);
            dest.Modified = true;
        }

        /// <summary>
        /// Box blur image
        /// </summary>
        /// <param name="queue">cloo command queue</param>
        /// <param name="source">image source</param>
        /// <param name="dest">image destination</param>
        /// <param name="sampler">sampler to be used for image reading</param>
        /// <param name="offset">offset</param>
        public void BoxBlur(ClooCommandQueue queue, ClooImage2DFloatA source, ClooImage2DFloatA dest, ClooSampler sampler, int offset)
        {
            if (queue == null) throw new ArgumentNullException("queue");
            if (source == null) throw new ArgumentNullException("source");
            if (dest == null) throw new ArgumentNullException("dest");
            if (sampler == null) throw new ArgumentNullException("sampler");
            if ((source.Width > dest.Width) || (source.Height > dest.Height)) throw new ArgumentException("Destination image (" + dest.Width + "x" + dest.Height + ") must have at least the same size as the source image (" + source.Width + "x" + source.Height + ")");

            KernelImageFloatBoxBlur.SetArguments(source, dest, sampler, offset);
            queue.Execute(KernelImageFloatBoxBlur, null, new long[] { source.Width, source.Height }, null, null);
            dest.Modified = true;
            dest.Normalized = source.Normalized;
        }

        /// <summary>
        /// Box blur image
        /// </summary>
        /// <param name="queue">cloo command queue</param>
        /// <param name="source">image source</param>
        /// <param name="dest">image destination</param>
        /// <param name="sampler">sampler to be used for image reading</param>
        /// <param name="offset">offset</param>
        public void BoxBlur(ClooCommandQueue queue, ClooImage2DFloatRgbA source, ClooImage2DFloatRgbA dest, ClooSampler sampler, int offset)
        {
            if (queue == null) throw new ArgumentNullException("queue");
            if (source == null) throw new ArgumentNullException("source");
            if (dest == null) throw new ArgumentNullException("dest");
            if ((source.Width > dest.Width) || (source.Height > dest.Height)) throw new ArgumentException("Destination image (" + dest.Width + "x" + dest.Height + ") must have at least the same size as the source image (" + source.Width + "x" + source.Height + ")");

            KernelImageFloatBoxBlur.SetArguments(source, dest, sampler, offset);
            queue.Execute(KernelImageFloatBoxBlur, null, new long[] { source.Width, source.Height }, null, null);
            dest.Modified = true;
            dest.Normalized = source.Normalized;
        }

        /// <summary>
        /// Clamps minimum and maximum value
        /// </summary>
        /// <param name="queue">cloo command queue</param>
        /// <param name="source">image source</param>
        /// <param name="dest">image destination</param>
        /// <param name="minValue">minimum value</param>
        /// <param name="maxValue">maximum value</param>
        public void Clamp(ClooCommandQueue queue, ClooImage2DFloatA source, ClooImage2DFloatA dest, float minValue, float maxValue)
        {
            if (queue == null) throw new ArgumentNullException("queue");
            if (source == null) throw new ArgumentNullException("source");
            if (dest == null) throw new ArgumentNullException("dest");
            if ((source.Width > dest.Width) || (source.Height > dest.Height)) throw new ArgumentException("Destination image (" + dest.Width + "x" + dest.Height + ") must have at least the same size as the source image (" + source.Width + "x" + source.Height + ")");

            KernelImageFloatClamp.SetArguments(source, dest, minValue, maxValue);
            queue.Execute(KernelImageFloatClamp, null, new long[] { source.Width, source.Height }, null, null);
            dest.Modified = true;
            dest.Normalized = source.Normalized;
        }

        /// <summary>
        /// Clamps minimum and maximum value
        /// </summary>
        /// <param name="queue">cloo command queue</param>
        /// <param name="image">image</param>
        /// <param name="minValue">minimum value</param>
        /// <param name="maxValue">maximum value</param>
        public void Clamp(ClooCommandQueue queue, ClooImage2DFloatA image, float minValue, float maxValue)
        {
            Clamp(queue, image, image, minValue, maxValue);
        }

        /// <summary>
        /// Clamps minimum and maximum value
        /// </summary>
        /// <param name="queue">cloo command queue</param>
        /// <param name="source">image source</param>
        /// <param name="dest">image destination</param>
        /// <param name="minValue">minimum value</param>
        /// <param name="maxValue">maximum value</param>
        public void Clamp(ClooCommandQueue queue, ClooImage2DFloatRgbA source, ClooImage2DFloatRgbA dest, float minValue, float maxValue)
        {
            if (queue == null) throw new ArgumentNullException("queue");
            if (source == null) throw new ArgumentNullException("source");
            if (dest == null) throw new ArgumentNullException("dest");
            if ((source.Width > dest.Width) || (source.Height > dest.Height)) throw new ArgumentException("Destination image (" + dest.Width + "x" + dest.Height + ") must have at least the same size as the source image (" + source.Width + "x" + source.Height + ")");

            KernelImageFloatClamp.SetArguments(source, dest, minValue, maxValue);
            queue.Execute(KernelImageFloatClamp, null, new long[] { source.Width, source.Height }, null, null);
            dest.Modified = true;
            dest.Normalized = source.Normalized;
        }

        /// <summary>
        /// Clamps minimum and maximum value
        /// </summary>
        /// <param name="queue">cloo command queue</param>
        /// <param name="image">image</param>
        /// <param name="minValue">minimum value</param>
        /// <param name="maxValue">maximum value</param>
        public void Clamp(ClooCommandQueue queue, ClooImage2DFloatRgbA image, float minValue, float maxValue)
        {
            Clamp(queue, image, image, minValue, maxValue);
        }

        /// <summary>
        /// Denormalize float image
        /// </summary>
        /// <param name="queue">cloo command queue</param>
        /// <param name="source">image source</param>
        /// <param name="dest">image destination</param>
        public void Denormalize(ClooCommandQueue queue, ClooImage2DFloatA source, ClooImage2DFloatA dest)
        {
            if (queue == null) throw new ArgumentNullException("queue");
            if (source == null) throw new ArgumentNullException("source");
            if (dest == null) throw new ArgumentNullException("dest");
            if ((source.Width > dest.Width) || (source.Height > dest.Height)) throw new ArgumentException("Destination image (" + dest.Width + "x" + dest.Height + ") must have at least the same size as the source image (" + source.Width + "x" + source.Height + ")");

            KernelImageFloatMultiplyValue.SetArguments(source, dest, 255f);
            queue.Execute(KernelImageFloatMultiplyValue, null, new long[] { source.Width, source.Height }, null, null);
            dest.Modified = true;
            dest.Normalized = false;
        }

        /// <summary>
        /// Denormalize float image
        /// </summary>
        /// <param name="queue">cloo command queue</param>
        /// <param name="image">image</param>
        public void Denormalize(ClooCommandQueue queue, ClooImage2DFloatA image)
        {
            Denormalize(queue, image, image);
        }

        /// <summary>
        /// Denormalize float image
        /// </summary>
        /// <param name="queue">cloo command queue</param>
        /// <param name="source">image source</param>
        /// <param name="dest">image destination</param>
        public void Denormalize(ClooCommandQueue queue, ClooImage2DFloatRgbA source, ClooImage2DFloatRgbA dest)
        {
            if (queue == null) throw new ArgumentNullException("queue");
            if (source == null) throw new ArgumentNullException("source");
            if (dest == null) throw new ArgumentNullException("dest");
            if ((source.Width > dest.Width) || (source.Height > dest.Height)) throw new ArgumentException("Destination image (" + dest.Width + "x" + dest.Height + ") must have at least the same size as the source image (" + source.Width + "x" + source.Height + ")");

            KernelImageFloatMultiplyValue.SetArguments(source, dest, 255f);
            queue.Execute(KernelImageFloatMultiplyValue, null, new long[] { source.Width, source.Height }, null, null);
            dest.Modified = true;
            dest.Normalized = false;
        }

        /// <summary>
        /// Denormalize float image
        /// </summary>
        /// <param name="queue">cloo command queue</param>
        /// <param name="image">image</param>
        public void Denormalize(ClooCommandQueue queue, ClooImage2DFloatRgbA image)
        {
            Denormalize(queue, image, image);
        }

        /// <summary>
        /// Calculates the difference between two images (e.g. for background subtraction)
        /// </summary>
        /// <param name="queue">cloo command queue</param>
        /// <param name="source">image source</param>
        /// <param name="dest">image destination</param>
        public void Diff(ClooCommandQueue queue, ClooImage2DFloatA source1, ClooImage2DFloatA source2, ClooImage2DFloatA dest)
        {
            if (queue == null) throw new ArgumentNullException("queue");
            if (source1 == null) throw new ArgumentNullException("source1");
            if (source2 == null) throw new ArgumentNullException("source2");
            if (dest == null) throw new ArgumentNullException("dest");
            if ((source2.Width > source1.Width) || (source2.Height > source1.Height)) throw new ArgumentException("Second image (" + dest.Width + "x" + dest.Height + ") must have at least the same size as the first image (" + source1.Width + "x" + source1.Height + ")");
            if ((source1.Width > dest.Width) || (source1.Height > dest.Height)) throw new ArgumentException("Destination image (" + dest.Width + "x" + dest.Height + ") must have at least the same size as the source image (" + source1.Width + "x" + source1.Height + ")");

            KernelImageFloatDiff.SetArguments(source1, source2, dest);
            queue.Execute(KernelImageFloatDiff, null, new long[] { source2.Width, source2.Height }, null, null);
            dest.Modified = true;
            dest.Normalized = false;
        }

        /// <summary>
        /// Calculates the difference between two images (e.g. for background subtraction)
        /// </summary>
        /// <param name="queue">cloo command queue</param>
        /// <param name="source">image source</param>
        /// <param name="dest">image destination</param>
        public void Diff(ClooCommandQueue queue, ClooImage2DFloatRgbA source1, ClooImage2DFloatRgbA source2, ClooImage2DFloatA dest)
        {
            if (queue == null) throw new ArgumentNullException("queue");
            if (source1 == null) throw new ArgumentNullException("source1");
            if (source2 == null) throw new ArgumentNullException("source2");
            if (dest == null) throw new ArgumentNullException("dest");
            if ((source2.Width > source1.Width) || (source2.Height > source1.Height)) throw new ArgumentException("Second image (" + dest.Width + "x" + dest.Height + ") must have at least the same size as the first image (" + source1.Width + "x" + source1.Height + ")");
            if ((source1.Width > dest.Width) || (source1.Height > dest.Height)) throw new ArgumentException("Destination image (" + dest.Width + "x" + dest.Height + ") must have at least the same size as the source image (" + source1.Width + "x" + source1.Height + ")");

            KernelImageFloatDiff.SetArguments(source1, source2, dest);
            queue.Execute(KernelImageFloatDiff, null, new long[] { source2.Width, source2.Height }, null, null);
            dest.Modified = true;
            dest.Normalized = false;
        }

        /// <summary>
        /// Extracts a channel of an RGB image
        /// </summary>
        /// <param name="queue">cloo command queue</param>
        /// <param name="source">image source</param>
        /// <param name="dest">image destination</param>
        /// <param name="offset">offset (0..3)</param>
        public void ExtractChannel(ClooCommandQueue queue, ClooImage2DByteRgbA source, ClooImage2DByteA dest, byte offset)
        {
            if (queue == null) throw new ArgumentNullException("queue");
            if (source == null) throw new ArgumentNullException("source");
            if (dest == null) throw new ArgumentNullException("dest");
            if (offset > 3) throw new ArgumentOutOfRangeException("offset", String.Format("offset must be between 0..3 but was {0}", offset));
            if ((source.Width > dest.Width) || (source.Height > dest.Height)) throw new ArgumentException("Destination image (" + dest.Width + "x" + dest.Height + ") must have at least the same size as the source image (" + source.Width + "x" + source.Height + ")");

            KernelImageByteExtractChannel.SetArguments(source, dest, offset);
            queue.Execute(KernelImageByteExtractChannel, null, new long[] { source.Width, source.Height }, null, null);
            dest.Modified = true;
        }

        /// <summary>
        /// Extracts a channel of an RGB image
        /// </summary>
        /// <param name="queue">cloo command queue</param>
        /// <param name="source">image source</param>
        /// <param name="dest">image destination</param>
        /// <param name="offset">offset (0..3)</param>
        public void ExtractChannel(ClooCommandQueue queue, ClooImage2DFloatRgbA source, ClooImage2DFloatA dest, byte offset)
        {
            if (queue == null) throw new ArgumentNullException("queue");
            if (source == null) throw new ArgumentNullException("source");
            if (dest == null) throw new ArgumentNullException("dest");
            if (offset > 3) throw new ArgumentOutOfRangeException("offset", String.Format("offset must be between 0..3 but was {0}", offset));
            if ((source.Width > dest.Width) || (source.Height > dest.Height)) throw new ArgumentException("Destination image (" + dest.Width + "x" + dest.Height + ") must have at least the same size as the source image (" + source.Width + "x" + source.Height + ")");

            KernelImageFloatExtractChannel.SetArguments(source, dest, offset);
            queue.Execute(KernelImageFloatExtractChannel, null, new long[] { source.Width, source.Height }, null, null);
            dest.Modified = true;
            dest.Normalized = source.Normalized;
        }

        /// <summary>
        /// Flip image X coordinate
        /// </summary>
        /// <param name="queue">cloo command queue</param>
        /// <param name="source">image source</param>
        /// <param name="dest">image destination</param>
        public void FlipX(ClooCommandQueue queue, ClooImage2DByteA source, ClooImage2DByteA dest)
        {
            if (queue == null) throw new ArgumentNullException("queue");
            if (source == null) throw new ArgumentNullException("source");
            if (dest == null) throw new ArgumentNullException("dest");
            if (source == dest) throw new ArgumentException("Flipping kernel is not designed to run inline therefore source and destination must be different images");
            if ((source.Width > dest.Width) || (source.Height > dest.Height)) throw new ArgumentException("Destination image (" + dest.Width + "x" + dest.Height + ") must have at least the same size as the source image (" + source.Width + "x" + source.Height + ")");

            KernelImageByteFlipX.SetArguments(source, dest);
            queue.Execute(KernelImageByteFlipX, null, new long[] { source.Width, source.Height }, null, null);
            dest.Modified = true;
        }

        /// <summary>
        /// Flip image X coordinate
        /// </summary>
        /// <param name="queue">cloo command queue</param>
        /// <param name="source">image source</param>
        /// <param name="dest">image destination</param>
        public void FlipX(ClooCommandQueue queue, ClooImage2DByteRgbA source, ClooImage2DByteRgbA dest)
        {
            if (queue == null) throw new ArgumentNullException("queue");
            if (source == null) throw new ArgumentNullException("source");
            if (dest == null) throw new ArgumentNullException("dest");
            if (source == dest) throw new ArgumentException("Flipping kernel is not designed to run inline therefore source and destination must be different images");
            if ((source.Width > dest.Width) || (source.Height > dest.Height)) throw new ArgumentException("Destination image (" + dest.Width + "x" + dest.Height + ") must have at least the same size as the source image (" + source.Width + "x" + source.Height + ")");

            KernelImageByteFlipX.SetArguments(source, dest);
            queue.Execute(KernelImageByteFlipX, null, new long[] { source.Width, source.Height }, null, null);
            dest.Modified = true;
        }

        /// <summary>
        /// Flip image X coordinate
        /// </summary>
        /// <param name="queue">cloo command queue</param>
        /// <param name="source">image source</param>
        /// <param name="dest">image destination</param>
        public void FlipX(ClooCommandQueue queue, ClooImage2DFloatA source, ClooImage2DFloatA dest)
        {
            if (queue == null) throw new ArgumentNullException("queue");
            if (source == null) throw new ArgumentNullException("source");
            if (dest == null) throw new ArgumentNullException("dest");
            if (source == dest) throw new ArgumentException("Flipping kernel is not designed to run inline therefore source and destination must be different images");
            if ((source.Width > dest.Width) || (source.Height > dest.Height)) throw new ArgumentException("Destination image (" + dest.Width + "x" + dest.Height + ") must have at least the same size as the source image (" + source.Width + "x" + source.Height + ")");

            KernelImageFloatFlipX.SetArguments(source, dest);
            queue.Execute(KernelImageFloatFlipX, null, new long[] { source.Width, source.Height }, null, null);
            dest.Modified = true;
            dest.Normalized = source.Normalized;
        }

        /// <summary>
        /// Flip image X coordinate
        /// </summary>
        /// <param name="queue">cloo command queue</param>
        /// <param name="source">image source</param>
        /// <param name="dest">image destination</param>
        public void FlipX(ClooCommandQueue queue, ClooImage2DFloatRgbA source, ClooImage2DFloatRgbA dest)
        {
            if (queue == null) throw new ArgumentNullException("queue");
            if (source == null) throw new ArgumentNullException("source");
            if (dest == null) throw new ArgumentNullException("dest");
            if (source == dest) throw new ArgumentException("Flipping kernel is not designed to run inline therefore source and destination must be different images");
            if ((source.Width > dest.Width) || (source.Height > dest.Height)) throw new ArgumentException("Destination image (" + dest.Width + "x" + dest.Height + ") must have at least the same size as the source image (" + source.Width + "x" + source.Height + ")");

            KernelImageFloatFlipX.SetArguments(source, dest);
            queue.Execute(KernelImageFloatFlipX, null, new long[] { source.Width, source.Height }, null, null);
            dest.Modified = true;
            dest.Normalized = source.Normalized;
        }

        /// <summary>
        /// Flip image Y coordinate
        /// </summary>
        /// <param name="queue">cloo command queue</param>
        /// <param name="source">image source</param>
        /// <param name="dest">image destination</param>
        public void FlipY(ClooCommandQueue queue, ClooImage2DByteA source, ClooImage2DByteA dest)
        {
            if (queue == null) throw new ArgumentNullException("queue");
            if (source == null) throw new ArgumentNullException("source");
            if (dest == null) throw new ArgumentNullException("dest");
            if (source == dest) throw new ArgumentException("Flipping kernel is not designed to run inline therefore source and destination must be different images");
            if ((source.Width > dest.Width) || (source.Height > dest.Height)) throw new ArgumentException("Destination image (" + dest.Width + "x" + dest.Height + ") must have at least the same size as the source image (" + source.Width + "x" + source.Height + ")");

            KernelImageByteFlipY.SetArguments(source, dest);
            queue.Execute(KernelImageByteFlipY, null, new long[] { source.Width, source.Height }, null, null);
            dest.Modified = true;
        }

        /// <summary>
        /// Flip image Y coordinate
        /// </summary>
        /// <param name="queue">cloo command queue</param>
        /// <param name="source">image source</param>
        /// <param name="dest">image destination</param>
        public void FlipY(ClooCommandQueue queue, ClooImage2DByteRgbA source, ClooImage2DByteRgbA dest)
        {
            if (queue == null) throw new ArgumentNullException("queue");
            if (source == null) throw new ArgumentNullException("source");
            if (dest == null) throw new ArgumentNullException("dest");
            if (source == dest) throw new ArgumentException("Flipping kernel is not designed to run inline therefore source and destination must be different images");
            if ((source.Width > dest.Width) || (source.Height > dest.Height)) throw new ArgumentException("Destination image (" + dest.Width + "x" + dest.Height + ") must have at least the same size as the source image (" + source.Width + "x" + source.Height + ")");

            KernelImageByteFlipY.SetArguments(source, dest);
            queue.Execute(KernelImageByteFlipY, null, new long[] { source.Width, source.Height }, null, null);
            dest.Modified = true;
        }

        /// <summary>
        /// Flip image Y coordinate
        /// </summary>
        /// <param name="queue">cloo command queue</param>
        /// <param name="source">image source</param>
        /// <param name="dest">image destination</param>
        public void FlipY(ClooCommandQueue queue, ClooImage2DFloatA source, ClooImage2DFloatA dest)
        {
            if (queue == null) throw new ArgumentNullException("queue");
            if (source == null) throw new ArgumentNullException("source");
            if (dest == null) throw new ArgumentNullException("dest");
            if (source == dest) throw new ArgumentException("Flipping kernel is not designed to run inline therefore source and destination must be different images");
            if ((source.Width > dest.Width) || (source.Height > dest.Height)) throw new ArgumentException("Destination image (" + dest.Width + "x" + dest.Height + ") must have at least the same size as the source image (" + source.Width + "x" + source.Height + ")");

            KernelImageFloatFlipY.SetArguments(source, dest);
            queue.Execute(KernelImageFloatFlipY, null, new long[] { source.Width, source.Height }, null, null);
            dest.Modified = true;
            dest.Normalized = source.Normalized;
        }

        /// <summary>
        /// Flip image Y coordinate
        /// </summary>
        /// <param name="queue">cloo command queue</param>
        /// <param name="source">image source</param>
        /// <param name="dest">image destination</param>
        public void FlipY(ClooCommandQueue queue, ClooImage2DFloatRgbA source, ClooImage2DFloatRgbA dest)
        {
            if (queue == null) throw new ArgumentNullException("queue");
            if (source == null) throw new ArgumentNullException("source");
            if (dest == null) throw new ArgumentNullException("dest");
            if (source == dest) throw new ArgumentException("Flipping kernel is not designed to run inline therefore source and destination must be different images");
            if ((source.Width > dest.Width) || (source.Height > dest.Height)) throw new ArgumentException("Destination image (" + dest.Width + "x" + dest.Height + ") must have at least the same size as the source image (" + source.Width + "x" + source.Height + ")");

            KernelImageFloatFlipY.SetArguments(source, dest);
            queue.Execute(KernelImageFloatFlipY, null, new long[] { source.Width, source.Height }, null, null);
            dest.Modified = true;
            dest.Normalized = source.Normalized;
        }

        /// <summary>
        /// Convert float image to byte image
        /// </summary>
        /// <param name="queue">cloo command queue</param>
        /// <param name="source">image source</param>
        /// <param name="dest">image destination</param>
        public void FloatToByte(ClooCommandQueue queue, ClooImage2DFloatA source, ClooImage2DByteA dest)
        {
            if (queue == null) throw new ArgumentNullException("queue");
            if (source == null) throw new ArgumentNullException("source");
            if (dest == null) throw new ArgumentNullException("dest");
            if ((source.Width > dest.Width) || (source.Height > dest.Height)) throw new ArgumentException("Destination image (" + dest.Width + "x" + dest.Height + ") must have at least the same size as the source image (" + source.Width + "x" + source.Height + ")");

            if (source.Normalized)
            {
                // use normalized kernel
                KernelImageFloatToByteNorm.SetArguments(source, dest);
                queue.Execute(KernelImageFloatToByteNorm, null, new long[] { source.Width, source.Height }, null, null);
            }
            else
            {
                // use normal kernel
                KernelImageFloatToByte.SetArguments(source, dest);
                queue.Execute(KernelImageFloatToByte, null, new long[] { source.Width, source.Height }, null, null);
            }
            dest.Modified = true;
        }

        /// <summary>
        /// Convert float image to byte image
        /// </summary>
        /// <param name="queue">cloo command queue</param>
        /// <param name="source">image source</param>
        /// <param name="dest">image destination</param>
        public void FloatToByte(ClooCommandQueue queue, ClooImage2DFloatRgbA source, ClooImage2DByteRgbA dest)
        {
            if (queue == null) throw new ArgumentNullException("queue");
            if (source == null) throw new ArgumentNullException("source");
            if (dest == null) throw new ArgumentNullException("dest");
            if ((source.Width > dest.Width) || (source.Height > dest.Height)) throw new ArgumentException("Destination image (" + dest.Width + "x" + dest.Height + ") must have at least the same size as the source image (" + source.Width + "x" + source.Height + ")");

            if (source.Normalized)
            {
                // use normalized kernel
                KernelImageFloatToByteNorm.SetArguments(source, dest);
                queue.Execute(KernelImageFloatToByteNorm, null, new long[] { source.Width, source.Height }, null, null);
            }
            else
            {
                // use normal kernel
                KernelImageFloatToByte.SetArguments(source, dest);
                queue.Execute(KernelImageFloatToByte, null, new long[] { source.Width, source.Height }, null, null);
            }
            dest.Modified = true;
        }

        /// <summary>
        /// GrayScale image
        /// </summary>
        /// <param name="queue">cloo command queue</param>
        /// <param name="source">image source</param>
        /// <param name="dest">image destination</param>
        public void GrayScale(ClooCommandQueue queue, ClooImage2DByteRgbA source, ClooImage2DByteA dest)
        {
            if (queue == null) throw new ArgumentNullException("queue");
            if (source == null) throw new ArgumentNullException("source");
            if (dest == null) throw new ArgumentNullException("dest");
            if ((source.Width > dest.Width) || (source.Height > dest.Height)) throw new ArgumentException("Destination image (" + dest.Width + "x" + dest.Height + ") must have at least the same size as the source image (" + source.Width + "x" + source.Height + ")");

            KernelImageByteGrayScale.SetArguments(source, dest);
            queue.Execute(KernelImageByteGrayScale, null, new long[] { source.Width, source.Height }, null, null);
            dest.Modified = true;
        }

        /// <summary>
        /// GrayScale image
        /// </summary>
        /// <param name="queue">cloo command queue</param>
        /// <param name="source">image source</param>
        /// <param name="dest">image destination</param>
        public void GrayScale(ClooCommandQueue queue, ClooImage2DByteRgbA source, ClooImage2DFloatA dest)
        {
            if (queue == null) throw new ArgumentNullException("queue");
            if (source == null) throw new ArgumentNullException("source");
            if (dest == null) throw new ArgumentNullException("dest");
            if ((source.Width > dest.Width) || (source.Height > dest.Height)) throw new ArgumentException("Destination image (" + dest.Width + "x" + dest.Height + ") must have at least the same size as the source image (" + source.Width + "x" + source.Height + ")");

            KernelImageByteGrayScaleFloat.SetArguments(source, dest);
            queue.Execute(KernelImageByteGrayScaleFloat, null, new long[] { source.Width, source.Height }, null, null);
            dest.Modified = true;
            dest.Normalized = false;
        }

        /// <summary>
        /// GrayScale image
        /// </summary>
        /// <param name="queue">cloo command queue</param>
        /// <param name="source">image source</param>
        /// <param name="dest">image destination</param>
        public void GrayScale(ClooCommandQueue queue, ClooImage2DFloatRgbA source, ClooImage2DFloatA dest)
        {
            if (queue == null) throw new ArgumentNullException("queue");
            if (source == null) throw new ArgumentNullException("source");
            if (dest == null) throw new ArgumentNullException("dest");
            if ((source.Width > dest.Width) || (source.Height > dest.Height)) throw new ArgumentException("Destination image (" + dest.Width + "x" + dest.Height + ") must have at least the same size as the source image (" + source.Width + "x" + source.Height + ")");

            KernelImageFloatGrayScale.SetArguments(source, dest);
            queue.Execute(KernelImageFloatGrayScale, null, new long[] { source.Width, source.Height }, null, null);
            dest.Modified = true;
            dest.Normalized = source.Normalized;
        }

        /// <summary>
        /// Create histogram
        /// </summary>
        /// <param name="queue">cloo command queue</param>
        /// <param name="source">image source</param>
        /// <param name="histogram">byte buffer (256 bytes)</param>
        /// <param name="startX">start from X coordinate</param>
        /// <param name="startY">start from Y coordinate</param>
        /// <param name="width">width</param>
        /// <param name="height">height</param>
        public void Histogram256(ClooCommandQueue queue, ClooImage2DByteA source, ClooBuffer<uint> histogram,
            int startX = 0, int startY = 0, int width = 0, int height = 0)
        {
            if (queue == null) throw new ArgumentNullException("queue");
            if (source == null) throw new ArgumentNullException("source");
            if (histogram == null) throw new ArgumentNullException("histogram");

            if (histogram.Size < 256) throw new ArgumentException("Buffer size for histogram must be at least 256 bytes", "histogram");
            if (width == 0) width = source.Width - startX;
            if (height == 0) height = source.Height - startY;
            if (width < 0) throw new ArgumentOutOfRangeException("width");
            if (height < 0) throw new ArgumentOutOfRangeException("height");

            KernelCoreUIntSetValue.SetArguments(histogram, (uint)0);
            queue.Execute(KernelCoreUIntSetValue, null, new long[] { 256 }, null, null);

            KernelImageByteHistogram256.SetArguments(source, histogram, (uint)startX, (uint)startY);
            queue.Execute(KernelImageByteHistogram256, null, new long[] { width, height }, null, null);
            histogram.Modified = true;
        }

        /// <summary>
        /// Create color histogram with N bins
        /// </summary>
        /// <param name="queue">cloo command queue</param>
        /// <param name="source">image source</param>
        /// <param name="histogram">byte buffer (must be at least bins^3 bytes in size)</param>
        /// <param name="bins">number of bins</param>
        /// <param name="startX">start from X coordinate</param>
        /// <param name="startY">start from Y coordinate</param>
        /// <param name="width">width</param>
        /// <param name="height">height</param>
        public void HistogramN(ClooCommandQueue queue, ClooImage2DByteRgbA source, ClooBuffer<uint> histogram,
            byte bins, int startX = 0, int startY = 0, int width = 0, int height = 0)
        {
            if (queue == null) throw new ArgumentNullException("queue");
            if (source == null) throw new ArgumentNullException("source");
            if (histogram == null) throw new ArgumentNullException("histogram");

            int size = bins * bins * bins;
            if (histogram.Size < size) throw new ArgumentException("Buffer size for histogram must be at least " + size + " bytes", "histogram");
            if (width == 0) width = source.Width - startX;
            if (height == 0) height = source.Height - startY;
            if (width < 0) throw new ArgumentOutOfRangeException("width");
            if (height < 0) throw new ArgumentOutOfRangeException("height");

            KernelCoreUIntSetValue.SetArguments(histogram, (uint)0);
            queue.Execute(KernelCoreUIntSetValue, null, new long[] { 256 }, null, null);

            KernelImageByteRgbHistogramN.SetArguments(source, histogram, bins, (uint)startX, (uint)startY);
            queue.Execute(KernelImageByteRgbHistogramN, null, new long[] { width, height }, null, null);
            histogram.Modified = true;
        }

        /// <summary>
        /// Convert an float image to an integral image
        /// </summary>
        /// <remarks>
        /// We skip the last line to keep the original size (better performance)
        /// </remarks>
        /// <param name="queue">cloo command queue</param>
        /// <param name="source">image source</param>
        /// <param name="dest">image destination</param>
        public void Integral(ClooCommandQueue queue, ClooImage2DFloatA source, ClooImage2DFloatA dest)
        {
            if (queue == null) throw new ArgumentNullException("queue");
            if (source == null) throw new ArgumentNullException("source");
            if (dest == null) throw new ArgumentNullException("dest");
            if (source == dest) throw new ArgumentNullException("Source image and destination image must not be the same instance");
            if ((source.Width > dest.Width) || (source.Height > dest.Height)) throw new ArgumentException("Destination image (" + dest.Width + "x" + dest.Height + ") must have at least the source image width and height but is only " + source.Width + "x" + source.Height);

            // execute step 1
            SetValue(queue, dest, 0);
            KernelImageFloatIntegralStep1.SetArguments(source, dest);
            queue.Execute(KernelImageFloatIntegralStep1, null, new long[] { source.Height - 1 }, null, null);
            dest.Modified = true;

            // execute step 2
            KernelImageFloatIntegral.SetArguments(dest, dest, source.Height);
            queue.Execute(KernelImageFloatIntegral, null, new long[] { source.Width - 1 }, null, null);
            dest.Modified = true;

            dest.Normalized = false;
        }

        /// <summary>
        /// Convert an float image to an integral image
        /// </summary>
        /// <remarks>
        /// We skip the last line to keep the original size (better performance)
        /// </remarks>
        /// <param name="queue">cloo command queue</param>
        /// <param name="source">image source</param>
        /// <param name="dest">image destination</param>
        public void Integral(ClooCommandQueue queue, ClooImage2DByteA source, ClooImage2DUIntA dest)
        {
            if (queue == null) throw new ArgumentNullException("queue");
            if (source == null) throw new ArgumentNullException("source");
            if (dest == null) throw new ArgumentNullException("dest");
            if ((source.Width > dest.Width) || (source.Height > dest.Height)) throw new ArgumentException("Destination image (" + dest.Width + "x" + dest.Height + ") must have at least the source image width and height but is only " + source.Width + "x" + source.Height);

            // execute step 1
            SetValue(queue, dest, 0);
            KernelImageUIntIntegralStep1.SetArguments(source, dest);
            queue.Execute(KernelImageUIntIntegralStep1, null, new long[] { source.Height - 1 }, null, null);
            dest.Modified = true;

            // execute step 2
            KernelImageUIntIntegral.SetArguments(dest, dest, source.Height);
            queue.Execute(KernelImageUIntIntegral, null, new long[] { source.Width - 1 }, null, null);
            dest.Modified = true;
        }

        /// <summary>
        /// Convert an float image to a squared integral image
        /// </summary>
        /// <remarks>
        /// We skip the last line to keep the original size (better performance)
        /// </remarks>
        /// <param name="queue">cloo command queue</param>
        /// <param name="source">image source</param>
        /// <param name="dest">image destination</param>
        public void IntegralSquare(ClooCommandQueue queue, ClooImage2DFloatA source, ClooImage2DFloatA dest)
        {
            if (queue == null) throw new ArgumentNullException("queue");
            if (source == null) throw new ArgumentNullException("source");
            if (dest == null) throw new ArgumentNullException("dest");
            if (source == dest) throw new ArgumentNullException("Source image and destination image must not be the same instance");
            if ((source.Width > dest.Width) || (source.Height > dest.Height)) throw new ArgumentException("Destination image (" + dest.Width + "x" + dest.Height + ") must have at least the source image width and height but is only " + source.Width + "x" + source.Height);

            // execute step 1
            SetValue(queue, dest, 0);
            KernelImageFloatIntegralSquareStep1.SetArguments(source, dest);
            queue.Execute(KernelImageFloatIntegralSquareStep1, null, new long[] { source.Height - 1 }, null, null);
            dest.Modified = true;

            // execute step 2
            KernelImageFloatIntegralSquare.SetArguments(dest, dest, source.Height);
            queue.Execute(KernelImageFloatIntegralSquare, null, new long[] { source.Width - 1 }, null, null);
            dest.Modified = true;

            dest.Normalized = false;
        }

        /// <summary>
        /// Convert an float image to a squared integral image
        /// </summary>
        /// <remarks>
        /// We skip the last line to keep the original size (better performance)
        /// </remarks>
        /// <param name="queue">cloo command queue</param>
        /// <param name="source">image source</param>
        /// <param name="dest">image destination</param>
        public void IntegralSquare(ClooCommandQueue queue, ClooImage2DByteA source, ClooImage2DUIntA dest)
        {
            if (queue == null) throw new ArgumentNullException("queue");
            if (source == null) throw new ArgumentNullException("source");
            if (dest == null) throw new ArgumentNullException("dest");
            if ((source.Width > dest.Width) || (source.Height > dest.Height)) throw new ArgumentException("Destination image (" + dest.Width + "x" + dest.Height + ") must have at least the source image width and height but is only " + source.Width + "x" + source.Height);

            // execute step 1
            SetValue(queue, dest, 0);
            KernelImageUIntIntegralSquareStep1.SetArguments(source, dest);
            queue.Execute(KernelImageUIntIntegralSquareStep1, null, new long[] { source.Height - 1 }, null, null);
            dest.Modified = true;

            // execute step 2
            KernelImageUIntIntegralSquare.SetArguments(dest, dest, source.Height);
            queue.Execute(KernelImageUIntIntegralSquare, null, new long[] { source.Width - 1 }, null, null);
            dest.Modified = true;
        }

        /// <summary>
        /// HSL to RGB image
        /// </summary>
        /// <param name="queue">cloo command queue</param>
        /// <param name="source">image source</param>
        /// <param name="dest">image destination</param>
        public void HslToRgb(ClooCommandQueue queue, ClooImage2DByteRgbA source, ClooImage2DByteRgbA dest)
        {
            if (queue == null) throw new ArgumentNullException("queue");
            if (source == null) throw new ArgumentNullException("source");
            if (dest == null) throw new ArgumentNullException("dest");
            if ((source.Width > dest.Width) || (source.Height > dest.Height)) throw new ArgumentException("Destination image (" + dest.Width + "x" + dest.Height + ") must have at least the same size as the source image (" + source.Width + "x" + source.Height + ")");

            // use normal kernel
            KernelImageByteHslToRgb.SetArguments(source, dest);
            queue.Execute(KernelImageByteHslToRgb, null, new long[] { source.Width, source.Height }, null, null);

            dest.Modified = true;
        }

        /// <summary>
        /// HSL to RGB image
        /// </summary>
        /// <param name="queue">cloo command queue</param>
        /// <param name="source">image source</param>
        /// <param name="dest">image destination</param>
        public void HslToRgb(ClooCommandQueue queue, ClooImage2DFloatRgbA source, ClooImage2DFloatRgbA dest)
        {
            if (queue == null) throw new ArgumentNullException("queue");
            if (source == null) throw new ArgumentNullException("source");
            if (dest == null) throw new ArgumentNullException("dest");
            if ((source.Width > dest.Width) || (source.Height > dest.Height)) throw new ArgumentException("Destination image (" + dest.Width + "x" + dest.Height + ") must have at least the same size as the source image (" + source.Width + "x" + source.Height + ")");

            if (source.Normalized)
            {
                // use normalized kernel
                KernelImageFloatHslToRgbNorm.SetArguments(source, dest);
                queue.Execute(KernelImageFloatHslToRgbNorm, null, new long[] { source.Width, source.Height }, null, null);
            }
            else
            {
                // use normal kernel
                KernelImageFloatHslToRgb.SetArguments(source, dest);
                queue.Execute(KernelImageFloatHslToRgb, null, new long[] { source.Width, source.Height }, null, null);
            }
            dest.Modified = true;
            dest.Normalized = source.Normalized;
        }

        /// <summary>
        /// Multiplies a constant value to all float values in an image
        /// </summary>
        /// <param name="queue">cloo command queue</param>
        /// <param name="source">image source</param>
        /// <param name="dest">image destination</param>
        /// <param name="factor">factor to multiply</param>
        public void MultiplyValue(ClooCommandQueue queue, ClooImage2DFloatA source, ClooImage2DFloatA dest, float factor)
        {
            if (queue == null) throw new ArgumentNullException("queue");
            if (source == null) throw new ArgumentNullException("source");
            if (dest == null) throw new ArgumentNullException("dest");
            if ((source.Width > dest.Width) || (source.Height > dest.Height)) throw new ArgumentException("Destination image (" + dest.Width + "x" + dest.Height + ") must have at least the same size as the source image (" + source.Width + "x" + source.Height + ")");

            KernelImageFloatMultiplyValue.SetArguments(source, dest, factor);
            queue.Execute(KernelImageFloatMultiplyValue, null, new long[] { source.Width, source.Height }, null, null);
            dest.Modified = true;
            dest.Normalized = source.Normalized;
        }

        /// <summary>
        /// Multiplies a constant value to all float values in an image
        /// </summary>
        /// <param name="queue">cloo command queue</param>
        /// <param name="image">image</param>
        /// <param name="factor">factor to multiply</param>
        public void MultiplyValue(ClooCommandQueue queue, ClooImage2DFloatA image, float factor)
        {
            MultiplyValue(queue, image, image, factor);
        }

        /// <summary>
        /// Multiplies a constant value to all float values in an image
        /// </summary>
        /// <param name="queue">cloo command queue</param>
        /// <param name="source">image source</param>
        /// <param name="dest">image destination</param>
        /// <param name="factor">factor to multiply</param>
        public void MultiplyValue(ClooCommandQueue queue, ClooImage2DFloatRgbA source, ClooImage2DFloatRgbA dest, float factor)
        {
            if (queue == null) throw new ArgumentNullException("queue");
            if (source == null) throw new ArgumentNullException("source");
            if (dest == null) throw new ArgumentNullException("dest");
            if ((source.Width > dest.Width) || (source.Height > dest.Height)) throw new ArgumentException("Destination image (" + dest.Width + "x" + dest.Height + ") must have at least the same size as the source image (" + source.Width + "x" + source.Height + ")");

            KernelImageFloatMultiplyValue.SetArguments(source, dest, factor);
            queue.Execute(KernelImageFloatMultiplyValue, null, new long[] { source.Width, source.Height }, null, null);
            dest.Modified = true;
            dest.Normalized = source.Normalized;
        }

        /// <summary>
        /// Multiplies a constant value to all float values in an image
        /// </summary>
        /// <param name="queue">cloo command queue</param>
        /// <param name="image">image</param>
        /// <param name="factor">factor to multiply</param>
        public void MultiplyValue(ClooCommandQueue queue, ClooImage2DFloatRgbA image, float factor)
        {
            MultiplyValue(queue, image, image, factor);
        }

        /// <summary>
        /// Normalize float image
        /// </summary>
        /// <param name="queue">cloo command queue</param>
        /// <param name="source">image source</param>
        /// <param name="dest">image destination</param>
        public void Normalize(ClooCommandQueue queue, ClooImage2DFloatA source, ClooImage2DFloatA dest)
        {
            if (queue == null) throw new ArgumentNullException("queue");
            if (source == null) throw new ArgumentNullException("source");
            if (dest == null) throw new ArgumentNullException("dest");
            if ((source.Width > dest.Width) || (source.Height > dest.Height)) throw new ArgumentException("Destination image (" + dest.Width + "x" + dest.Height + ") must have at least the same size as the source image (" + source.Width + "x" + source.Height + ")");

            KernelImageFloatMultiplyValue.SetArguments(source, dest, 1f / 255f);
            queue.Execute(KernelImageFloatMultiplyValue, null, new long[] { source.Width, source.Height }, null, null);
            dest.Modified = true;
            dest.Normalized = true;
        }

        /// <summary>
        /// Normalize float image
        /// </summary>
        /// <param name="queue">cloo command queue</param>
        /// <param name="image">image</param>
        public void Normalize(ClooCommandQueue queue, ClooImage2DFloatA image)
        {
            Normalize(queue, image, image);
        }

        /// <summary>
        /// Normalize float image
        /// </summary>
        /// <param name="queue">cloo command queue</param>
        /// <param name="source">image source</param>
        /// <param name="dest">image destination</param>
        public void Normalize(ClooCommandQueue queue, ClooImage2DFloatRgbA source, ClooImage2DFloatRgbA dest)
        {
            if (queue == null) throw new ArgumentNullException("queue");
            if (source == null) throw new ArgumentNullException("source");
            if (dest == null) throw new ArgumentNullException("dest");
            if ((source.Width > dest.Width) || (source.Height > dest.Height)) throw new ArgumentException("Destination image (" + dest.Width + "x" + dest.Height + ") must have at least the same size as the source image (" + source.Width + "x" + source.Height + ")");

            KernelImageFloatMultiplyValue.SetArguments(source, dest, 1f / 255f);
            queue.Execute(KernelImageFloatMultiplyValue, null, new long[] { source.Width, source.Height }, null, null);
            dest.Modified = true;
            dest.Normalized = true;
        }

        /// <summary>
        /// Normalize float image
        /// </summary>
        /// <param name="queue">cloo command queue</param>
        /// <param name="image">image</param>
        public void Normalize(ClooCommandQueue queue, ClooImage2DFloatRgbA image)
        {
            Normalize(queue, image, image);
        }

        /// <summary>
        /// RGB to HSL image
        /// </summary>
        /// <param name="queue">cloo command queue</param>
        /// <param name="source">image source</param>
        /// <param name="dest">image destination</param>
        public void RgbToHsl(ClooCommandQueue queue, ClooImage2DByteRgbA source, ClooImage2DByteRgbA dest)
        {
            if (queue == null) throw new ArgumentNullException("queue");
            if (source == null) throw new ArgumentNullException("source");
            if (dest == null) throw new ArgumentNullException("dest");
            if ((source.Width > dest.Width) || (source.Height > dest.Height)) throw new ArgumentException("Destination image (" + dest.Width + "x" + dest.Height + ") must have at least the same size as the source image (" + source.Width + "x" + source.Height + ")");

            // use normal kernel
            KernelImageByteRgbToHsl.SetArguments(source, dest);
            queue.Execute(KernelImageByteRgbToHsl, null, new long[] { source.Width, source.Height }, null, null);

            dest.Modified = true;
        }

        /// <summary>
        /// RGB to HSL image
        /// </summary>
        /// <param name="queue">cloo command queue</param>
        /// <param name="source">image source</param>
        /// <param name="dest">image destination</param>
        public void RgbToHsl(ClooCommandQueue queue, ClooImage2DFloatRgbA source, ClooImage2DFloatRgbA dest)
        {
            if (queue == null) throw new ArgumentNullException("queue");
            if (source == null) throw new ArgumentNullException("source");
            if (dest == null) throw new ArgumentNullException("dest");
            if ((source.Width > dest.Width) || (source.Height > dest.Height)) throw new ArgumentException("Destination image (" + dest.Width + "x" + dest.Height + ") must have at least the same size as the source image (" + source.Width + "x" + source.Height + ")");

            if (source.Normalized)
            {
                // use normalized kernel
                KernelImageFloatRgbToHslNorm.SetArguments(source, dest);
                queue.Execute(KernelImageFloatRgbToHslNorm, null, new long[] { source.Width, source.Height }, null, null);
            }
            else
            {
                // use normal kernel
                KernelImageFloatRgbToHsl.SetArguments(source, dest);
                queue.Execute(KernelImageFloatRgbToHsl, null, new long[] { source.Width, source.Height }, null, null);
            }
            dest.Modified = true;
            dest.Normalized = source.Normalized;
        }

        /// <summary>
        /// Sets channel of a RGBA image
        /// </summary>
        /// <param name="queue">cloo command queue</param>
        /// <param name="source">source image</param>
        /// <param name="mask">mask image</param>
        /// <param name="dest">destination image</param>
        /// <param name="offset">offset (0..3)</param>
        public void SetChannel(ClooCommandQueue queue, ClooImage2DByteRgbA source, ClooImage2DByteA mask, ClooImage2DByteRgbA dest, byte offset)
        {
            if (queue == null) throw new ArgumentNullException("queue");
            if (source == null) throw new ArgumentNullException("source");
            if (mask == null) throw new ArgumentNullException("mask");
            if (dest == null) throw new ArgumentNullException("dest");
            if (offset > 3) throw new ArgumentOutOfRangeException("offset", String.Format("offset must be between 0..3 but was {0}", offset));

            if ((source.Width > mask.Width) || (source.Height > mask.Height)) throw new ArgumentException("Image mask (" + dest.Width + "x" + dest.Height + ") must have at least the same size as the source image (" + source.Width + "x" + source.Height + ")");
            if ((source.Width > dest.Width) || (source.Height > dest.Height)) throw new ArgumentException("Destination image (" + dest.Width + "x" + dest.Height + ") must have at least the same size as the source image (" + source.Width + "x" + source.Height + ")");

            KernelImageByteSetChannel.SetArguments(source, mask, dest, offset);
            queue.Execute(KernelImageByteSetChannel, null, new long[] { source.Width, source.Height }, null, null);
            dest.Modified = true;
        }

        /// <summary>
        /// Sets channel of a RGBA image
        /// </summary>
        /// <param name="queue">cloo command queue</param>
        /// <param name="image">image</param>
        /// <param name="mask">mask image</param>
        /// <param name="offset">offset (0..3)</param>
        public void SetChannel(ClooCommandQueue queue, ClooImage2DByteRgbA image, ClooImage2DByteA mask, byte offset)
        {
            SetChannel(queue, image, mask, image, offset);
        }

        /// <summary>
        /// Sets channel of a RGBA image
        /// </summary>
        /// <param name="queue">cloo command queue</param>
        /// <param name="source">source image</param>
        /// <param name="dest">destination image</param>
        /// <param name="offset">offset (0..3)</param>
        /// <param name="value">constant mask value</param>
        public void SetChannel(ClooCommandQueue queue, ClooImage2DByteRgbA source, ClooImage2DByteRgbA dest, byte offset, byte value)
        {
            if (queue == null) throw new ArgumentNullException("queue");
            if (source == null) throw new ArgumentNullException("source");
            if (dest == null) throw new ArgumentNullException("dest");
            if (offset > 3) throw new ArgumentOutOfRangeException("offset", String.Format("offset must be between 0..3 but was {0}", offset));

            if ((source.Width > dest.Width) || (source.Height > dest.Height)) throw new ArgumentException("Destination image (" + dest.Width + "x" + dest.Height + ") must have at least the same size as the source image (" + source.Width + "x" + source.Height + ")");

            KernelImageByteSetChannelConstant.SetArguments(source, dest, offset, (uint)value);
            queue.Execute(KernelImageByteSetChannelConstant, null, new long[] { source.Width, source.Height }, null, null);
            dest.Modified = true;
        }

        /// <summary>
        /// Sets channel of a RGBA image
        /// </summary>
        /// <param name="queue">cloo command queue</param>
        /// <param name="image">image</param>
        /// <param name="mask">mask image</param>
        /// <param name="offset">offset (0..3)</param>
        /// <param name="value">constant mask value</param>
        public void SetChannel(ClooCommandQueue queue, ClooImage2DByteRgbA image, byte offset, byte value)
        {
            SetChannel(queue, image, image, offset, value);
        }

        /// <summary>
        /// Sets channel of a RGBA image
        /// </summary>
        /// <param name="queue">cloo command queue</param>
        /// <param name="source">source image</param>
        /// <param name="mask">mask image</param>
        /// <param name="dest">destination image</param>
        /// <param name="offset">offset (0..3)</param>
        public void SetChannel(ClooCommandQueue queue, ClooImage2DFloatRgbA source, ClooImage2DFloatA mask, ClooImage2DFloatRgbA dest, byte offset)
        {
            if (queue == null) throw new ArgumentNullException("queue");
            if (source == null) throw new ArgumentNullException("source");
            if (mask == null) throw new ArgumentNullException("mask");
            if (dest == null) throw new ArgumentNullException("dest");
            if (offset > 3) throw new ArgumentOutOfRangeException("offset", String.Format("offset must be between 0..3 but was {0}", offset));

            if ((source.Width > mask.Width) || (source.Height > mask.Height)) throw new ArgumentException("Image mask (" + dest.Width + "x" + dest.Height + ") must have at least the same size as the source image (" + source.Width + "x" + source.Height + ")");
            if ((source.Width > dest.Width) || (source.Height > dest.Height)) throw new ArgumentException("Destination image (" + dest.Width + "x" + dest.Height + ") must have at least the same size as the source image (" + source.Width + "x" + source.Height + ")");

            KernelImageFloatSetChannel.SetArguments(source, mask, dest, offset);
            queue.Execute(KernelImageFloatSetChannel, null, new long[] { source.Width, source.Height }, null, null);
            dest.Modified = true;
            dest.Normalized = source.Normalized;
        }

        /// <summary>
        /// Sets channel of a RGBA image
        /// </summary>
        /// <param name="queue">cloo command queue</param>
        /// <param name="image">image</param>
        /// <param name="mask">mask image</param>
        /// <param name="offset">offset (0..3)</param>
        public void SetChannel(ClooCommandQueue queue, ClooImage2DFloatRgbA image, ClooImage2DFloatA mask, byte offset)
        {
            SetChannel(queue, image, mask, image, offset);
        }

        /// <summary>
        /// Sets channel of a RGBA image
        /// </summary>
        /// <param name="queue">cloo command queue</param>
        /// <param name="source">source image</param>
        /// <param name="dest">destination image</param>
        /// <param name="offset">offset (0..3)</param>
        /// <param name="value">constant mask value</param>
        public void SetChannel(ClooCommandQueue queue, ClooImage2DFloatRgbA source, ClooImage2DFloatRgbA dest, byte offset, float value)
        {
            if (queue == null) throw new ArgumentNullException("queue");
            if (source == null) throw new ArgumentNullException("source");
            if (dest == null) throw new ArgumentNullException("dest");
            if (offset > 3) throw new ArgumentOutOfRangeException("offset", String.Format("offset must be between 0..3 but was {0}", offset));

            if ((source.Width > dest.Width) || (source.Height > dest.Height)) throw new ArgumentException("Destination image (" + dest.Width + "x" + dest.Height + ") must have at least the same size as the source image (" + source.Width + "x" + source.Height + ")");

            KernelImageFloatSetChannelConstant.SetArguments(source, dest, offset, value);
            queue.Execute(KernelImageFloatSetChannelConstant, null, new long[] { source.Width, source.Height }, null, null);
            dest.Modified = true;
            dest.Normalized = source.Normalized;
        }

        /// <summary>
        /// Sets channel of a RGBA image
        /// </summary>
        /// <param name="queue">cloo command queue</param>
        /// <param name="image">image</param>
        /// <param name="mask">mask image</param>
        /// <param name="offset">offset (0..3)</param>
        /// <param name="value">constant mask value</param>
        public void SetChannel(ClooCommandQueue queue, ClooImage2DFloatRgbA image, byte offset, byte value)
        {
            SetChannel(queue, image, image, offset, value);
        }

        /// <summary>
        /// Sets a constant value to all cells in an image
        /// </summary>
        /// <param name="queue">cloo command queue</param>
        /// <param name="image">image</param>
        /// <param name="value">value to set</param>
        public void SetValue(ClooCommandQueue queue, ClooImage2DByteA image, byte value)
        {
            if (queue == null) throw new ArgumentNullException("queue");
            if (image == null) throw new ArgumentNullException("dest");

            KernelImageByteSetValueA.SetArguments(image, (uint)value);
            queue.Execute(KernelImageByteSetValueA, null, new long[] { image.Width, image.Height }, null, null);
            image.Modified = true;
        }

        /// <summary>
        /// Sets a constant value to all cells in an image
        /// </summary>
        /// <param name="queue">cloo command queue</param>
        /// <param name="image">image</param>
        /// <param name="value">value to set</param>
        public void SetValue(ClooCommandQueue queue, ClooImage2DByteRgbA image, byte value)
        {
            if (queue == null) throw new ArgumentNullException("queue");
            if (image == null) throw new ArgumentNullException("image");

            KernelImageByteSetValueA.SetArguments(image, (uint)value);
            queue.Execute(KernelImageByteSetValueA, null, new long[] { image.Width, image.Height }, null, null);
            image.Modified = true;
        }

        /// <summary>
        /// Sets a constant value to all cells in an image
        /// </summary>
        /// <param name="queue">cloo command queue</param>
        /// <param name="image">image</param>
        /// <param name="value">value to set</param>
        public void SetValue(ClooCommandQueue queue, ClooImage2DByteRgbA image, uint value)
        {
            if (queue == null) throw new ArgumentNullException("queue");
            if (image == null) throw new ArgumentNullException("image");

            uint r = (value >> 24) % 256;
            uint g = (value >> 16) % 256;
            uint b = (value >> 8) % 256;
            uint a = (value) % 256;
            KernelImageByteSetValueRgbA.SetArguments(image, r, g, b, a);
            queue.Execute(KernelImageByteSetValueRgbA, null, new long[] { image.Width, image.Height }, null, null);
            image.Modified = true;
        }

        /// <summary>
        /// Sets a constant value to all float values in an image
        /// </summary>
        /// <param name="queue">cloo command queue</param>
        /// <param name="image">image</param>
        /// <param name="value">value to set</param>
        public void SetValue(ClooCommandQueue queue, ClooImage2DFloatA image, float value)
        {
            if (queue == null) throw new ArgumentNullException("queue");
            if (image == null) throw new ArgumentNullException("image");

            KernelImageFloatSetValue.SetArguments(image, value);
            queue.Execute(KernelImageFloatSetValue, null, new long[] { image.Width, image.Height }, null, null);
            image.Modified = true;
        }

        /// <summary>
        /// Sets a constant value to all float values in an image
        /// </summary>
        /// <param name="queue">cloo command queue</param>
        /// <param name="image">image</param>
        /// <param name="value">value to set</param>
        public void SetValue(ClooCommandQueue queue, ClooImage2DFloatRgbA image, float value)
        {
            if (queue == null) throw new ArgumentNullException("queue");
            if (image == null) throw new ArgumentNullException("image");

            KernelImageFloatSetValue.SetArguments(image, value);
            queue.Execute(KernelImageFloatSetValue, null, new long[] { image.Width, image.Height }, null, null);
            image.Modified = true;
        }

        /// <summary>
        /// Sets a constant value to all cells in an image
        /// </summary>
        /// <param name="queue">cloo command queue</param>
        /// <param name="image">image</param>
        /// <param name="value">value to set</param>
        public void SetValue(ClooCommandQueue queue, ClooImage2DUIntA image, uint value)
        {
            if (queue == null) throw new ArgumentNullException("queue");
            if (image == null) throw new ArgumentNullException("dest");

            KernelImageUIntSetValueA.SetArguments(image, value);
            queue.Execute(KernelImageUIntSetValueA, null, new long[] { image.Width, image.Height }, null, null);
            image.Modified = true;
        }

        /// <summary>
        /// Sobel filter image
        /// </summary>
        /// <param name="queue">cloo command queue</param>
        /// <param name="source">image source</param>
        /// <param name="dest">image destination</param>
        /// <param name="sampler">sampler to be used for image reading</param>
        public void Sobel(ClooCommandQueue queue, ClooImage2DByteA source, ClooImage2DByteA dest, ClooSampler sampler)
        {
            if (queue == null) throw new ArgumentNullException("queue");
            if (source == null) throw new ArgumentNullException("source");
            if (dest == null) throw new ArgumentNullException("dest");
            if (sampler == null) throw new ArgumentNullException("sampler");
            if ((source.Width > dest.Width) || (source.Height > dest.Height)) throw new ArgumentException("Destination image (" + dest.Width + "x" + dest.Height + ") must have at least the same size as the source image (" + source.Width + "x" + source.Height + ")");

            KernelImageByteSobel.SetArguments(source, dest, sampler);
            queue.Execute(KernelImageByteSobel, null, new long[] { source.Width, source.Height }, null, null);
            dest.Modified = true;
        }

        /// <summary>
        /// Sobel filter image
        /// </summary>
        /// <param name="queue">cloo command queue</param>
        /// <param name="source">image source</param>
        /// <param name="dest">image destination</param>
        /// <param name="sampler">sampler to be used for image reading</param>
        public void Sobel(ClooCommandQueue queue, ClooImage2DByteRgbA source, ClooImage2DByteRgbA dest, ClooSampler sampler)
        {
            if (queue == null) throw new ArgumentNullException("queue");
            if (source == null) throw new ArgumentNullException("source");
            if (dest == null) throw new ArgumentNullException("dest");
            if (sampler == null) throw new ArgumentNullException("sampler");
            if ((source.Width > dest.Width) || (source.Height > dest.Height)) throw new ArgumentException("Destination image (" + dest.Width + "x" + dest.Height + ") must have at least the same size as the source image (" + source.Width + "x" + source.Height + ")");

            KernelImageByteSobel.SetArguments(source, dest, sampler);
            queue.Execute(KernelImageByteSobel, null, new long[] { source.Width, source.Height }, null, null);
            dest.Modified = true;
        }

        /// <summary>
        /// Sobel filter image
        /// </summary>
        /// <param name="queue">cloo command queue</param>
        /// <param name="source">image source</param>
        /// <param name="dest">image destination</param>
        /// <param name="sampler">sampler to be used for image reading</param>
        public void Sobel(ClooCommandQueue queue, ClooImage2DFloatA source, ClooImage2DFloatA dest, ClooSampler sampler)
        {
            if (queue == null) throw new ArgumentNullException("queue");
            if (source == null) throw new ArgumentNullException("source");
            if (dest == null) throw new ArgumentNullException("dest");
            if (sampler == null) throw new ArgumentNullException("sampler");
            if ((source.Width > dest.Width) || (source.Height > dest.Height)) throw new ArgumentException("Destination image (" + dest.Width + "x" + dest.Height + ") must have at least the same size as the source image (" + source.Width + "x" + source.Height + ")");

            KernelImageFloatSobel.SetArguments(source, dest, sampler);
            queue.Execute(KernelImageFloatSobel, null, new long[] { source.Width, source.Height }, null, null);
            dest.Modified = true;
            dest.Normalized = source.Normalized;
        }

        /// <summary>
        /// Sobel filter image
        /// </summary>
        /// <param name="queue">cloo command queue</param>
        /// <param name="source">image source</param>
        /// <param name="dest">image destination</param>
        /// <param name="sampler">sampler to be used for image reading</param>
        public void Sobel(ClooCommandQueue queue, ClooImage2DFloatRgbA source, ClooImage2DFloatRgbA dest, ClooSampler sampler)
        {
            if (queue == null) throw new ArgumentNullException("queue");
            if (source == null) throw new ArgumentNullException("source");
            if (dest == null) throw new ArgumentNullException("dest");
            if (sampler == null) throw new ArgumentNullException("sampler");
            if ((source.Width > dest.Width) || (source.Height > dest.Height)) throw new ArgumentException("Destination image (" + dest.Width + "x" + dest.Height + ") must have at least the same size as the source image (" + source.Width + "x" + source.Height + ")");

            KernelImageFloatSobel.SetArguments(source, dest, sampler);
            queue.Execute(KernelImageFloatSobel, null, new long[] { source.Width, source.Height }, null, null);
            dest.Modified = true;
            dest.Normalized = source.Normalized;
        }

        /// <summary>
        /// Swaps two channels in an image
        /// </summary>
        /// <param name="queue">cloo command queue</param>
        /// <param name="source">source image</param>
        /// <param name="dest">destination image</param>
        /// <param name="offsetFrom">first channel offset (0..3)</param>
        /// <param name="offsetTo">second channel offset (0..3)</param>
        public void SwapChannel(ClooCommandQueue queue, ClooImage2DByteRgbA source, ClooImage2DByteRgbA dest, byte offsetFrom, byte offsetTo)
        {
            if (queue == null) throw new ArgumentNullException("queue");
            if (source == null) throw new ArgumentNullException("source");
            if (dest == null) throw new ArgumentNullException("dest");
            if (offsetFrom > 3) throw new ArgumentOutOfRangeException("offsetFrom", String.Format("offset must be between 0..3 but was {0}", offsetFrom));
            if (offsetTo > 3) throw new ArgumentOutOfRangeException("offsetTo", String.Format("offset must be between 0..3 but was {0}", offsetTo));

            if ((source.Width > dest.Width) || (source.Height > dest.Height)) throw new ArgumentException("Destination image (" + dest.Width + "x" + dest.Height + ") must have at least the same size as the source image (" + source.Width + "x" + source.Height + ")");

            KernelImageByteSwapChannel.SetArguments(source, dest, offsetFrom, offsetTo);
            queue.Execute(KernelImageByteSwapChannel, null, new long[] { source.Width, source.Height }, null, null);
            dest.Modified = true;
        }

        /// <summary>
        /// Swaps two channels in an image
        /// </summary>
        /// <param name="queue">cloo command queue</param>
        /// <param name="source">source image</param>
        /// <param name="dest">destination image</param>
        /// <param name="offsetFrom">first channel offset (0..3)</param>
        /// <param name="offsetTo">second channel offset (0..3)</param>
        public void SwapChannel(ClooCommandQueue queue, ClooImage2DFloatRgbA source, ClooImage2DFloatRgbA dest, byte offsetFrom, byte offsetTo)
        {
            if (queue == null) throw new ArgumentNullException("queue");
            if (source == null) throw new ArgumentNullException("source");
            if (dest == null) throw new ArgumentNullException("dest");
            if (offsetFrom > 3) throw new ArgumentOutOfRangeException("offsetFrom", String.Format("offset must be between 0..3 but was {0}", offsetFrom));
            if (offsetTo > 3) throw new ArgumentOutOfRangeException("offsetTo", String.Format("offset must be between 0..3 but was {0}", offsetTo));

            if ((source.Width > dest.Width) || (source.Height > dest.Height)) throw new ArgumentException("Destination image (" + dest.Width + "x" + dest.Height + ") must have at least the same size as the source image (" + source.Width + "x" + source.Height + ")");

            KernelImageFloatSwapChannel.SetArguments(source, dest, offsetFrom, offsetTo);
            queue.Execute(KernelImageFloatSwapChannel, null, new long[] { source.Width, source.Height }, null, null);
            dest.Modified = true;
            dest.Normalized = source.Normalized;
        }

        public void yuvtorgb(ClooCommandQueue queue, int[] sizesV, ClooBuffer<byte> source, ClooImage2DFloatRgbA dest, ClooBuffer<int> sizes)
        {
            if (queue == null) throw new ArgumentNullException("queue");
            if (source == null) throw new ArgumentNullException("source");
            if (dest == null) throw new ArgumentNullException("dest");

            //if ((source.Width > dest.Width) || (source.Height > dest.Height)) throw new ArgumentException("Destination image (" + dest.Width + "x" + dest.Height + ") must have at least the same size as the source image (" + source.Width + "x" + source.Height + ")");

            KernelSpecialKernel[0].SetArguments(source, dest, sizes);
            queue.Execute(KernelSpecialKernel[0], null, new long[] { sizesV[1], sizesV[0] }, null, null);
            dest.Modified = true;
        }

        /// <summary>
        /// Special Kernel
        /// </summary>
        /// <param name="queue">cloo command queue</param>
        /// <param name="source">source image</param>
        /// <param name="dest">destination image</param>
        /// <param name="offsetFrom">first channel offset (0..3)</param>
        /// <param name="offsetTo">second channel offset (0..3)</param>
        bool crappy_device = false;
        ClooBuffer<int> startFromBuff=null;  // keep this global so it doesn't need to be reinitialized
        int[][] startFrom = new int[][] { new int[2] { 1, 2}, new int[2] { 1, 2 }, new int[2] { 1, 2 } };
        public void SpecialKernel(ClooCommandQueue queue, ClooContext _context, int kernNum, ClooBuffer<byte> final_out = null, params ClooImage2DFloatRgbA[] args) //ClooImage2DByteRgbA source, ClooImage2DByteRgbA dest)//, ClooSampler sampler)
        {
            if (queue == null) throw new ArgumentNullException("queue");
            if (args[0] == null) throw new ArgumentNullException("source");
            if ((args[1] == null) && (final_out==null)) throw new ArgumentNullException("dest");
            //if (sampler == null) throw new ArgumentNullException("sampler");
            if ((args[1] != null) && ((args[0].Width > args[1].Width) || (args[0].Height > args[1].Height))) throw new ArgumentException("Destination image (" + args[0].Width + "x" + args[0].Height + ") must have at least the same size as the source image (" + args[1].Width + "x" + args[1].Height + ")");

            if (queue.Device.Type == ComputeDeviceTypes.Cpu) crappy_device = true;
            else crappy_device = false;
            //if (crappy_device) System.Windows.Forms.MessageBox.Show("IzCrappy");
            int l_size = 8;
            if (crappy_device)
                l_size = 1;
            long[] globalW = new long[] { args[0].Width - (args[0].Width % l_size), args[0].Height - (args[0].Height % l_size) };
            
            long[] localW = new long[] { 1, 8 };

            for (int i = 0; i < (!crappy_device ? 3 : 1); i++)
            {
                if (i == 0)
                {
                    startFrom[0][0] = 0;
                    startFrom[0][1] = 0;
                }
                if (i == 1)
                {
                    startFrom[1][0] = args[0].Width - (args[0].Width % l_size);
                    startFrom[1][0] = 0;
                    globalW[0] = args[0].Width % l_size;
                }
                else if (i == 2)
                {
                    startFrom[2][0] = 0;
                    startFrom[2][1] = args[0].Height - (args[0].Height % l_size);
                    globalW[0] = args[0].Width;
                    globalW[1] = args[0].Height % l_size;
                }

                if ((globalW[0] == 0) ||
                    (globalW[1] == 0))
                    continue;

                if (startFromBuff == null)
                    startFromBuff = new ClooBuffer<int>(_context, ComputeMemoryFlags.ReadOnly | ComputeMemoryFlags.UseHostPointer, startFrom[i]);

                //long[] localW = new long[] { 1, 1 }; 
                //System.Windows.Forms.MessageBox.Show("Pass" + i.ToString());

                if (final_out==null)
                            KernelSpecialKernel[kernNum].SetArguments(args[0], args[1], startFromBuff, (args.Length > 2) ? args[2] : null);
                        else if (args.Length > 2)
                            KernelSpecialKernel[kernNum].SetArguments(args[0], args[1], startFromBuff, args[2], final_out);
                        else
                            KernelSpecialKernel[kernNum].SetArguments(args[0], args[1], startFromBuff, final_out);
                        queue.Execute(KernelSpecialKernel[kernNum], null, globalW, null, null);
                        
                //queue.Finish();
                //startFromBuff.Dispose();

            }

            args[1].Modified = true; // args[0] is input, args[1] is output, other inputs follow starting with args[2]
            if (final_out!=null) 
                final_out.Modified = true;
        }

        public void SpecialKernel_limited(int startx, int starty, int radius, ClooCommandQueue queue, ClooContext _context, int kernNum, params ClooImage2DFloatRgbA[] args) //ClooImage2DByteRgbA source, ClooImage2DByteRgbA dest)//, ClooSampler sampler)
        {
            if (queue == null) throw new ArgumentNullException("queue");
            if (args[0] == null) throw new ArgumentNullException("source");
            if (args[1] == null) throw new ArgumentNullException("dest");
            //if (sampler == null) throw new ArgumentNullException("sampler");
            if ((args[0].Width > args[1].Width) || (args[0].Height > args[1].Height)) throw new ArgumentException("Destination image (" + args[0].Width + "x" + args[0].Height + ") must have at least the same size as the source image (" + args[1].Width + "x" + args[1].Height + ")");

            int WGs = radius * 2;


            //if (queue.Device.Type == ComputeDeviceTypes.Cpu) crappy_device = true;
            //else crappy_device = false;
            //if (crappy_device) System.Windows.Forms.MessageBox.Show("IzCrappy");
            int l_size = 1;
            //if (crappy_device)
            //    l_size = 1;
            long[] globalW = new long[] { WGs, WGs };
            int[] startFrom = new int[] { startx, starty };

            //for (int i = 0; i < (!crappy_device ? 3 : 1); i++)
            {


                ClooBuffer<int> startFromBuff = new ClooBuffer<int>(_context, ComputeMemoryFlags.ReadOnly | ComputeMemoryFlags.UseHostPointer, startFrom);

                //long[] localW = new long[] { 1, 1 }; 
                //System.Windows.Forms.MessageBox.Show("Pass" + i.ToString());

                KernelSpecialKernel[kernNum].SetArguments(args[0], args[1], startFromBuff, (args.Length > 2) ? args[2] : null);//, sampler);
                queue.Execute(KernelSpecialKernel[kernNum], null, globalW, null, null);
                        
                queue.Finish();
                startFromBuff.Dispose();
            }

            args[1].Modified = true; // args[0] is input, args[1] is output, other inputs follow starting with args[2]

        }
    }
}
