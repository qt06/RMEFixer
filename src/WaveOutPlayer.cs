using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace RMEFixer;

[SupportedOSPlatform("windows")]
public sealed class WaveOutPlayer : IDisposable
{
    private const int BufferCount = 8;
    private const int MilliSecPerBuffer = 200;

    private readonly object _gate = new();
    private readonly int _bufferSize;
    private int _deviceId;
    private readonly int _sampleRate;
    private readonly int _channels;
    private readonly short _bitsPerSample;
    private readonly byte[][] _buffers;
    private readonly WaveHeader[] _headers;
    private readonly GCHandle[] _bufferHandles;
    private readonly GCHandle _headersHandle;
    private readonly bool[] _busy;
    private readonly Queue<byte[]> _pending = new();
    private readonly List<byte> _carry = new();
    private readonly WaveOutProc _callback;
    private readonly GCHandle _callbackHandle;
    private IntPtr _device;
    private bool _disposed;
    private volatile int _resetting;
    private readonly ManualResetEventSlim _resetDone = new(true);

    public event EventHandler? PlaybackComplete;

    public int DeviceId => _deviceId;

    public bool IsPlaying
    {
        get
        {
            lock (_gate)
            {
                if (_disposed) return false;
                for (var i = 0; i < BufferCount; i++)
                    if (_busy[i]) return true;
                return _pending.Count > 0 || _carry.Count > 0;
            }
        }
    }

    public WaveOutPlayer(int sampleRate = 22050, int channels = 1, short bitsPerSample = 16, int deviceId = -1)
    {
        _sampleRate = sampleRate;
        _channels = channels;
        _bitsPerSample = bitsPerSample;
        var frameSize = channels * (bitsPerSample / 8);
        _bufferSize = sampleRate * frameSize * MilliSecPerBuffer / 1000;
        _buffers = new byte[BufferCount][];
        _headers = new WaveHeader[BufferCount];
        _bufferHandles = new GCHandle[BufferCount];
        _busy = new bool[BufferCount];

        for (var i = 0; i < BufferCount; i++)
        {
            _buffers[i] = new byte[_bufferSize];
            _bufferHandles[i] = GCHandle.Alloc(_buffers[i], GCHandleType.Pinned);
            _headers[i] = new WaveHeader
            {
                lpData = Marshal.UnsafeAddrOfPinnedArrayElement(_buffers[i], 0),
                dwBufferLength = (uint)_bufferSize,
                dwUser = (IntPtr)i,
            };
        }

        _headersHandle = GCHandle.Alloc(_headers, GCHandleType.Pinned);
        _callback = OnWaveOutProc;
        _callbackHandle = GCHandle.Alloc(_callback);
        _deviceId = deviceId < 0 ? -1 : deviceId;

        var fmt = new WaveFormatEx
        {
            wFormatTag = 0x0001,
            nChannels = (ushort)channels,
            nSamplesPerSec = (uint)sampleRate,
            nAvgBytesPerSec = (uint)(sampleRate * frameSize),
            nBlockAlign = (ushort)frameSize,
            wBitsPerSample = (ushort)bitsPerSample,
        };

        var devId = deviceId < 0 ? Winmm.WAVE_MAPPER : (uint)deviceId;
        var res = Winmm.waveOutOpen(out _device, devId, ref fmt,
            Marshal.GetFunctionPointerForDelegate(_callback), IntPtr.Zero, Winmm.CALLBACK_FUNCTION);
        if (res != 0)
        {
            Cleanup();
            throw new InvalidOperationException($"waveOutOpen 失败: 0x{res:X8}");
        }

        for (var i = 0; i < BufferCount; i++)
        {
            res = Winmm.waveOutPrepareHeader(_device, ref _headers[i], (uint)Marshal.SizeOf<WaveHeader>());
            if (res != 0)
            {
                Winmm.waveOutClose(_device);
                _device = IntPtr.Zero;
                Cleanup();
                throw new InvalidOperationException($"waveOutPrepareHeader 失败: 0x{res:X8}");
            }
        }
    }

