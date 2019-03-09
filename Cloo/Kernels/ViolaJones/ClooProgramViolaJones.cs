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
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using Cloo;
using OpenClooVision.Imaging;
using OpenClooVision.Kernels.Imaging;

namespace OpenClooVision.Kernels.ViolaJones
{
    /// <summary>
    /// Cloo program for Viola & Jones kernels
    /// </summary>
    [CLSCompliant(false)]
    public class ClooProgramViolaJones : ClooProgramImaging
    {
        /// <summary>
        /// Clears the result rectangles
        /// </summary>
        protected ClooKernel KernelImageViolaJonesClear { get; set; }

        /// <summary>
        /// Process object detection on a frame on GPU
        /// </summary>
        protected ClooKernel KernelImageViolaJonesProcess { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="context">compute context</param>
        /// <param name="lines">lines of source code</param>
        protected ClooProgramViolaJones(ClooContext context, string[] lines) :
            base(context, lines)
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="context">compute context</param>
        /// <param name="lines">lines of source code</param>
        protected ClooProgramViolaJones(ClooContext context, IList<byte[]> binaries, IEnumerable<ClooDevice> devices) :
            base(context, binaries, devices)
        {
        }

        public void ClearViolaJonesRectangles(ClooCommandQueue queue, ClooBuffer<Rectangle> resRectangles)
        {
            if (queue == null) throw new ArgumentNullException("queue");
            if (resRectangles == null) throw new ArgumentNullException("resRectangles");

            KernelImageViolaJonesClear.SetArguments(resRectangles);
            queue.Execute(KernelImageViolaJonesClear, null, new long[] { resRectangles.HostBuffer.Length }, null, null);
        }

        public void ProcessViolaJonesFrame(ClooCommandQueue queue, ClooBuffer<HaarFeatureNode> nodes,
            int stagesCount, IBuffer<int> stageNodeCounts, IBuffer<float> stageThresholds,
            ClooImage2DUIntA integral, ClooImage2DUIntA integralSquare,
            IBuffer<Rectangle> resRectangles,
            float scale, int countX, int countY,
            int stepX, int stepY, int windowWidth, int windowHeight)
        {
            if (queue == null) throw new ArgumentNullException("queue");
            if (nodes == null) throw new ArgumentNullException("nodes");
            if (integral == null) throw new ArgumentNullException("integral");
            if (integralSquare == null) throw new ArgumentNullException("integralSquare");
            if (resRectangles == null) throw new ArgumentNullException("resRectangles");

            KernelImageViolaJonesProcess.SetArguments(nodes, stagesCount, stageNodeCounts, stageThresholds,
                integral, integralSquare, resRectangles, resRectangles.HostBuffer.Length, scale, stepX, stepY,
                windowWidth, windowHeight);
            queue.Execute(KernelImageViolaJonesProcess, null, new long[] { countX, countY }, null, null);
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="context">compute context</param>
        /// <param name="useBinaryCache">use cache for binaries</param>
        public static new ClooProgramViolaJones Create(ClooContext context, bool useBinaryCache = false)
        {
            var dict = GetEmbeddedSourceLines(typeof(ClooProgramViolaJones));
            string header = null;
            if (dict.TryGetValue("Header", out header)) dict.Remove("Header"); else header = "";
            List<string> lines = dict.Values.ToList();
            string source = header + "\r\n" + String.Join("\r\n\r\n", lines.ToArray());

            ClooProgramViolaJones res = null;
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
                    res = new ClooProgramViolaJones(context, binaries, context.Devices);
                }
            }

#if DEBUG
            try
            {
#endif
                if (res == null)
                {
                    // ok not in cache, so rebuilt from source
                    res = new ClooProgramViolaJones(context, new string[] { source });
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
            res.InitializeKernels(typeof(ClooProgramViolaJones));
            return res;
        }

        public static new ClooProgramViolaJones CreateSpecial(ClooContext context, List<string> kernList, bool useBinaryCache = false)
        {
            var dict = new Dictionary<string, string>();// GetEmbeddedSourceLines(typeof(ClooProgramViolaJones));
            ClooProgramViolaJones res = null;
            string source = "";
            for (int i = 0; i < kernList.Count; i++)
            {
                dict["specialKernel" + i] = kernList[i];
                string header = null;
                if (dict.TryGetValue("Header", out header)) dict.Remove("Header"); else header = "";
                List<string> lines = dict.Values.ToList();
                source += kernList[i];
            }

#if DEBUG
            try
            {
#endif
            if (res == null)
            {
                // ok not in cache, so rebuilt from source
                res = new ClooProgramViolaJones(context, new string[] { source });
                res.Build(context.Devices, "-cl-fast-relaxed-math -cl-no-signed-zeros -cl-mad-enable -cl-finite-math-only -cl-strict-aliasing -cl-unsafe-math-optimizations ", useBinaryCache);
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
                    string fullError = "";
                    while (line != null)
                    {
                        sb.AppendLine(line);
                        line = reader.ReadLine();
                        fullError += line+"\n";
                    }
                    
                    sb.AppendLine("");
                    sb.AppendLine(exception.Message+ fullError);
                }
                string errorMsg = sb.ToString();
                throw new ApplicationException(errorMsg);
            }
#endif

            res.InitializeKernelSpecial(typeof(ClooProgramViolaJones), kernList.Count);
            return res;
        }
    }
}
