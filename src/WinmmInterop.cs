using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace RMEFixer;

/// <summary>
/// P/Invoke bindings for <c>winmm.dll</c> (Windows Multimedia). All functions
/// use the <c>WINAPI</c> calling convention (<c>stdcall</c> on x86; the x64
/// fastcall ABI resolves to the same machine convention).
/// </summary>
[SupportedOSPlatform("windows")]
internal static partial class Winmm
{
    private const string Dll = "winmm.dll";

    public const uint MMSYSERR_NOERROR = 0;
    public const uint WOM_OPEN = 0x03BB;
    public const uint WOM_DONE = 0x03BD;
    public const uint WOM_CLOSE = 0x03BC;
    public const uint CALLBACK_FUNCTION = 0x00030000;
    public const uint WAVE_MAPPER = unchecked((uint)-1);
    public const uint WHDR_DONE = 0x00000001;
    public const uint WHDR_PREPARED = 0x00000002;
    public const uint WHDR_BEGINLOOP = 0x00000004;
    public const uint WHDR_ENDLOOP = 0x00000008;
    public const uint WHDR_INQUEUE = 0x00000010;

    [LibraryImport(Dll, EntryPoint = "waveOutGetErrorTextW")]
    [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvStdcall) })]
    public static partial uint waveOutGetErrorText(
        uint err,
        [Out] byte[] text,
        uint sizeBytes);

    [LibraryImport(Dll, EntryPoint = "waveOutOpen")]
    [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvStdcall) })]
    public static partial uint waveOutOpen(
        out IntPtr phwo,
        uint uDeviceID,
        ref WaveFormatEx pwfx,
        IntPtr dwCallback,
        IntPtr dwInstance,
        uint fdwOpen);

    [LibraryImport(Dll, EntryPoint = "waveOutPrepareHeader")]
    [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvStdcall) })]
    public static partial uint waveOutPrepareHeader(
        IntPtr hwo,
        ref WaveHeader pwh,
        uint cbwh);

    [LibraryImport(Dll, EntryPoint = "waveOutUnprepareHeader")]
    [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvStdcall) })]
    public static partial uint waveOutUnprepareHeader(
        IntPtr hwo,
        ref WaveHeader pwh,
        uint cbwh);

    [LibraryImport(Dll, EntryPoint = "waveOutWrite")]
    [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvStdcall) })]
    public static partial uint waveOutWrite(
        IntPtr hwo,
        ref WaveHeader pwh,
        uint cbwh);

    [LibraryImport(Dll, EntryPoint = "waveOutReset")]
    [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvStdcall) })]
    public static partial uint waveOutReset(IntPtr hwo);

    [LibraryImport(Dll, EntryPoint = "waveOutPause")]
    [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvStdcall) })]
    public static partial uint waveOutPause(IntPtr hwo);

    [LibraryImport(Dll, EntryPoint = "waveOutRestart")]
    [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvStdcall) })]
    public static partial uint waveOutRestart(IntPtr hwo);

    [LibraryImport(Dll, EntryPoint = "waveOutClose")]
    [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvStdcall) })]
    public static partial uint waveOutClose(IntPtr hwo);

    [LibraryImport(Dll, EntryPoint = "waveOutGetPosition")]
    [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvStdcall) })]
    public static partial uint waveOutGetPosition(
        IntPtr hwo,
        ref MmTime pmmt,
        uint cbmmt);

    [LibraryImport(Dll, EntryPoint = "waveOutGetNumDevs")]
    [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvStdcall) })]
    public static partial uint waveOutGetNumDevs();

    [LibraryImport(Dll, EntryPoint = "waveOutGetDevCapsW")]
    [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvStdcall) })]
    public static partial uint waveOutGetDevCaps(
        uint uDeviceID,
        out WaveOutCaps pwoc,
        uint cbwoc);

    public static string GetErrorText(uint err)
    {
        var buf = new byte[512];
        var res = waveOutGetErrorText(err, buf, (uint)buf.Length);
        if (res != MMSYSERR_NOERROR)
        {
            return $"winmm 错误 0x{err:X8}";
        }
        var byteLen = 0;
        while (byteLen + 1 < buf.Length && buf[byteLen] != 0 && buf[byteLen + 1] != 0)
        {
            byteLen += 2;
        }
        return System.Text.Encoding.Unicode.GetString(buf, 0, byteLen);
    }
}

[UnmanagedFunctionPointer(CallingConvention.StdCall)]
public delegate void WaveOutProc(
    IntPtr hwo,
    uint uMsg,
    IntPtr dwInstance,
    IntPtr dwParam1,
    IntPtr dwParam2);

[StructLayout(LayoutKind.Sequential)]
public struct WaveFormatEx
{
    public ushort wFormatTag;
    public ushort nChannels;
    public uint nSamplesPerSec;
    public uint nAvgBytesPerSec;
    public ushort nBlockAlign;
    public ushort wBitsPerSample;
    public ushort cbSize;
}

[StructLayout(LayoutKind.Sequential)]
public struct WaveHeader
{
    public IntPtr lpData;
    public uint dwBufferLength;
    public uint dwBytesRecorded;
    public IntPtr dwUser;
    public uint dwFlags;
    public uint dwLoops;
    public IntPtr lpNext;
    public IntPtr reserved;
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct WaveOutCaps
{
    public ushort wMid;
    public ushort wPid;
    public uint vDriverVersion;
    public fixed char szPname[32];
    public uint dwFormats;
    public ushort wChannels;
    public ushort wReserved1;
    public ushort dwSupport;

    public readonly string Name
    {
        get
        {
            fixed (char* p = szPname)
            {
                var len = 0;
                while (len < 32 && p[len] != '\0') len++;
                return new string(p, 0, len);
            }
        }
    }
}

[StructLayout(LayoutKind.Sequential)]
public struct MmTime
{
    public uint wType;
    public uint cb;
    public uint dwMilliSecs;
    public uint dwMicroSecs;
    public uint dwSamples;
}