    public void Feed(ReadOnlySpan<byte> pcm)
    {
        if (pcm.Length == 0) return;

        var copy = new byte[pcm.Length];
        pcm.CopyTo(copy);

        if (_resetting == 1)
            _resetDone.Wait(3000);

        lock (_gate)
        {
            if (_disposed) return;
            _pending.Enqueue(copy);
            Drain();
        }
    }

    public void DrainPending()
    {
        lock (_gate)
        {
            if (_disposed) return;
            Drain();
        }
    }

    public void Stop()
    {
        bool needsReset;
        lock (_gate)
        {
            _pending.Clear();
            _carry.Clear();
            needsReset = false;
            for (var i = 0; i < BufferCount; i++)
            {
                if (_busy[i])
                {
                    needsReset = true;
                    break;
                }
            }
            if (!needsReset)
            {
                Array.Clear(_busy, 0, _busy.Length);
                return;
            }
        }

        if (_device != IntPtr.Zero && Interlocked.CompareExchange(ref _resetting, 1, 0) == 0)
        {
            var dev = _device;
            _resetDone.Reset();
            _ = Task.Run(() =>
            {
                try { Winmm.waveOutReset(dev); }
                finally
                {
                    Interlocked.Exchange(ref _resetting, 0);
                    _resetDone.Set();
                }
            });
        }

        lock (_gate) { Array.Clear(_busy, 0, _busy.Length); }
    }

    public void Pause(bool pause)
    {
        IntPtr dev;
        lock (_gate)
        {
            if (_disposed) return;
            dev = _device;
        }
        if (dev == IntPtr.Zero) return;
        if (pause) Winmm.waveOutPause(dev);
        else Winmm.waveOutRestart(dev);
    }

    public void Close() => Dispose();

    public void ChangeDevice(int newDeviceId)
    {
        var mappedNew = newDeviceId < 0 ? -1 : newDeviceId;
        if (mappedNew == _deviceId) return;

        IntPtr oldDevice;
        lock (_gate)
        {
            if (_disposed) return;

            oldDevice = _device;
            _device = IntPtr.Zero;
            _pending.Clear();
            _carry.Clear();
            Array.Clear(_busy, 0, _busy.Length);
        }

        if (oldDevice != IntPtr.Zero)
        {
            Winmm.waveOutReset(oldDevice);
            for (var i = 0; i < BufferCount; i++)
                Winmm.waveOutUnprepareHeader(oldDevice, ref _headers[i], (uint)Marshal.SizeOf<WaveHeader>());
            Winmm.waveOutClose(oldDevice);
        }

        var frameSize = _channels * (_bitsPerSample / 8);
        var fmt = new WaveFormatEx
        {
            wFormatTag = 0x0001,
            nChannels = (ushort)_channels,
            nSamplesPerSec = (uint)_sampleRate,
            nAvgBytesPerSec = (uint)(_sampleRate * frameSize),
            nBlockAlign = (ushort)frameSize,
            wBitsPerSample = (ushort)_bitsPerSample,
        };

        var devId = newDeviceId < 0 ? Winmm.WAVE_MAPPER : (uint)newDeviceId;
        var res = Winmm.waveOutOpen(out var newDevice, devId, ref fmt,
            Marshal.GetFunctionPointerForDelegate(_callback), IntPtr.Zero, Winmm.CALLBACK_FUNCTION);

        lock (_gate)
        {
            if (res != 0)
            {
                _device = IntPtr.Zero;
                _deviceId = -1;
                throw new InvalidOperationException($"waveOutOpen 失败: 0x{res:X8}");
            }

            _device = newDevice;

            for (var i = 0; i < BufferCount; i++)
            {
                res = Winmm.waveOutPrepareHeader(_device, ref _headers[i], (uint)Marshal.SizeOf<WaveHeader>());
                if (res != 0)
                {
                    Winmm.waveOutClose(_device);
                    _device = IntPtr.Zero;
                    _deviceId = -1;
                    throw new InvalidOperationException($"waveOutPrepareHeader 失败: 0x{res:X8}");
                }
            }

            _deviceId = mappedNew;
        }
    }

