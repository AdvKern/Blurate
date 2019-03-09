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
using System.Diagnostics;
//using System.Drawing.Imaging;
using Android.Graphics;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Threading;

namespace OpenClooVision.Capture
{
    /// <summary>
    /// Capture device
    /// </summary>
    public class CaptureDevice : IDisposable
    {
        CaptureGrabber _captureGrabber;
        string _deviceMoniker;
        IntPtr _sharedMemSection;
        IntPtr _sharedMemMap;

        ManualResetEvent _manualResetEventStop;
        Thread _thread;

        private bool _hasNewFrame = false;
        private byte[] _bitmapBuffer;
        /// <summary>
        /// Raw bitmap buffer
        /// </summary>
        public byte[] BitmapBuffer { get { return _bitmapBuffer; } }

        /// <summary>
        /// Gets the image height, which is captured
        /// </summary>
        public int Height { get { return _captureGrabber.Height; } }

        /// <summary>
        /// Check if capture thread is running
        /// </summary>
        public bool IsRunning
        {
            get
            {
                if (_thread != null)
                {
                    if (!_thread.Join(0)) return true;
                }
                return false;
            }
        }

        /// <summary>
        /// Pixel size in bytes
        /// </summary>
        public int PixelSize { get { return _captureGrabber.PixelSize; } }

        /// <summary>
        /// Image pixel format
        /// </summary>
        public PixelFormat PixelFormat { get { return _captureGrabber.PixelFormat; } }

        /// <summary>
        /// Gets the image width, which is captured
        /// </summary>
        public int Width { get { return _captureGrabber.Width; } }

        /// <summary>
        /// Event fires if new frame is ready to read
        /// </summary>
        public event EventHandler NewFrame;

        object _newFrameLock = new object();

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="moniker">Moniker string of capture device to open</param>
        /// <param name="width">capture image width</param>
        /// <param name="height">capture image height</param>
        /// <param name="pixelFormat">pixel format (RGB or RGBA)</param>
        public CaptureDevice(string moniker, int width, int height, PixelFormat pixelFormat)
        {
            _deviceMoniker = moniker;
            _captureGrabber = new CaptureGrabber(width, height, pixelFormat);
            _bitmapBuffer = new byte[width * height * _captureGrabber.PixelSize];
        }

        /// <summary>
        /// Start thread to capture images
        /// </summary>
        public void Start()
        {
            if (_thread != null) Stop();

            _captureGrabber.NewFrame += new EventHandler(captureGrabber_NewFrame);

            // create access to shared memory
            uint length = (uint)(_captureGrabber.Width * _captureGrabber.Height * PixelSize);
            _sharedMemSection = CreateFileMapping(new IntPtr(-1), IntPtr.Zero, 0x04, 0, length, null);
            _sharedMemMap = MapViewOfFile(_sharedMemSection, 0xF001F, 0, 0, length);
            _captureGrabber.SharedMemMap = _sharedMemMap;

            _manualResetEventStop = new ManualResetEvent(false);
            _thread = new Thread(Execute);
            _thread.IsBackground = true;
            _thread.Start();
        }

        /// <summary>
        /// Stops the capture thread
        /// </summary>
        public void Stop()
        {
            if (IsRunning)
            {
                _captureGrabber.NewFrame -= new EventHandler(captureGrabber_NewFrame);

                _manualResetEventStop.Set();
                if (!_manualResetEventStop.WaitOne(200)) _thread.Abort();
                if (_thread != null) _thread = null;
            }
        }

        /// <summary>
        /// Dispose instance
        /// </summary>
        public void Dispose()
        {
            Stop();
            _captureGrabber = null;
        }

        /// <summary>
        /// New image frame has arrived
        /// </summary>
        /// <param name="sender">sender</param>
        /// <param name="e">event arguments</param>
        private void captureGrabber_NewFrame(object sender, EventArgs e)
        {
            if (_hasNewFrame) return;

            lock (_bitmapBuffer)
            {
                _hasNewFrame = true;
                Array.Copy(_captureGrabber.BitmapBuffer, _bitmapBuffer, _captureGrabber.BitmapBuffer.Length);
            }
        }

