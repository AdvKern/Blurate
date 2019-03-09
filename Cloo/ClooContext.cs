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
using System.Linq;
using Cloo;

namespace OpenClooVision
{
    /// <summary>
    /// Cloo compute context
    /// </summary>
    public class ClooContext : ComputeContext
    {
        private List<ClooDevice> _devices = new List<ClooDevice>();
        /// <summary>
        /// Gets a list of devices associated to this compute context
        /// </summary>
        public new IEnumerable<ClooDevice> Devices
        {
            get { return _devices; }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="deviceType">device type</param>
        /// <param name="properties">context properties</param>
        /// <param name="notify">context notify</param>
        /// <param name="userDataPtr">user data pointer</param>
        public ClooContext(ComputeDeviceTypes deviceType, ComputeContextPropertyList properties, ComputeContextNotifier notify, System.IntPtr userDataPtr)
            : base(deviceType, properties, notify, userDataPtr)
        {
            foreach (ComputeDevice device in base.Devices)
                _devices.Add(ClooDevice.FromBaseDevice(device));
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="devices">list of devices</param>
        /// <param name="properties">context properties</param>
        /// <param name="notify">context notify</param>
        /// <param name="userDataPtr">user data pointer</param>
        public ClooContext(IEnumerable<ClooDevice> devices, ComputeContextPropertyList properties, ComputeContextNotifier notify, System.IntPtr userDataPtr)
            : base(devices.Cast<ComputeDevice>().ToList(), properties, notify, userDataPtr)
        {
            _devices.AddRange(devices);
        }

        /// <summary>
        /// Creates a compute context from device
        /// </summary>
        /// <param name="device">compute device</param>
        /// <returns>compute context</returns>
        /// <exception cref="ArgumentNulLException">device</exception>
        public static ClooContext FromDevice(ClooDevice device)
        {
            if (device == null) throw new ArgumentNullException("device");
            return device.CreateContext();
        }

        /// <summary>
        /// Creates a command queue using this context and the first associated devices
        /// </summary>
        /// <param name="flags">command queue flags (default = ComputeCommandQueueFlags.None)</param>
        /// <returns>new instance of ClooCommandQueue</returns>
        /// <exception cref="InvalidOperationException">This context has no devices, cannot create ClooCommandQueue</exception>
        public ClooCommandQueue CreateCommandQueue(ComputeCommandQueueFlags flags = ComputeCommandQueueFlags.None)
        {
            if (_devices.Count <= 0) throw new InvalidOperationException("This context has no devices, cannot create ClooCommandQueue");
            return new ClooCommandQueue(this, _devices[0], flags);
        }
    }
}
