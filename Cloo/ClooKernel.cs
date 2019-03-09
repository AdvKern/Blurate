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
using Cloo;

namespace OpenClooVision
{
    /// <summary>
    /// Cloo compute kernel
    /// </summary>
    public class ClooKernel : ComputeKernel
    {
        private ClooProgram _program;
        /// <summary>
        /// Gets the associated compute program
        /// </summary>
        public new ClooProgram Program
        {
            get { return _program; }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="functionName">function name</param>
        /// <param name="program">compute program</param>
        public ClooKernel(string functionName, ClooProgram program)
            : base(functionName, program)
        {
            _program = program;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="handle">function handle</param>
        /// <param name="program">compute program</param>
        public ClooKernel(System.IntPtr handle, ClooProgram program)
            : base(handle, program)
        {
            _program = program;
        }

        /// <summary>
        /// Sets all arguments for a kernel
        /// </summary>
        /// <param name="args">arguments</param>
        public void SetArguments(params object[] args)
        {
            for (int i = 0; i < args.Length; i++)
            {
                try
                {
                    object arg = args[i];
                    if (arg == null) {
                        if (i == args.Length - 1) continue; // Windows doesn't need this but Xamarin does
                        SetMemoryArgument(i, null);
                        continue;
                    }
                    var typeCode = Type.GetTypeCode(arg.GetType());
                    switch (typeCode)
                    {
                        //case TypeCode.Boolean:
                        //    break;
                        case TypeCode.Byte:
                            SetValueArgument<byte>(i, (byte)arg);
                            break;
                        case TypeCode.Char:
                            SetValueArgument<char>(i, (char)arg);
                            break;
                        case TypeCode.Decimal:
                            SetValueArgument<decimal>(i, (decimal)arg);
                            break;
                        case TypeCode.Double:
                            SetValueArgument<double>(i, (double)arg);
                            break;
                        case TypeCode.Empty:
                            SetMemoryArgument(i, null);
                            break;
                        case TypeCode.Int16:
                            SetValueArgument<short>(i, (short)arg);
                            break;
                        case TypeCode.Int32:
                            SetValueArgument<int>(i, (int)arg);
                            break;
                        case TypeCode.Int64:
                            SetValueArgument<long>(i, (long)arg);
                            break;
                        case TypeCode.Object:
                            {
                                ComputeSampler sampler = arg as ComputeSampler;
                                if (sampler != null) { SetSamplerArgument(i, sampler); continue; }
                            }
                            {
                                ComputeMemory mem = arg as ComputeMemory;
                                if (mem != null) { SetMemoryArgument(i, mem); continue; }
                            }
                            {
                                LocalSize localSize = arg as LocalSize;
                                if (localSize != null) { SetLocalArgument(i, localSize.DataSize); continue; }
                            }
                            goto default;
                        case TypeCode.SByte:
                            SetValueArgument<sbyte>(i, (sbyte)arg);
                            break;
                        case TypeCode.Single:
                            SetValueArgument<float>(i, (float)arg);
                            break;
                        case TypeCode.UInt16:
                            SetValueArgument<ushort>(i, (ushort)arg);
                            break;
                        case TypeCode.UInt32:
                            SetValueArgument<uint>(i, (uint)arg);
                            break;
                        case TypeCode.UInt64:
                            SetValueArgument<ulong>(i, (ulong)arg);
                            break;
                        default:
                            throw new NotSupportedException("Type " + arg.GetType().ToString() + " is not supported as argument");
                    }
                }
                catch (InvalidKernelArgumentsComputeException)
                {
                    throw new ArgumentException(String.Format("Invalid kernel parameter specified on index {0} for function {1}", i, FunctionName));
                }
            }
        }
    }
}