        /// <summary>
        /// Gets an array of all available DeviceMonikers
        /// </summary>
        public static FilterInfo[] DeviceMonikers
        {
            get
            {
                List<FilterInfo> filters = new List<FilterInfo>();
                IMoniker[] ms = new IMoniker[1];
                ICreateDevEnum devEnum = Activator.CreateInstance(Type.GetTypeFromCLSID(SystemDeviceEnum)) as ICreateDevEnum;
                IEnumMoniker moniker;
                Guid guid = VideoInputDevice;
                if (devEnum.CreateClassEnumerator(ref guid, out moniker, 0) == 0)
                {
                    do
                    {
                        int res = moniker.Next(1, ms, IntPtr.Zero);
                        if (res != 0 || ms[0] == null) break;
                        filters.Add(new FilterInfo(ms[0]));
                        Marshal.ReleaseComObject(ms[0]);
                        ms[0] = null;
                    } while (true);
                }

                return filters.ToArray();
            }
        }

        /// <summary>
        /// Thread execute
        /// </summary>
        private void Execute()
        {
            ICaptureGraphBuilder2 captureGraph = null;
            IGraphBuilder graph = null;
            ISampleGrabber grabber = null;
            IBaseFilter sourceBase = null;
            IBaseFilter grabberBase = null;
            IMediaControl mediaControl = null;

            try
            {
                captureGraph = Activator.CreateInstance(Type.GetTypeFromCLSID(CaptureGraphBuilder2)) as ICaptureGraphBuilder2;
                graph = Activator.CreateInstance(Type.GetTypeFromCLSID(FilterGraph)) as IGraphBuilder;
                sourceBase = FilterInfo.CreateFilter(_deviceMoniker);

                // set default width and height
                object streamConfigObject;
                captureGraph.FindInterface(PinCategory.Capture, MediaTypes.Video, sourceBase, typeof(IAMStreamConfig).GUID, out streamConfigObject);
                if (streamConfigObject != null)
                {
                    IAMStreamConfig streamConfig = (IAMStreamConfig)streamConfigObject;
                    AMMediaType mediaType = new AMMediaType();
                    streamConfig.GetFormat(out mediaType);
                    VideoInfoHeader header = (VideoInfoHeader)Marshal.PtrToStructure(mediaType.FormatPtr, typeof(VideoInfoHeader));
                    header.BitmapInfoHeader.Width = Width;
                    header.BitmapInfoHeader.Height = Height;
                    Marshal.StructureToPtr(header, mediaType.FormatPtr, false);
                    streamConfig.SetFormat(mediaType);
                    mediaType.Dispose();
                }

                grabber = Activator.CreateInstance(Type.GetTypeFromCLSID(SampleGrabber)) as ISampleGrabber;
                grabberBase = grabber as IBaseFilter;

                graph.AddFilter(sourceBase, "source");
                graph.AddFilter(grabberBase, "grabber");

                using (AMMediaType mediaType = new AMMediaType())
                {
                    mediaType.MajorType = MediaTypes.Video;
                    /*switch ((uint)PixelFormat)
                    {
                        case Android.Graphics.Bitmap.Config.:
                            mediaType.SubType = MediaSubTypes.RGB24;
                            break;
                        default:
                            mediaType.SubType = MediaSubTypes.RGB32;
                            break;
                    }*/
                    grabber.SetMediaType(mediaType);
                    grabber.SetBufferSamples(false);
                    grabber.SetCallback(_captureGrabber, 1);
                    grabber.SetOneShot(false);

                    if (graph.Connect(sourceBase.GetPin(PinDirection.Output, 0), grabberBase.GetPin(PinDirection.Input, 0)) >= 0)
                    {
                        if (grabber.GetConnectedMediaType(mediaType) == 0)
                        {
                            VideoInfoHeader header = (VideoInfoHeader)Marshal.PtrToStructure(mediaType.FormatPtr, typeof(VideoInfoHeader));
                            _captureGrabber.Width = header.BitmapInfoHeader.Width;
                            _captureGrabber.Height = header.BitmapInfoHeader.Height;

                            // check if desired size is not available and recreate buffer if needed
                            int newSize = _captureGrabber.Width * _captureGrabber.Height * PixelSize;
                            if (_bitmapBuffer.Length != newSize)
                            {
                                // re-create access to shared memory width new size
                                uint length = (uint)(_captureGrabber.Width * _captureGrabber.Height * PixelSize);
                                _sharedMemSection = CreateFileMapping(new IntPtr(-1), IntPtr.Zero, 0x04, 0, length, null);
                                _sharedMemMap = MapViewOfFile(_sharedMemSection, 0xF001F, 0, 0, length);
                                _captureGrabber.SharedMemMap = _sharedMemMap;

                                _bitmapBuffer = new byte[newSize];
                            }
                        }
                    }
                    graph.Render(grabberBase.GetPin(PinDirection.Output, 0));

                    IVideoWindow wnd = (IVideoWindow)graph;
                    wnd.put_AutoShow(false);
                    wnd = null;

                    mediaControl = (IMediaControl)graph;
                    mediaControl.Run();

                    while (!_manualResetEventStop.WaitOne(1))
                    {
                        if (!_hasNewFrame) continue;
                        try
                        {
                            lock (_bitmapBuffer)
                            {
                                // fire new frame event
                                if (NewFrame != null) NewFrame(this, EventArgs.Empty);
                            }
                        }
                        finally { _hasNewFrame = false; }
                    }

                    mediaControl.StopWhenReady();
                }
            }
            catch (ThreadAbortException)
            {
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
            finally
            {
                Marshal.ReleaseComObject(captureGraph); captureGraph = null;
                Marshal.ReleaseComObject(graph); graph = null;
                Marshal.ReleaseComObject(sourceBase); sourceBase = null;
                Marshal.ReleaseComObject(grabberBase); grabberBase = null;
                Marshal.ReleaseComObject(grabber); grabber = null;
                Marshal.ReleaseComObject(mediaControl); mediaControl = null;
            }
        }

        static readonly Guid CaptureGraphBuilder2 = new Guid(0xBF87B6E1, 0x8C27, 0x11d0, 0xB3, 0xF0, 0x0, 0xAA, 0x00, 0x37, 0x61, 0xC5);
        static readonly Guid FilterGraph = new Guid(0xE436EBB3, 0x524F, 0x11CE, 0x9F, 0x53, 0x00, 0x20, 0xAF, 0x0B, 0xA7, 0x70);
        static readonly Guid SampleGrabber = new Guid(0xC1F400A0, 0x3F08, 0x11D3, 0x9F, 0x0B, 0x00, 0x60, 0x08, 0x03, 0x9E, 0x37);
        static readonly Guid SystemDeviceEnum = new Guid(0x62BE5D10, 0x60EB, 0x11D0, 0xBD, 0x3B, 0x00, 0xA0, 0xC9, 0x11, 0xCE, 0x86);
        static readonly Guid VideoInputDevice = new Guid(0x860BB310, 0x5D01, 0x11D0, 0xBD, 0x3B, 0x00, 0xA0, 0xC9, 0x11, 0xCE, 0x86);

        [ComVisible(false)]
        public class PinCategory
        {
            public static readonly Guid Capture = new Guid(0xfb6c4281, 0x0353, 0x11d1, 0x90, 0x5f, 0x00, 0x00, 0xc0, 0xcc, 0x16, 0xba);
            public static readonly Guid Preview = new Guid(0xfb6c4282, 0x0353, 0x11d1, 0x90, 0x5f, 0x00, 0x00, 0xc0, 0xcc, 0x16, 0xba);
        }

        [ComVisible(false)]
        internal class MediaTypes
        {
            public static readonly Guid Video = new Guid(0x73646976, 0x0000, 0x0010, 0x80, 0x00, 0x00, 0xAA, 0x00, 0x38, 0x9B, 0x71);
            public static readonly Guid Interleaved = new Guid(0x73766169, 0x0000, 0x0010, 0x80, 0x00, 0x00, 0xAA, 0x00, 0x38, 0x9B, 0x71);
            public static readonly Guid Audio = new Guid(0x73647561, 0x0000, 0x0010, 0x80, 0x00, 0x00, 0xAA, 0x00, 0x38, 0x9B, 0x71);
            public static readonly Guid Text = new Guid(0x73747874, 0x0000, 0x0010, 0x80, 0x00, 0x00, 0xAA, 0x00, 0x38, 0x9B, 0x71);
            public static readonly Guid Stream = new Guid(0xE436EB83, 0x524F, 0x11CE, 0x9F, 0x53, 0x00, 0x20, 0xAF, 0x0B, 0xA7, 0x70);
        }

        [ComVisible(false)]
        internal class MediaSubTypes
        {
            public static readonly Guid YUYV = new Guid(0x56595559, 0x0000, 0x0010, 0x80, 0x00, 0x00, 0xAA, 0x00, 0x38, 0x9B, 0x71);
            public static readonly Guid IYUV = new Guid(0x56555949, 0x0000, 0x0010, 0x80, 0x00, 0x00, 0xAA, 0x00, 0x38, 0x9B, 0x71);
            public static readonly Guid DVSD = new Guid(0x44535644, 0x0000, 0x0010, 0x80, 0x00, 0x00, 0xAA, 0x00, 0x38, 0x9B, 0x71);
            public static readonly Guid RGB1 = new Guid(0xE436EB78, 0x524F, 0x11CE, 0x9F, 0x53, 0x00, 0x20, 0xAF, 0x0B, 0xA7, 0x70);
            public static readonly Guid RGB4 = new Guid(0xE436EB79, 0x524F, 0x11CE, 0x9F, 0x53, 0x00, 0x20, 0xAF, 0x0B, 0xA7, 0x70);
            public static readonly Guid RGB8 = new Guid(0xE436EB7A, 0x524F, 0x11CE, 0x9F, 0x53, 0x00, 0x20, 0xAF, 0x0B, 0xA7, 0x70);
            public static readonly Guid RGB565 = new Guid(0xE436EB7B, 0x524F, 0x11CE, 0x9F, 0x53, 0x00, 0x20, 0xAF, 0x0B, 0xA7, 0x70);
            public static readonly Guid RGB555 = new Guid(0xE436EB7C, 0x524F, 0x11CE, 0x9F, 0x53, 0x00, 0x20, 0xAF, 0x0B, 0xA7, 0x70);
            public static readonly Guid RGB24 = new Guid(0xE436Eb7D, 0x524F, 0x11CE, 0x9F, 0x53, 0x00, 0x20, 0xAF, 0x0B, 0xA7, 0x70);
            public static readonly Guid RGB32 = new Guid(0xE436EB7E, 0x524F, 0x11CE, 0x9F, 0x53, 0x00, 0x20, 0xAF, 0x0B, 0xA7, 0x70);
            public static readonly Guid Avi = new Guid(0xE436EB88, 0x524F, 0x11CE, 0x9F, 0x53, 0x00, 0x20, 0xAF, 0x0B, 0xA7, 0x70);
            public static readonly Guid Asf = new Guid(0x3DB80F90, 0x9412, 0x11D1, 0xAD, 0xED, 0x00, 0x00, 0xF8, 0x75, 0x4B, 0x99);
        }

        /// <summary>
        /// Create pointer to shared memory
        /// </summary>
        /// <param name="hFile">file handle</param>
        /// <param name="lpFileMappingAttrs">pointer to mapping attributes</param>
        /// <param name="protect"></param>
        /// <param name="dwMaxSizeHigh">most significant uint</param>
        /// <param name="dwMaxSizeLow">least significant uint</param>
        /// <param name="lpName">pointer of name</param>
        /// <returns></returns>
        [DllImport("kernel32.dll", SetLastError = true)]
        static extern IntPtr CreateFileMapping(IntPtr hFile, IntPtr lpFileMappingAttrs, uint protect, uint dwMaxSizeHigh, uint dwMaxSizeLow, string lpName);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern IntPtr MapViewOfFile(IntPtr hFileMappingObject, uint dwDesiredAccess, uint dwFileOffsetHigh, uint dwFileOffsetLow, uint dwNumBytesToMap);
    }
}
