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
using Cloo;

namespace OpenClooVision
{
    /// <summary>
    /// Cloo compute device
    /// </summary>
    public class ClooDevice : ComputeDevice
    {
        private static object _lockUpdateCompatibleDevices = new object();
        private static Dictionary<IntPtr, ClooDevice> _devicesByHandle = null;
        private static List<ClooDevice> _allDevices;
        /// <summary>
        /// Gets all compatible devices
        /// </summary>
        public static IEnumerable<ClooDevice> CompatibleDevices
        {
            get
            {
                return _allDevices;
            }
        }

        /// <summary>
        /// Static constructor
        /// </summary>
        static ClooDevice()
        {
            UpdateCompatibleDevices();
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="platform">compute platform</param>
        /// <param name="handle">device handle</param>
        public ClooDevice(ComputePlatform platform, System.IntPtr handle)
            : base(platform, handle)
        {
        }

        /// <summary>
        /// Creates a compute context for this device only
        /// </summary>
        /// <returns>compute context</returns>
        public ClooContext CreateContext(params ComputeContextProperty[] properties)
        {
            List<ComputeContextProperty> list = new List<ComputeContextProperty>();
            bool hasPlatform = false;
            if (properties != null)
            {
                foreach (var property in properties)
                {
                    if (property.Name == ComputeContextPropertyName.Platform) hasPlatform = true;
                    list.Add(property);
                }
            }
            if (!hasPlatform)
                list.Add(new ComputeContextProperty(ComputeContextPropertyName.Platform, Platform.Handle));
            return new ClooContext(Type, new ComputeContextPropertyList(list), null, IntPtr.Zero);
        }

        /// <summary>
        /// Creates a cloo compute device from base type
        /// </summary>
        /// <param name="baseDevice">base compute device</param>
        /// <returns>cloo compute device</returns>
        /// <exception cref="ArgumentNullException">baseDevice</exception>
        /// <exception cref="ArgumentException">Compute device not found, maybe you need to call ClooDevice.UpdateAllDevices()</exception>
        public static ClooDevice FromBaseDevice(ComputeDevice baseDevice)
        {
            if (baseDevice == null) throw new ArgumentNullException("baseDevice");
            ClooDevice clooDevice;
            if (_devicesByHandle.TryGetValue(baseDevice.Handle, out clooDevice)) return clooDevice;
            throw new ArgumentException("Compute device not found, maybe you need to call ClooDevice.UpdateAllDevices()", "baseDevice");
        }

        /// <summary>
        /// Hash code of device for dictionaries, etc.
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return Platform.Name.GetHashCode() ^ Name.GetHashCode() ^ DriverVersion.GetHashCode();
        }

        /// <summary>
        /// Overwritten ToString method
        /// </summary>
        /// <returns>compute device name</returns>
        public override string ToString()
        {
            return base.Name;
        }

        /// <summary>
        /// Updates all devices list
        /// </summary>
        public static void UpdateCompatibleDevices()
        {
            lock (_lockUpdateCompatibleDevices)
            {
                _devicesByHandle = new Dictionary<IntPtr, ClooDevice>();
                _allDevices = new List<ClooDevice>();
                try
                {
                    foreach (ComputePlatform platform in ComputePlatform.Platforms)
                    {
                        foreach (ComputeDevice device in platform.Devices)
                        {
                            if (_devicesByHandle.ContainsKey(device.Handle)) continue; // device already added

                            if (!device.CompilerAvailable) continue;
                            if (!device.ImageSupport) continue;

                            ClooDevice clooDevice = new ClooDevice(platform, device.Handle);
                            _devicesByHandle[device.Handle] = clooDevice;
                            _allDevices.Add(clooDevice);
                        }
                    }
                }
                catch { }
            }
        }
    }
}
