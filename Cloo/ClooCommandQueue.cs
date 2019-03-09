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

using Cloo;

namespace OpenClooVision
{
    /// <summary>
    /// Cloo compute command queue
    /// </summary>
    public class ClooCommandQueue : ComputeCommandQueue
    {
        private ClooContext _context;
        /// <summary>
        /// Gets the associated compute context
        /// </summary>
        public new ClooContext Context
        {
            get { return _context; }
        }

        private ClooDevice _device;
        /// <summary>
        /// Gets the associated compute device
        /// </summary>
        public new ClooDevice Device
        {
            get { return _device; }
        }

        private ComputeCommandQueueFlags _flags;
        /// <summary>
        /// Compute command queue flags
        /// </summary>
        public ComputeCommandQueueFlags Flags
        {
            get { return _flags; }
        }

        /// <summary>
        /// Cloo compute command queue
        /// </summary>
        /// <param name="context">command context</param>
        /// <param name="device">command device</param>
        /// <param name="flags">command queue flags</param>
        public ClooCommandQueue(ClooContext context, ClooDevice device, ComputeCommandQueueFlags flags)
            : base(context, device, flags)
        {
            _context = context;
            _device = device;
            _flags = flags;
        }
    }
}
