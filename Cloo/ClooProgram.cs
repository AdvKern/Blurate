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
using System.IO;
using System.Linq;
using System.Reflection;
using Cloo;

namespace OpenClooVision
{
    /// <summary>
    /// Constructor
    /// </summary>
    public class ClooProgram : ComputeProgram
    {
        private static Dictionary<int, IList<byte[]>> _binariesCache = new Dictionary<int, IList<byte[]>>();
        /// <summary>
        /// Binaries cache to reuse programs already built
        /// </summary>
        public static Dictionary<int, IList<byte[]>> BinariesCache
        {
            get { return ClooProgram._binariesCache; }
        }

        private ClooContext _context;
        /// <summary>
        /// Gets the associated compute context
        /// </summary>
        public new ClooContext Context
        {
            get { return _context; }
        }

        private List<ClooDevice> _devices = new List<ClooDevice>();
        /// <summary>
        /// Gets a list of devices associated to this compute program
        /// </summary>
        public new IEnumerable<ClooDevice> Devices
        {
            get { return _devices; }
        }

        /// <summary>
        /// Lines of OpenCL source code for each function
        /// </summary>
        public SortedDictionary<string, string> Functions
        {
            get
            {
                return GetEmbeddedSourceLines(GetType());
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="context">compute context</param>
        /// <param name="source">OpenCL source code</param>
        public ClooProgram(ClooContext context, string source)
            : base(context, source)
        {
            _context = context;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="context">compute context</param>
        /// <param name="source">OpenCL source code</param>
        public ClooProgram(ClooContext context, string[] source)
            : base(context, source)
        {
            _context = context;
            _devices.AddRange(context.Devices);
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="context">compute context</param>
        /// <param name="binaries">program binaries</param>
        /// <param name="devices">devices</param>
        public ClooProgram(ClooContext context, IList<byte[]> binaries, IEnumerable<ClooDevice> devices)
            : base(context, binaries, devices.Cast<ComputeDevice>().ToList())
        {
            _context = context;
            _devices.AddRange(devices);
        }

        /// <summary>
        /// Build program
        /// </summary>
        /// <param name="devices">list of devices to compile to</param>
        /// <param name="options">compile options</param>
        /// <param name="addToBinaryCache">put built binaries into cache to reuse already compiled programs later</param>
        public void Build(IEnumerable<ClooDevice> devices, string options, bool addToBinaryCache = false)
        {
            List<ComputeDevice> listDevices = new List<ComputeDevice>();

            int hashCode = 0;
            if (addToBinaryCache)
            {
                // check if we already have binaries
                hashCode = Source.GetArrayHashCode() ^ (options != null ? options.GetHashCode() : 0);
                foreach (ClooDevice device in devices)
                {
                    hashCode ^= device.GetHashCode();
                    listDevices.Add(device);
                }
            }
            else
            {
                // build a list of ClooDevices only
                foreach (ClooDevice device in devices)
                    listDevices.Add(device);
            }

            base.Build(listDevices, options, null, IntPtr.Zero);

            if (addToBinaryCache)
                _binariesCache[hashCode] = base.Binaries;
        }

        /// <summary>
        /// Build program
        /// </summary>
        /// <param name="devices">list of devices to compile to</param>
        /// <param name="options">compile options</param>
        /// <param name="notify">build notifier</param>
        /// <param name="notifyDataPtr">build notifier data pointer</param>
        /// <param name="addToBinaryCache">put built binaries into cache to reuse already compiled programs later</param>
        public void Build(IEnumerable<ClooDevice> devices, string options, ComputeProgramBuildNotifier notify, 
            IntPtr notifyDataPtr, bool addToBinaryCache = false)
        {
            List<ComputeDevice> listDevices = new List<ComputeDevice>();

            int hashCode = 0;
            if (addToBinaryCache)
            {
                // check if we already have binaries
                hashCode = Source.GetArrayHashCode() ^ (options != null ? options.GetHashCode() : 0);
                foreach (ClooDevice device in devices)
                {
                    hashCode ^= device.GetHashCode();
                    listDevices.Add(device);
                }
            }
            else
            {
                // build a list of ClooDevices only
                foreach (ClooDevice device in devices)
                    listDevices.Add(device);
            }

            base.Build(devices.Cast<ComputeDevice>().ToList(), options, notify, notifyDataPtr);

            if (addToBinaryCache)
                _binariesCache[hashCode] = base.Binaries;
        }

        /// <summary>
        /// Initialize kernels
        /// </summary>
        /// <param name="type">type to initialize</param>
        public virtual void InitializeKernels(Type type)
        {
            if (type == null || type == typeof(ClooProgram)) return;

            // create cloo kernels for each ClooKernel property
            foreach (var pi in type.GetProperties(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetProperty))
            {
                if (pi.PropertyType != typeof(ClooKernel)) continue;
                string name = pi.Name;
                if (!name.StartsWith("Kernel")) continue;
                if (name.StartsWith("KernelSpecialKernel")) continue;
                name = name.Remove(0, 6);
                pi.SetValue(this, new ClooKernel(name, this), null);
            }
        }

        /// <summary>
        /// Initialize Special kernel
        /// </summary>
        /// <param name="type">type to initialize</param>
        public virtual void InitializeKernelSpecial(Type type, int numKerns)
        {
            if (type == null || type == typeof(ClooProgram)) return;

            // create cloo kernels for each ClooKernel property
            int kernCntr = 0;
            foreach (var pi in type.GetProperties(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetProperty))
            {
                if (pi.PropertyType != typeof(List<ClooKernel>)) continue;
                string name = pi.Name;
                if (!name.StartsWith("KernelSpecialKernel")) continue;
                while (true)
                {
                    name = "s" + pi.Name.Remove(0, 7) + kernCntr;
                    //this.Source = plainText;

                    if (kernCntr == 0) pi.SetValue(this, new List<ClooKernel>(), null);
                    List<ClooKernel> funcList = (List<ClooKernel>)pi.GetValue(this);
                    funcList.Add(new ClooKernel(name, this));
                    kernCntr++;
                    if (kernCntr >= numKerns) break;
                }
            }
        }



        /// <summary>
        /// Gets all OpenCL sources from embedded resources of a specific namespace
        /// </summary>
        /// <param name="type">type to extract the namespace prefix from</param>
        /// <returns></returns>
        protected static SortedDictionary<string, string> GetEmbeddedSourceLines(Type type)
        {
            SortedDictionary<string, string> lines = new SortedDictionary<string, string>();
            Assembly assembly = Assembly.GetExecutingAssembly();

            while (type != null)
            {
                foreach (string name in assembly.GetManifestResourceNames())
                {
                    if (name.StartsWith(type.Namespace + ".") && name.EndsWith(".cl", StringComparison.InvariantCultureIgnoreCase))
                    {
                        string functionName = name.Substring(type.Namespace.Length + 1, name.Length - type.Namespace.Length - 4);
                        using (StreamReader reader = new StreamReader(assembly.GetManifestResourceStream(name)))
                        {
                            if (functionName == "Header")
                            {
                                // append headers
                                string header = null;
                                if (lines.TryGetValue(functionName, out header))
                                {
                                    header += reader.ReadToEnd() + "\r\n";
                                    lines[functionName] = header;
                                    continue;
                                }
                            }

                            lines[functionName] = reader.ReadToEnd();
                        }
                    }
                }
                type = type.BaseType;
                if (type == typeof(ClooProgram)) break;
            }
            return lines;
        }
    }
}
