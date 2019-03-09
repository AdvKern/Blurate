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
using System.Runtime.InteropServices.ComTypes;
//using System.Drawing;
using Android.Graphics;
using System.Drawing;

namespace OpenClooVision.Capture
{
    [ComImport]
    [Guid("56A868A9-0AD4-11CE-B03A-0020AF0BA770")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IGraphBuilder
    {
        [PreserveSig]
        int AddFilter([In] IBaseFilter filter, [In, MarshalAs(UnmanagedType.LPWStr)] string name);

        [PreserveSig]
        int RemoveFilter([In] IBaseFilter filter);

        [PreserveSig]
        int EnumFilters([Out] out IntPtr enumerator);

        [PreserveSig]
        int FindFilterByName([In, MarshalAs(UnmanagedType.LPWStr)] string name, [Out] out IBaseFilter filter);

        [PreserveSig]
        int ConnectDirect([In] IPin pinOut, [In] IPin pinIn, [In, MarshalAs(UnmanagedType.LPStruct)] AMMediaType mediaType);

        [PreserveSig]
        int Reconnect([In] IPin pin);

        [PreserveSig]
        int Disconnect([In] IPin pin);

        [PreserveSig]
        int SetDefaultSyncSource();

        [PreserveSig]
        int Connect([In] IPin pinOut, [In] IPin pinIn);

        [PreserveSig]
        int Render([In] IPin pinOut);

        [PreserveSig]
        int RenderFile(
            [In, MarshalAs(UnmanagedType.LPWStr)] string file,
            [In, MarshalAs(UnmanagedType.LPWStr)] string playList);

        [PreserveSig]
        int AddSourceFilter(
            [In, MarshalAs(UnmanagedType.LPWStr)] string fileName,
            [In, MarshalAs(UnmanagedType.LPWStr)] string filterName,
            [Out] out IBaseFilter filter);

        [PreserveSig]
        int SetLogFile(IntPtr hFile);

        [PreserveSig]
        int Abort();

        [PreserveSig]
        int ShouldOperationContinue();
    }

    [ComImport]
    [Guid("56A86895-0AD4-11CE-B03A-0020AF0BA770")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IBaseFilter
    {
        [PreserveSig]
        int GetClassID([Out] out Guid ClassID);

        [PreserveSig]
        int Stop();

        [PreserveSig]
        int Pause();

        [PreserveSig]
        int Run(long start);

        [PreserveSig]
        int GetState(int milliSecsTimeout, [Out] out int filterState);

        [PreserveSig]
        int SetSyncSource([In] IntPtr clock);

        [PreserveSig]
        int GetSyncSource([Out] out IntPtr clock);

        [PreserveSig]
        int EnumPins([Out] out IEnumPins enumPins);

        [PreserveSig]
        int FindPin([In, MarshalAs(UnmanagedType.LPWStr)] string id, [Out] out IPin pin);

        [PreserveSig]
        int QueryFilterInfo([Out] FilterInfo filterInfo);

        [PreserveSig]
        int JoinFilterGraph([In] IFilterGraph graph, [In, MarshalAs(UnmanagedType.LPWStr)] string name);

        [PreserveSig]
        int QueryVendorInfo([Out, MarshalAs(UnmanagedType.LPWStr)] out string vendorInfo);
    }

    [ComImport, Guid("56A86891-0AD4-11CE-B03A-0020AF0BA770"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IPin
    {
        [PreserveSig]
        int Connect([In] IPin receivePin, [In, MarshalAs(UnmanagedType.LPStruct)] AMMediaType mediaType);

        [PreserveSig]
        int ReceiveConnection([In] IPin receivePin, [In, MarshalAs(UnmanagedType.LPStruct)] AMMediaType mediaType);

        [PreserveSig]
        int Disconnect();

        [PreserveSig]
        int ConnectedTo([Out] out IPin pin);

        [PreserveSig]
        int ConnectionMediaType([Out, MarshalAs(UnmanagedType.LPStruct)] AMMediaType mediaType);

        [PreserveSig]
        int QueryPinInfo([Out, MarshalAs(UnmanagedType.LPStruct)] PinInfo pinInfo);

        [PreserveSig]
        int QueryDirection(out PinDirection pinDirection);

        [PreserveSig]
        int QueryId([Out, MarshalAs(UnmanagedType.LPWStr)] out string id);

        [PreserveSig]
        int QueryAccept([In, MarshalAs(UnmanagedType.LPStruct)] AMMediaType mediaType);

        [PreserveSig]
        int EnumMediaTypes(IntPtr enumerator);

        [PreserveSig]
        int QueryInternalConnections(IntPtr apPin, [In, Out] ref int nPin);

        [PreserveSig]
        int EndOfStream();

        [PreserveSig]
        int BeginFlush();

        [PreserveSig]
        int EndFlush();

        [PreserveSig]
        int NewSegment(long start, long stop, double rate);
    }

    [ComImport]
    [Guid("56A86892-0AD4-11CE-B03A-0020AF0BA770")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IEnumPins
    {
        [PreserveSig]
        int Next([In] int cPins,
           [Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)] IPin[] pins,
           [Out] out int pinsFetched);

        [PreserveSig]
        int Skip([In] int cPins);

        [PreserveSig]
        int Reset();

        [PreserveSig]
        int Clone([Out] out IEnumPins enumPins);
    }

    [ComImport]
    [Guid("56A8689F-0AD4-11CE-B03A-0020AF0BA770")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IFilterGraph
    {
        [PreserveSig]
        int AddFilter([In] IBaseFilter filter, [In, MarshalAs(UnmanagedType.LPWStr)] string name);

        [PreserveSig]
        int RemoveFilter([In] IBaseFilter filter);

        [PreserveSig]
        int EnumFilters([Out] out IntPtr enumerator);

        [PreserveSig]
        int FindFilterByName([In, MarshalAs(UnmanagedType.LPWStr)] string name, [Out] out IBaseFilter filter);

        [PreserveSig]
        int ConnectDirect([In] IPin pinOut, [In] IPin pinIn, [In, MarshalAs(UnmanagedType.LPStruct)] AMMediaType mediaType);

        [PreserveSig]
        int Reconnect([In] IPin pin);

        [PreserveSig]
        int Disconnect([In] IPin pin);

        [PreserveSig]
        int SetDefaultSyncSource();
    }

    [ComImport, Guid("55272A00-42CB-11CE-8135-00AA004BB851"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IPropertyBag
    {
        [PreserveSig]
        int Read(
            [In, MarshalAs(UnmanagedType.LPWStr)] string propertyName,
            [In, Out, MarshalAs(UnmanagedType.Struct)] ref object pVar,
            [In] IntPtr pErrorLog);

        [PreserveSig]
        int Write(
            [In, MarshalAs(UnmanagedType.LPWStr)] string propertyName,
            [In, MarshalAs(UnmanagedType.Struct)] ref object pVar);
    }

    [ComImport, Guid("6B652FFF-11FE-4FCE-92AD-0266B5D7C78F"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface ISampleGrabber
    {
        [PreserveSig]
        int SetOneShot([In, MarshalAs(UnmanagedType.Bool)] bool oneShot);

        [PreserveSig]
        int SetMediaType([In, MarshalAs(UnmanagedType.LPStruct)] AMMediaType mediaType);

        [PreserveSig]
        int GetConnectedMediaType([Out, MarshalAs(UnmanagedType.LPStruct)] AMMediaType mediaType);

        [PreserveSig]
        int SetBufferSamples([In, MarshalAs(UnmanagedType.Bool)] bool bufferThem);

        [PreserveSig]
        int GetCurrentBuffer(ref int bufferSize, IntPtr buffer);

        [PreserveSig]
        int GetCurrentSample(IntPtr sample);

        [PreserveSig]
        int SetCallback(ISampleGrabberCB callback, int whichMethodToCallback);
    }

    [ComImport, Guid("0579154A-2B53-4994-B0D0-E773148EFF85"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface ISampleGrabberCB
    {
        [PreserveSig]
        int SampleCB(double sampleTime, IntPtr sample);

        [PreserveSig]
        int BufferCB(double sampleTime, IntPtr buffer, int bufferLen);
    }

    [ComImport, Guid("29840822-5B84-11D0-BD3B-00A0C911CE86"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface ICreateDevEnum
    {
        [PreserveSig]
        int CreateClassEnumerator([In] ref Guid type, [Out] out IEnumMoniker enumMoniker, [In] int flags);
    }

    [ComImport, Guid("56A868B4-0AD4-11CE-B03A-0020AF0BA770"), InterfaceType(ComInterfaceType.InterfaceIsDual)]
    internal interface IVideoWindow
    {
        [PreserveSig]
        int put_Caption(string caption);
        [PreserveSig]
        int get_Caption([Out] out string caption);
        [PreserveSig]
        int put_WindowStyle(int windowStyle);
        [PreserveSig]
        int get_WindowStyle(out int windowStyle);
        [PreserveSig]
        int put_WindowStyleEx(int windowStyleEx);
        [PreserveSig]
        int get_WindowStyleEx(out int windowStyleEx);
        [PreserveSig]
        int put_AutoShow([In, MarshalAs(UnmanagedType.Bool)] bool autoShow);
        [PreserveSig]
        int get_AutoShow([Out, MarshalAs(UnmanagedType.Bool)] out bool autoShow);
        [PreserveSig]
        int put_WindowState(int windowState);
        [PreserveSig]
        int get_WindowState(out int windowState);
        [PreserveSig]
        int put_BackgroundPalette([In, MarshalAs(UnmanagedType.Bool)] bool backgroundPalette);
        [PreserveSig]
        int get_BackgroundPalette([Out, MarshalAs(UnmanagedType.Bool)] out bool backgroundPalette);
        [PreserveSig]
        int put_Visible([In, MarshalAs(UnmanagedType.Bool)] bool visible);
        [PreserveSig]
        int get_Visible([Out, MarshalAs(UnmanagedType.Bool)] out bool visible);
        [PreserveSig]
        int put_Left(int left);
        [PreserveSig]
        int get_Left(out int left);
        [PreserveSig]
        int put_Width(int width);
        [PreserveSig]
        int get_Width(out int width);
        [PreserveSig]
        int put_Top(int top);
        [PreserveSig]
        int get_Top(out int top);
        [PreserveSig]
        int put_Height(int height);
        [PreserveSig]
        int get_Height(out int height);
        [PreserveSig]
        int put_Owner(IntPtr owner);
        [PreserveSig]
        int get_Owner(out IntPtr owner);
        [PreserveSig]
        int put_MessageDrain(IntPtr drain);
        [PreserveSig]
        int get_MessageDrain(out IntPtr drain);
        [PreserveSig]
        int get_BorderColor(out int color);
        [PreserveSig]
        int put_BorderColor(int color);
        [PreserveSig]
        int get_FullScreenMode([Out, MarshalAs(UnmanagedType.Bool)] out bool fullScreenMode);
        [PreserveSig]
        int put_FullScreenMode([In, MarshalAs(UnmanagedType.Bool)] bool fullScreenMode);
        [PreserveSig]
        int SetWindowForeground(int focus);
        [PreserveSig]
        int NotifyOwnerMessage(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam);
        [PreserveSig]
        int SetWindowPosition(int left, int top, int width, int height);
        [PreserveSig]
        int GetWindowPosition(out int left, out int top, out int width, out int height);
        [PreserveSig]
        int GetMinIdealImageSize(out int width, out int height);
        [PreserveSig]
        int GetMaxIdealImageSize(out int width, out int height);
        [PreserveSig]
        int GetRestorePosition(out int left, out int top, out int width, out int height);
        [PreserveSig]
        int HideCursor([In, MarshalAs(UnmanagedType.Bool)] bool hideCursor);
        [PreserveSig]
        int IsCursorHidden([Out, MarshalAs(UnmanagedType.Bool)] out bool hideCursor);
    }

    [ComImport, Guid("56A868B1-0AD4-11CE-B03A-0020AF0BA770"), InterfaceType(ComInterfaceType.InterfaceIsDual)]
    internal interface IMediaControl
    {
        [PreserveSig]
        int Run();

        [PreserveSig]
        int Pause();

        [PreserveSig]
        int Stop();

        [PreserveSig]
        int GetState(int timeout, out int filterState);

        [PreserveSig]
        int RenderFile(string fileName);

        [PreserveSig]
        int AddSourceFilter([In] string fileName, [Out, MarshalAs(UnmanagedType.IDispatch)] out object filterInfo);

        [PreserveSig]
        int get_FilterCollection(
            [Out, MarshalAs(UnmanagedType.IDispatch)] out object collection);

        [PreserveSig]
        int get_RegFilterCollection(
            [Out, MarshalAs(UnmanagedType.IDispatch)] out object collection);

        [PreserveSig]
        int StopWhenReady();
    }

    public class FilterInfo : IComparable
    {
        /// <summary>
        /// Name of filter
        /// </summary>
        public string Name { get; protected set; }

        /// <summary>
        /// Moniker
        /// </summary>
        public string MonikerString { get; protected set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="moniker">Moniker</param>
        public FilterInfo(string moniker)
        {
            MonikerString = moniker;
            Name = GetName(moniker);
        }

        internal FilterInfo(IMoniker moniker)
        {
            MonikerString = GetMonikerString(moniker);
            Name = GetName(moniker);
        }

        public int CompareTo(object value)
        {
            FilterInfo f = value as FilterInfo;
            if (f == null) return 1;

            return (Name.CompareTo(f.Name));
        }

        internal static IBaseFilter CreateFilter(string filterMoniker)
        {
            object filterObject = null;
            IBindCtx bindCtx = null;
            IMoniker moniker = null;

            int n = 0;

            if (CreateBindCtx(0, out bindCtx) == 0)
            {
                if (MkParseDisplayName(bindCtx, filterMoniker, ref n, out moniker) == 0)
                {
                    Guid filterId = typeof(IBaseFilter).GUID;
                    moniker.BindToObject(null, null, ref filterId, out filterObject);

                    Marshal.ReleaseComObject(moniker);
                }
                Marshal.ReleaseComObject(bindCtx);
            }
            return filterObject as IBaseFilter;
        }

        private string GetMonikerString(IMoniker moniker)
        {
            string str;
            moniker.GetDisplayName(null, null, out str);
            return str;
        }

        private string GetName(IMoniker moniker)
        {
            object bagObj = null;
            IPropertyBag bag = null;

            try
            {
                Guid bagId = typeof(IPropertyBag).GUID;
                moniker.BindToStorage(null, null, ref bagId, out bagObj);
                bag = (IPropertyBag)bagObj;

                object val = null;
                int hr = bag.Read("FriendlyName", ref val, IntPtr.Zero);
                if (hr != 0)
                    Marshal.ThrowExceptionForHR(hr);
                return (string)val;
            }
            finally
            {
                bag = null;
                if (bagObj != null)
                {
                    Marshal.ReleaseComObject(bagObj);
                    bagObj = null;
                }
            }
        }

        private string GetName(string monikerString)
        {
            IBindCtx bindCtx = null;
            IMoniker moniker = null;
            String name = "";
            int n = 0;

            if (CreateBindCtx(0, out bindCtx) == 0)
            {
                if (MkParseDisplayName(bindCtx, monikerString, ref n, out moniker) == 0)
                {
                    name = GetName(moniker);

                    Marshal.ReleaseComObject(moniker);
                    moniker = null;
                }
                Marshal.ReleaseComObject(bindCtx);
                bindCtx = null;
            }
            return name;
        }

        [DllImport("ole32.dll")]
        public static extern int CreateBindCtx(int reserved, out IBindCtx ppbc);

        [DllImport("ole32.dll", CharSet = CharSet.Unicode)]
        public static extern int MkParseDisplayName(IBindCtx pbc, string szUserName, ref int pchEaten, out IMoniker ppmk);
    }

    [ComVisible(false)]
    internal enum PinDirection
    {
        Input,
        Output
    }

    [ComVisible(false)]
    [StructLayout(LayoutKind.Sequential)]
    internal class AMMediaType : IDisposable
    {
        public Guid MajorType;
        public Guid SubType;

        [MarshalAs(UnmanagedType.Bool)]
        public bool FixedSizeSamples = true;

        [MarshalAs(UnmanagedType.Bool)]
        public bool TemporalCompression;

        public int SampleSize = 1;
        public Guid FormatType;
        public IntPtr UnknownPtr;
        public int FormatSize;
        public IntPtr FormatPtr;

        /// <summary>
        /// Destructor
        /// </summary>
        ~AMMediaType()
        {
            Dispose(false);
        }

        /// <summary>
        /// Dispose object
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            // remove me from the Finalization queue 
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Free resources being used
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            if (FormatSize != 0) Marshal.FreeCoTaskMem(FormatPtr);
            if (UnknownPtr != IntPtr.Zero) Marshal.Release(UnknownPtr);
        }
    }

    [ComVisible(false), StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Unicode)]
    internal class PinInfo
    {
        public IBaseFilter Filter;
        public PinDirection Direction;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
        public string Name;
    }

    /// <summary>
    /// Rectangle as used in DirectShow
    /// </summary>
    [ComVisible(false)]
    [StructLayout(LayoutKind.Sequential)]
    internal struct NativeRect
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }

    /// <summary>
    /// Bitmap info header
    /// </summary>
    [ComVisible(false)]
    [StructLayout(LayoutKind.Sequential, Pack = 2)]
    internal struct BitmapInfoHeader
    {
        public int Size;
        public int Width;
        public int Height;
        public short Planes;
        public short BitCount;
        public int Compression;
        public int ImageSize;
        public int XPelsPerMeter;
        public int YPelsPerMeter;
        public int ColorsUsed;
        public int ColorsImportant;
    }

    /// <summary>
    /// Video info header
    /// </summary>
    [ComVisible(false)]
    [StructLayout(LayoutKind.Sequential)]
    internal struct VideoInfoHeader
    {
        public NativeRect SrcRect;
        public NativeRect TargetRect;
        public int BitRate;
        public int BitErrorRate;
        public long AvgTimePerFrame;
        public BitmapInfoHeader BitmapInfoHeader;
    }

    /// <summary>
    /// From AnalogVideoStandard
    /// </summary>
    [Flags]
    public enum AnalogVideoStandard
    {
        None = 0x00000000,
        NTSC_M = 0x00000001,
        NTSC_M_J = 0x00000002,
        NTSC_433 = 0x00000004,
        PAL_B = 0x00000010,
        PAL_D = 0x00000020,
        PAL_G = 0x00000040,
        PAL_H = 0x00000080,
        PAL_I = 0x00000100,
        PAL_M = 0x00000200,
        PAL_N = 0x00000400,
        PAL_60 = 0x00000800,
        SECAM_B = 0x00001000,
        SECAM_D = 0x00002000,
        SECAM_G = 0x00004000,
        SECAM_H = 0x00008000,
        SECAM_K = 0x00010000,
        SECAM_K1 = 0x00020000,
        SECAM_L = 0x00040000,
        SECAM_L1 = 0x00080000,
        PAL_N_COMBO = 0x00100000,

        NTSCMask = 0x00000007,
        PALMask = 0x00100FF0,
        SECAMMask = 0x000FF000
    }

    /// <summary>
    /// From VIDEO_STREAM_CONFIG_CAPS
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public class VideoStreamConfigCaps
    {
        public Guid guid;
        public AnalogVideoStandard VideoStandard;
        public Size InputSize;
        public Size MinCroppingSize;
        public Size MaxCroppingSize;
        public int CropGranularityX;
        public int CropGranularityY;
        public int CropAlignX;
        public int CropAlignY;
        public Size MinOutputSize;
        public Size MaxOutputSize;
        public int OutputGranularityX;
        public int OutputGranularityY;
        public int StretchTapsX;
        public int StretchTapsY;
        public int ShrinkTapsX;
        public int ShrinkTapsY;
        public long MinFrameInterval;
        public long MaxFrameInterval;
        public int MinBitsPerSecond;
        public int MaxBitsPerSecond;
    }

    /// <summary>
    /// This interface sets the output format on certain capture and compression filters,
    /// for both audio and video.
    /// </summary>
    [ComImport]
    [Guid("C6E13340-30AC-11d0-A18C-00A0C9118956")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IAMStreamConfig
    {
        [PreserveSig]
        int SetFormat([In, MarshalAs(UnmanagedType.LPStruct)] AMMediaType mediaType);

        [PreserveSig]
        int GetFormat([Out] out AMMediaType mediaType);

        [PreserveSig]
        int GetNumberOfCapabilities(out int count, out int size);

        [PreserveSig]
        int GetStreamCaps(
            [In] int index,
            [Out] out AMMediaType mediaType,
            [In, MarshalAs(UnmanagedType.LPStruct)] VideoStreamConfigCaps streamConfigCaps
            );
    }

    /// <summary>
    /// This interface builds capture graphs and other custom filter graphs. 
    /// </summary>
    [ComImport]
    [Guid("93E5A4E0-2D50-11d2-ABFA-00A0C9C6E38D")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface ICaptureGraphBuilder2
    {
        [PreserveSig]
        int SetFiltergraph([In] IGraphBuilder graphBuilder);

        [PreserveSig]
        int GetFiltergraph([Out] out IGraphBuilder graphBuilder);

        [PreserveSig]
        int SetOutputFileName([In, MarshalAs(UnmanagedType.LPStruct)] Guid type,
            [In, MarshalAs(UnmanagedType.LPWStr)] string fileName,
            [Out] out IBaseFilter baseFilter,
            [Out] out IntPtr fileSinkFilter);

        [PreserveSig]
        int FindInterface([In, MarshalAs(UnmanagedType.LPStruct)] Guid category,
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid type,
            [In] IBaseFilter baseFilter,
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid interfaceID,
            [Out, MarshalAs(UnmanagedType.IUnknown)] out object retInterface);

        [PreserveSig]
        int RenderStream([In, MarshalAs(UnmanagedType.LPStruct)] Guid category,
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid mediaType,
            [In, MarshalAs(UnmanagedType.IUnknown)] object source,
            [In] IBaseFilter compressor,
            [In] IBaseFilter renderer);

        [PreserveSig]
        int ControlStream([In, MarshalAs(UnmanagedType.LPStruct)] Guid category,
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid mediaType,
            [In, MarshalAs(UnmanagedType.Interface)] IBaseFilter filter,
            [In] long start,
            [In] long stop,
            [In] short startCookie,
            [In] short stopCookie);

        [PreserveSig]
        int AllocCapFile([In, MarshalAs(UnmanagedType.LPWStr)] string fileName,
            [In] long size);

        [PreserveSig]
        int CopyCaptureFile([In, MarshalAs(UnmanagedType.LPWStr)] string oldFileName,
            [In, MarshalAs(UnmanagedType.LPWStr)] string newFileName,
            [In, MarshalAs(UnmanagedType.Bool)] bool allowEscAbort,
            [In] IntPtr callback);

        [PreserveSig]
        int FindPin([In, MarshalAs(UnmanagedType.IUnknown)] object source,
            [In] PinDirection pinDirection,
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid category,
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid mediaType,
            [In, MarshalAs(UnmanagedType.Bool)] bool unconnected,
            [In] int index,
            [Out, MarshalAs(UnmanagedType.Interface)] out IPin pin);
    }
}
