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
using Cloo;
using OpenClooVision.Imaging;
using Android.Graphics;

namespace OpenClooVision
{
    /// <summary>
    /// Cloo computer buffer
    /// </summary>
    [CLSCompliant(false)]
    public class ClooBuffer<T> : ComputeBuffer<T>, IBuffer<T> where T : struct
    {
        private object _lockRead = new object();
        private object _lockWrite = new object();

        private ClooContext _context;
        /// <summary>
        /// Gets the associated compute context
        /// </summary>
        public new ClooContext Context
        {
            get { return _context; }
        }

        /// <summary>
        /// Host buffer
        /// </summary>
        protected T[] _hostBuffer = null;
        //public byte[] _hostBufferByte = null;
        /// <summary>
        /// Host buffer
        /// </summary>
        public T[] HostBuffer
        {
            get { return _hostBuffer; }
            set { _hostBuffer = value; }
        }

        protected bool _modified = false;
        /// <summary>
        /// Get or set state if host buffer has been modified since last operation
        /// </summary>
        public bool Modified
        {
            get { return _modified; }
            set { _modified = value; }
        }

       /// <summary>
       /// Constructor
       /// </summary>
       /// <param name="context">compute context</param>
       /// <param name="flags">memory flags</param>
       /// <param name="data">buffer data</param>
        public ClooBuffer(ClooContext context, ComputeMemoryFlags flags, long count, IntPtr dataPtr)
            : base(context, flags, count, dataPtr)
        {
            _context = context;
            //_hostBuffer = data;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="context">compute context</param>
        /// <param name="flags">memory flags</param>
        /// <param name="data">buffer data</param>
        public ClooBuffer(ClooContext context, ComputeMemoryFlags flags, T[] data)
            : base(context, flags, data)
        {
            _context = context;
            _hostBuffer = data;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="context">compute context</param>
        /// <param name="flags">memory flags</param>
        /// <param name="count">buffer size</param>
        public ClooBuffer(ClooContext context, ComputeMemoryFlags flags, long count)
            : base(context, flags, count)
        {
            _context = context;
            _hostBuffer = new T[count];
        }

        /// <summary>
        /// Reads a buffer from device
        /// </summary>
        /// <param name="buffer">compute buffer</param>
        /// <param name="queue">command queue</param>
        /// <param name="buffer">buffer to read into</param>
        /// <param name="count">item count to read</param>
        public void ReadFromDevice(ClooCommandQueue queue)
        {
            lock (_lockRead)
            {
                GCHandle handle = GCHandle.Alloc(_hostBuffer, GCHandleType.Pinned);
                try { queue.Read(this, true, 0, Count, handle.AddrOfPinnedObject(), null); }
                finally { handle.Free(); }
            }
        }

        /// <summary>
        /// Write a buffer to device
        /// </summary>
        /// <param name="image">compute image</param>
        /// <param name="queue">command queue</param>
        public void WriteToDevice(ClooCommandQueue queue)
        {
            WriteToDevice(queue, _hostBuffer);
        }

        /// <summary>
        /// Writes a buffer from device
        /// </summary>
        /// <param name="image">compute image</param>
        /// <param name="queue">command queue</param>
        /// <param name="buffer">buffer to read into</param>
        public void WriteToDevice(ClooCommandQueue queue, T[] buf)
        {
            if (_hostBuffer.Length != buf.Length) throw new ArgumentException("Write buffer size (" + buf.Length + ") does not match the original size (" + _hostBuffer.Length + ")");

            lock (_lockWrite)
            {
                GCHandle handle = GCHandle.Alloc(buf, GCHandleType.Pinned);
                try { queue.Write(this, true, 0, buf.Length, handle.AddrOfPinnedObject(), null); }
                finally { handle.Free(); }
                _hostBuffer = buf;

                _modified = false;
            }
        }

        /// <summary>
        /// Converts host buffer to existing managed bitmap (make sure size matches!)
        /// </summary>
        /// <param name="queue">compute command queue</param>
        /// <param name="bitmap">we have already a bitmap to put it in</param>
        /// <returns>managed bitmap</returns>
        /// <exception cref="ArgumentNullException">queue</exception>
        public unsafe Bitmap ToBitmap(ClooCommandQueue queue, Bitmap bitmap)

        {
            if (queue == null) throw new ArgumentNullException("queue");

            //BitmapData bitmapData = null;

            if (false)//typeof(T) == typeof(byte))
            {
                var byteBuffer = Java.Nio.ByteBuffer.AllocateDirect(bitmap.ByteCount);
                GCHandle handle = GCHandle.Alloc(_hostBuffer, GCHandleType.Pinned);
                //Marshal.Copy((byte[])handle.AddrOfPinnedObject(), byteBuffer.GetDirectBufferAddress(), 0, bitmap.ByteCount);
                bitmap.CopyPixelsFromBuffer(byteBuffer);
                byteBuffer.Clear();
                byteBuffer.Dispose();
                return bitmap;
            }
            return bitmap;
            /*else
            {
                if (_hostBufferByte == null)
                {


                    try { _hostBufferByte = new byte[bitmap.Width * bitmap.Height * 4]; }
                    catch
                    {
                        string msg = string.Format("Out of memory.");
                        //Android.Widget.Toast.MakeText(this, msg, Android.Widget.ToastLength.Long).Show();
                        return null;
                    }

                    //if (Modified)
                    
                    //Java.Lang.Object byteBuffer = (Java.Nio.ByteBuffer)Java.Nio.ByteBuffer.FromArray<byte>(_hostBufferByte);
                    //byteBuffer.

                }

                ReadByteFromDevice(queue);

                try
                {
                    var byteBuffer = Java.Nio.ByteBuffer.AllocateDirect(bitmap.Width * bitmap.Height * 4);
                    Marshal.Copy(_hostBufferByte, 0, byteBuffer.GetDirectBufferAddress(), bitmap.Width * bitmap.Height * 4);
                    bitmap.CopyPixelsFromBuffer(byteBuffer);
                    //byteBuffer.Clear();
                    byteBuffer.Dispose();
                }
                catch
                { }
                return bitmap;
            }*/






            //bitmap.CopyPixelsFromBuffer(_hostBuffer)

            //return BitmapFactory.DecodeByteArray(scan, 0, _hostBuffer.Length);

        }


        public unsafe Bitmap ByteToBitmap(ClooCommandQueue queue, Bitmap bitmap)

        {
            if (queue == null) throw new ArgumentNullException("queue");

            lock (_lockRead)
            {
                //GCHandle handle = GCHandle.Alloc(_hostBufferByte, GCHandleType.Pinned);
                try { queue.Read(this, true, 0, Count, bitmap.LockPixels(), null); }
                finally {  bitmap.UnlockPixels(); }
                //finally { handle.Free(); bitmap.UnlockPixels();  }
            }

            try
            {
                //Marshal.Copy(_hostBufferByte, 0, byteBuffer.GetDirectBufferAddress(), bitmap.Width * bitmap.Height * 4);
                //byteBuffer = ;

                //bitmap.CopyPixelsFromBuffer(Java.Nio.ByteBuffer.Wrap(_hostBufferByte));
            }
            catch(Exception ex)
            {
            }
            return bitmap;

        }

        /// <summary>
        /// Creates a new managed bitmap
        /// </summary>
        /// <param name="queue">compute command queue</param>
        /// <returns>managed bitmap</returns>
        public unsafe Bitmap ToBitmap(ClooCommandQueue queue, int Width, int Height)
        {
            //Bitmap bitmap = new Bitmap(Width, Height, PixelFormat.Format32bppArgb);
            Bitmap bitmap = Bitmap.CreateBitmap(Width, Height, Bitmap.Config.Argb8888);
            ToBitmap(queue, bitmap);
            return bitmap;
        }
    }
}
