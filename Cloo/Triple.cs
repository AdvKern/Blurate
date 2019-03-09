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

namespace OpenClooVision
{
    /// <summary>
    /// Tuple implementation for three items
    /// </summary>
    /// <typeparam name="T1">type of first item</typeparam>
    /// <typeparam name="T2">type of second item</typeparam>
    /// <typeparam name="T3">type of third item</typeparam>
    public struct Triple<T1, T2, T3> : IEquatable<Triple<T1, T2, T3>>
    {
        private readonly T1 _item1;
        /// <summary>
        /// First item
        /// </summary>
        public T1 Item1
        {
            get { return _item1; }
        }

        private readonly T2 _item2;
        /// <summary>
        /// Second item
        /// </summary>
        public T2 Item2
        {
            get { return _item2; }
        }

        private readonly T3 _item3;
        /// <summary>
        /// Third value
        /// </summary>
        public T3 Item3
        {
            get { return _item3; }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="item1">First item</param>
        /// <param name="item2">Second item</param>
        /// <param name="item3">Third item</param>
        public Triple(T1 item1, T2 item2, T3 item3)
        {
            _item1 = item1;
            _item2 = item2;
            _item3 = item3;
        }

        /// <summary>
        /// Check if tuple equals other tuple
        /// </summary>
        /// <param name="other">other tuple</param>
        /// <returns></returns>
        public bool Equals(Triple<T1, T2, T3> other)
        {
            return _item1.Equals(other._item1) &&
                    _item2.Equals(other._item2);
        }

        /// <summary>
        /// Check if tuple equals other object
        /// </summary>
        /// <param name="obj">other object</param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            if (obj is Triple<T1, T2, T3>)
                return this.Equals((Triple<T1, T2, T3>)obj);
            else
                return false;
        }

        /// <summary>
        /// Get hash code of tuple
        /// </summary>
        /// <returns>hash code</returns>
        public override int GetHashCode()
        {
            return _item1.GetHashCode() ^ _item2.GetHashCode() ^ _item3.GetHashCode();
        }
    }

}
