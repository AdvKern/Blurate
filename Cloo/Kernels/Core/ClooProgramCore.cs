using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Cloo;

namespace OpenClooVision.Kernels.Core
{
    /// <summary>
    /// Cloo program for core kernels
    /// </summary>
    [CLSCompliant(false)]
    public class ClooProgramCore : ClooProgram
    {
        /// <summary>
        /// Sets a constant value to all cells in a buffer
        /// </summary>
        protected ClooKernel KernelCoreByteSetValue { get; set; }

        /// <summary>
        /// Sets a constant value to all cells in a buffer
        /// </summary>
        protected ClooKernel KernelCoreFloatSetValue { get; set; }

        /// <summary>
        /// Sets a constant value to all cells in a buffer
        /// </summary>
        protected ClooKernel KernelCoreUIntSetValue { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="context">compute context</param>
        /// <param name="lines">lines of source code</param>
        protected ClooProgramCore(ClooContext context, string[] lines) :
            base(context, lines)
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="context">compute context</param>
        /// <param name="lines">lines of source code</param>
        protected ClooProgramCore(ClooContext context, IList<byte[]> binaries, IEnumerable<ClooDevice> devices) :
            base(context, binaries, devices)
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="context">compute context</param>
        /// <param name="useBinaryCache">use cache for binaries</param>
        public static ClooProgramCore Create(ClooContext context, bool useBinaryCache = false)
        {
            var dict = GetEmbeddedSourceLines(typeof(ClooProgramCore));
            string header = null;
            if (dict.TryGetValue("Header", out header)) dict.Remove("Header"); else header = "";
            List<string> lines = dict.Values.ToList();
            string source = header + "\r\n" + String.Join("\r\n\r\n", lines.ToArray());

            ClooProgramCore res = null;
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
                    res = new ClooProgramCore(context, binaries, context.Devices);
                }
            }

#if DEBUG
            try
            {
#endif
                if (res == null)
                {
                    // ok not in cache, so rebuilt from source
                    res = new ClooProgramCore(context, lines.ToArray()); 
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
            res.InitializeKernels(typeof(ClooProgramCore));
            return res;
        }

        /// <summary>
        /// Sets a value of all cells in a buffer
        /// </summary>
        /// <param name="queue">cloo command queue</param>
        /// <param name="source">image source</param>
        /// <param name="value">value to set</param>
        public void SetValue(ClooCommandQueue queue, ClooBuffer<byte> source, byte value)
        {
            if (queue == null) throw new ArgumentNullException("queue");
            if (source == null) throw new ArgumentNullException("source");

            KernelCoreByteSetValue.SetArguments(source, value);
            queue.Execute(KernelCoreByteSetValue, null, new long[] { source.HostBuffer.Length }, null, null);
            source.Modified = true;
        }

        /// <summary>
        /// Sets a value of all cells in a buffer
        /// </summary>
        /// <param name="queue">cloo command queue</param>
        /// <param name="source">image source</param>
        /// <param name="value">value to set</param>
        public void SetValue(ClooCommandQueue queue, ClooBuffer<float> source, float value)
        {
            if (queue == null) throw new ArgumentNullException("queue");
            if (source == null) throw new ArgumentNullException("source");

            KernelCoreFloatSetValue.SetArguments(source, value);
            queue.Execute(KernelCoreFloatSetValue, null, new long[] { source.HostBuffer.Length }, null, null);
            source.Modified = true;
        }

        /// <summary>
        /// Sets a value of all cells in a buffer
        /// </summary>
        /// <param name="queue">cloo command queue</param>
        /// <param name="source">image source</param>
        /// <param name="value">value to set</param>
        public void SetValue(ClooCommandQueue queue, ClooBuffer<uint> source, uint value)
        {
            if (queue == null) throw new ArgumentNullException("queue");
            if (source == null) throw new ArgumentNullException("source");

            KernelCoreUIntSetValue.SetArguments(source, value);
            queue.Execute(KernelCoreUIntSetValue, null, new long[] { source.HostBuffer.Length }, null, null);
            source.Modified = true;
        }
    }
}
