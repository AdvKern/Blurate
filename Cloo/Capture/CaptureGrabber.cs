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
using System.Runtime.InteropServices;
//using System.Drawing.Imaging;
using Android.Graphics;

namespace OpenClooVision.Capture
{
    /// <summary>
    /// Interface implementation of DirectShow's ISampleGrabberCB
    /// </summary>
    internal class CaptureGrabber : ISampleGrabberCB
    {
        private byte[] _bitmapBuffer = null;
        /// <summary>
        /// Managed bitmap buffer in BGR
        /// </summary>
        public byte[] BitmapBuffer
        {
            get { return _bitmapBuffer; }
        }

        private int _height;
        /// <summary>
        /// Image height
        /// </summary>
        public int Height { get { return _height; } set { _height = value; ChangeBitmapSize(); } }

        private int _width;
        /// <summary>
        /// Image width
        /// </summary>
        public int Width { get { return _width; } set { _width = value; ChangeBitmapSize(); } }

        /// <summary>
        /// Pixel size in bytes
        /// </summary>
        public int PixelSize { get; protected set; }

        /// <summary>
        /// Image pixel format
        /// </summary>
        public PixelFormat PixelFormat { get; protected set; }

        /// <summary>
        /// Last sample time
        /// </summary>
        public double SampleTime { get; protected set; }

        /// <summary>
        /// Unmanaged pointer of image data
        /// </summary>
        public IntPtr SharedMemMap { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="width">image width</param>
        /// <param name="height">image height</param>
        /// <param name="pixelFormat">image pixel format (Format24bppRgb or Format32bppRgb supported only!)</param>
        public CaptureGrabber(int width, int height, PixelFormat pixelFormat)
        {
            _width = width;
            _height = height;
            PixelFormat = pixelFormat;
            ChangeBitmapSize();

            /*switch (pixelFormat)
            {
                case PixelFormat.Format24bppRgb:
                    PixelSize = 3;
                    break;
                case PixelFormat.Format32bppRgb:
                    PixelSize = 4;
                    break;
                default:
                    throw new NotSupportedException("PixelFormat other than Format24bppRgb or Format32bppRgb are not supported!");
            }*/
        }

        /// <summary>
        /// Changes the bitmap size
        /// </summary>
        private void ChangeBitmapSize()
        {
            if (_width > 0 && _height > 0)
            {
                _bitmapBuffer = new byte[_width * _height * PixelSize];
            }
        }

        /// <summary>
        /// Event that is fired on every new frame
        /// </summary>
        public event EventHandler NewFrame;

        /// <summary>
        /// Fire event on every new frame
        /// </summary>
        protected virtual void OnNewFrame()
        {
            if (NewFrame != null) NewFrame(this, EventArgs.Empty);
        }

        /// <summary>
        /// Callback method that receives a pointer to the media sample.
        /// </summary>
        /// <param name="sampleTime">sample time</param>
        /// <param name="sample">pointer of sample</param>
        /// <returns>HRESULT</returns>
        public int SampleCB(double sampleTime, IntPtr sample)
        {
            // not used
            return 0;
        }

        /// <summary>
        /// Callback method that receives a pointer to the sample buffer.
        /// </summary>
        /// <param name="sampleTime">sample time</param>
        /// <param name="buffer">pointer to buffer</param>
        /// <param name="bufferLen">buffer length</param>
        /// <returns>HRESULT</returns>
        public int BufferCB(double sampleTime, IntPtr buffer, int bufferLen)
        {
            if (SharedMemMap != IntPtr.Zero)
            {
                RtlMoveMemory(SharedMemMap, buffer, bufferLen);
                lock (_bitmapBuffer) { Marshal.Copy(buffer, _bitmapBuffer, 0, bufferLen); }

                SampleTime = sampleTime;

                // fire event
                OnNewFrame();
            }
            return 0;
        }

        /// <summary>
        /// Copy memory from pointer to pointer
        /// </summary>
        /// <param name="destPtr">destination pointer</param>
        /// <param name="srcPtr">source pointer</param>
        /// <param name="length">number of bytes to copy</param>
        [DllImport("Kernel32.dll", EntryPoint = "RtlMoveMemory")]
        private static extern void RtlMoveMemory(IntPtr destPtr, IntPtr srcPtr, int length);
    }
}
