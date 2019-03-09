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

using Cloo;

namespace OpenClooVision.Imaging
{
    /// <summary>
    /// Cloo compute sampler
    /// </summary>
    public class ClooSampler : ComputeSampler
    {
        private ClooContext _context;
        /// <summary>
        /// Gets the associated compute context
        /// </summary>
        public new ClooContext Context
        {
            get { return _context; }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="context">Cloo compute context</param>
        /// <param name="normalizedCoords">are coordinates normalized? (between 0 and 1)</param>
        /// <param name="addressing">image addressing mode</param>
        /// <param name="filtering">image filtering mode</param>
        public ClooSampler(ClooContext context, bool normalizedCoords, ComputeImageAddressing addressing, ComputeImageFiltering filtering)
            : base(context, normalizedCoords, addressing, filtering)
        {
            _context = context;
        }
    }
}
