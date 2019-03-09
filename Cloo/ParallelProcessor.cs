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
using System.Threading;
using System.Collections.Generic;

namespace OpenClooVision
{
    /// <summary>
    /// Parallel processor for .NET 3.5
    /// (since the cool stuff was introduced in .NET 4.0)
    /// </summary>
    public class ParallelProcessor
    {
        /// <summary>
        /// Executes all methods in parallel and waits until all are finished
        /// </summary>
        /// <param name="fromIndex">start index</param>
        /// <param name="toIndex">stop index</param>
        /// <param name="action">action to execute</param>
        public static void For(int fromIndex, int toIndex, Action<int> action)
        {
            ManualResetEvent[] resetEvents = new ManualResetEvent[Math.Abs(fromIndex - toIndex)];
            if (resetEvents.Length <= 1)
            {
                // no threading needed
                action.Invoke(fromIndex);
                return;
            }

            List<Exception> exceptions = new List<Exception>();
            int step = 1;
            if (fromIndex > toIndex) step = -1;
            for (int i = fromIndex; (step > 0 ? i < toIndex : i > toIndex); i += step)
            {
                resetEvents[i] = new ManualResetEvent(false);
                ThreadPool.QueueUserWorkItem(new WaitCallback((object index) =>
                {
                    int methodIndex = (int)index;
                    try
                    {
                        action.Invoke(methodIndex);
                    }
                    catch (Exception ex)
                    {
                        lock (exceptions) { exceptions.Add(ex); }
                    }
                    finally
                    {
                        resetEvents[methodIndex].Set();
                    }
                }), i);
            }

            // workaround since WaitAll didn't work properly
            for (int i = 0; i < resetEvents.Length; i++)
                resetEvents[i].WaitOne();

            if (exceptions.Count > 0) throw exceptions[0];
        }

        /// <summary>
        /// Executes all methods in parallel and waits until all are finished
        /// </summary>
        /// <param name="methods">all methods to execute</param>
        public static void Do(params Action[] methods)
        {
            if (methods.Length <= 0) return;
            if (methods.Length == 1)
            {
                // no threading needed
                methods[0].Invoke();
                return;
            }
            ManualResetEvent[] resetEvents = new ManualResetEvent[methods.Length];
            List<Exception> exceptions = new List<Exception>();

            for (int i = 0; i < methods.Length; i++)
            {
                resetEvents[i] = new ManualResetEvent(false);
                ThreadPool.QueueUserWorkItem(new WaitCallback((object index) =>
                {
                    int methodIndex = (int)index;
                    try
                    {
                        methods[methodIndex].Invoke();
                    }
                    catch (Exception ex)
                    {
                        lock (exceptions) { exceptions.Add(ex); }
                    }
                    finally
                    {
                        resetEvents[methodIndex].Set();
                    }
                }), i);
            }

            // workaround since WaitAll didn't work properly
            for (int i = 0; i < resetEvents.Length; i++)
                resetEvents[i].WaitOne();

            if (exceptions.Count > 0) throw exceptions[0];
        }
    }
}