    public static List<AudioDeviceInfo> EnumerateDevices()
    {
        var count = Winmm.waveOutGetNumDevs();
        var list = new List<AudioDeviceInfo>((int)count + 1)
        {
            new AudioDeviceInfo { Id = -1, Name = "默认设备" }
        };
        for (uint i = 0; i < count; i++)
        {
            if (Winmm.waveOutGetDevCaps(i, out var caps, (uint)Marshal.SizeOf<WaveOutCaps>()) == 0)
                list.Add(new AudioDeviceInfo { Id = (int)i, Name = caps.Name });
        }
        return list;
    }

    public void Dispose()
    {
        if (_disposed) return;

        if (Interlocked.Exchange(ref _resetting, -1) == 1)
            _resetDone.Wait(3000);

        IntPtr dev;
        lock (_gate)
        {
            if (_disposed) return;
            _disposed = true;
            dev = _device;
            _device = IntPtr.Zero;
        }

        if (dev != IntPtr.Zero)
        {
            Winmm.waveOutReset(dev);
            for (var i = 0; i < BufferCount; i++)
                Winmm.waveOutUnprepareHeader(dev, ref _headers[i], (uint)Marshal.SizeOf<WaveHeader>());
            Winmm.waveOutClose(dev);
        }

        Cleanup();
    }

    private void Cleanup()
    {
        foreach (var h in _bufferHandles)
            if (h.IsAllocated) h.Free();
        if (_headersHandle.IsAllocated) _headersHandle.Free();
        if (_callbackHandle.IsAllocated) _callbackHandle.Free();
    }

    private void OnWaveOutProc(IntPtr hwo, uint uMsg, IntPtr dwInstance, IntPtr dwParam1, IntPtr dwParam2)
    {
        if (uMsg != Winmm.WOM_DONE) return;

        lock (_gate)
        {
            if (_disposed) return;
            var hdr = Marshal.PtrToStructure<WaveHeader>(dwParam1);
            var slot = (int)hdr.dwUser;
            if ((uint)slot >= BufferCount) return;
            _busy[slot] = false;
        }

        ThreadPool.QueueUserWorkItem(static st => ((WaveOutPlayer)st!).RetryDrain(), this);
    }

    private void RetryDrain()
    {
        lock (_gate)
        {
            if (_disposed || _resetting != 0) return;
            Drain();
            if (_pending.Count == 0 && _carry.Count == 0)
            {
                for (var i = 0; i < BufferCount; i++)
                    if (_busy[i]) return;
                PlaybackComplete?.Invoke(this, EventArgs.Empty);
            }
        }
    }

    private void Drain()
    {
        for (var slot = 0; slot < BufferCount; slot++)
        {
            if (_busy[slot]) continue;
            if (_device == IntPtr.Zero) return;
            if (!TryFill(slot)) return;

            _headers[slot].dwFlags &= ~Winmm.WHDR_DONE;
            if (Winmm.waveOutWrite(_device, ref _headers[slot], (uint)Marshal.SizeOf<WaveHeader>()) == 0)
                _busy[slot] = true;
        }
    }

    private bool TryFill(int slot)
    {
        var dst = _buffers[slot];
        var pos = 0;

        if (_carry.Count > 0)
        {
            var n = Math.Min(_bufferSize, _carry.Count);
            _carry.CopyTo(0, dst, 0, n);
            _carry.RemoveRange(0, n);
            pos = n;
        }

        while (pos < _bufferSize && _pending.Count > 0)
        {
            var chunk = _pending.Dequeue();
            var n = Math.Min(_bufferSize - pos, chunk.Length);
            Array.Copy(chunk, 0, dst, pos, n);
            pos += n;
            if (chunk.Length > n)
            {
                var rest = new byte[chunk.Length - n];
                Array.Copy(chunk, n, rest, 0, rest.Length);
                _carry.AddRange(rest);
            }
        }

        if (pos == 0) return false;
        _headers[slot].dwBufferLength = (uint)pos;
        return true;
    }
}
