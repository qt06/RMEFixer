using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace RMEFixer;

[SupportedOSPlatform("windows")]
internal static partial class Win32
{
    private const string U = "user32.dll";
    private const string S = "shell32.dll";

    [LibraryImport(U, EntryPoint = "RegisterClassW")]
    internal static partial short RegisterClassW(ref WNDCLASSW lpWndClass);
    [LibraryImport(U, EntryPoint = "CreateWindowExW")]
    internal static partial IntPtr CreateWindowExW(
        uint dwExStyle, IntPtr lpClassName, IntPtr lpWindowName,
        uint dwStyle, int x, int y, int nWidth, int nHeight,
        IntPtr hWndParent, IntPtr hMenu, IntPtr hInstance, IntPtr lpParam);
    [LibraryImport(U, EntryPoint = "DestroyWindow")]
    internal static partial int DestroyWindow(IntPtr hWnd);
    [LibraryImport(U, EntryPoint = "DefWindowProcW")]
    internal static partial IntPtr DefWindowProcW(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);
    [LibraryImport(U, EntryPoint = "PostQuitMessage")]
    internal static partial void PostQuitMessage(int nExitCode);
    [LibraryImport(U, EntryPoint = "GetMessageW")]
    internal static partial int GetMessageW(out MSG lpMsg, IntPtr hWnd, uint wMsgFilterMin, uint wMsgFilterMax);
    [LibraryImport(U, EntryPoint = "TranslateMessage")]
    internal static partial int TranslateMessage(ref MSG lpMsg);
    [LibraryImport(U, EntryPoint = "DispatchMessageW")]
    internal static partial IntPtr DispatchMessageW(ref MSG lpMsg);
    [LibraryImport(U, EntryPoint = "RegisterHotKey")]
    internal static partial int RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);
    [LibraryImport(U, EntryPoint = "UnregisterHotKey")]
    internal static partial int UnregisterHotKey(IntPtr hWnd, int id);
    [LibraryImport(U, EntryPoint = "CreatePopupMenu")]
    internal static partial IntPtr CreatePopupMenu();
    [LibraryImport(U, EntryPoint = "DestroyMenu")]
    internal static partial int DestroyMenu(IntPtr hMenu);
    [LibraryImport(U, EntryPoint = "AppendMenuW")]
    internal static partial int AppendMenuW(IntPtr hMenu, uint uFlags, uint uIDNewItem, IntPtr lpNewItem);
    [LibraryImport(U, EntryPoint = "TrackPopupMenu")]
    internal static partial int TrackPopupMenu(IntPtr hMenu, uint uFlags, int x, int y, int nReserved, IntPtr hWnd, IntPtr prcRect);
    [LibraryImport(U, EntryPoint = "GetCursorPos")]
    internal static partial int GetCursorPos(out POINT lpPoint);
    [LibraryImport(U, EntryPoint = "SetForegroundWindow")]
    internal static partial int SetForegroundWindow(IntPtr hWnd);
    [LibraryImport(U, EntryPoint = "PostMessageW")]
    internal static partial int PostMessageW(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);
    [LibraryImport(U, EntryPoint = "LoadImageW")]
    internal static partial IntPtr LoadImageW(IntPtr hInst, IntPtr name, uint type, int cx, int cy, uint fuLoad);
    [LibraryImport(U, EntryPoint = "DestroyIcon")]
    internal static partial int DestroyIcon(IntPtr hIcon);

    [LibraryImport(S, EntryPoint = "Shell_NotifyIconW")]
    internal static partial int Shell_NotifyIconW(uint dwMessage, ref NOTIFYICONDATAW lpData);

    [LibraryImport("kernel32.dll", EntryPoint = "GetModuleHandleW")]
    internal static partial IntPtr GetModuleHandleW(IntPtr lpModuleName);

    [LibraryImport(U, EntryPoint = "GetForegroundWindow")]
    internal static partial IntPtr GetForegroundWindow();

    internal const uint IMAGE_ICON = 1;
    internal const uint LR_DEFAULTCOLOR = 0;
    internal const int IDI_APPLICATION = 32512;

    internal const uint WS_EX_TOOLWINDOW = 0x00000080;
    internal const uint WS_EX_NOACTIVATE = 0x08000000;
    internal const uint WS_OVERLAPPED = 0x00000000;
    internal const int CW_USEDEFAULT = unchecked((int)0x80000000);

    internal const uint NIM_ADD = 0;
    internal const uint NIM_DELETE = 2;
    internal const uint NIM_SETVERSION = 4;
    internal const uint NIF_MESSAGE = 0x00000001;
    internal const uint NIF_ICON = 0x00000002;
    internal const uint NIF_TIP = 0x00000004;
    internal const uint NOTIFYICON_VERSION = 3;

    internal const int WM_DESTROY = 0x0002;
    internal const int WM_COMMAND = 0x0111;
    internal const int WM_HOTKEY = 0x0312;
    internal const int WM_APP = 0x8000;
    internal const int WM_USER = 0x0400;
    internal const int WM_NULL = 0x0000;
    internal const int WM_CONTEXTMENU = 0x007B;
    internal const int WM_LBUTTONDOWN = 0x0201;
    internal const int WM_RBUTTONDOWN = 0x0204;

    internal const uint MF_STRING = 0x00000000;
    internal const uint TPM_LEFTALIGN = 0x0000;
    internal const uint TPM_BOTTOMALIGN = 0x0020;
    internal const uint TPM_LEFTBUTTON = 0x0000;

    internal const uint MOD_ALT = 0x0001;
    internal const uint MOD_CONTROL = 0x0002;
    internal const uint MOD_WIN = 0x0008;
    internal const uint VK_F10 = 0x79;

    internal const int HOTKEY_ID = 1;
    internal const uint ID_MUTE = 1001;
    internal const uint ID_EXIT = 1002;
}

[StructLayout(LayoutKind.Sequential)]
internal struct WNDCLASSW
{
    public uint style;
    public IntPtr lpfnWndProc;
    public int cbClsExtra;
    public int cbWndExtra;
    public IntPtr hInstance;
    public IntPtr hIcon;
    public IntPtr hCursor;
    public IntPtr hbrBackground;
    public IntPtr lpszMenuName;
    public IntPtr lpszClassName;
}

[StructLayout(LayoutKind.Sequential)]
internal struct POINT
{
    public int x, y;
}

[StructLayout(LayoutKind.Sequential)]
internal struct MSG
{
    public IntPtr hwnd;
    public uint message;
    public IntPtr wParam;
    public IntPtr lParam;
    public uint time;
    public POINT pt;
}

[StructLayout(LayoutKind.Sequential)]
internal unsafe struct NOTIFYICONDATAW
{
    public uint cbSize;
    public IntPtr hWnd;
    public uint uID;
    public uint uFlags;
    public uint uCallbackMessage;
    public IntPtr hIcon;
    public fixed char szTip[128];
    public uint dwState;
    public uint dwStateMask;
    public fixed char szInfo[256];
    public uint uVersionOrTimeout;
    public fixed char szInfoTitle[64];
    public uint dwInfoFlags;
    public Guid guidItem;
}

internal delegate IntPtr WndProcDelegate(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

[SupportedOSPlatform("windows")]
internal static class App
{
    internal static IntPtr HWnd;
    internal static IntPtr HIcon;
    internal static IntPtr HMenu;
    internal static WndProcDelegate? WndProcCallback;

    internal static byte[] Wav1 = null!;
    internal static byte[] Wav2 = null!;
    internal static byte[] Wav3 = null!;
    internal static WavFormat Format;
    internal static HotkeyBinding Hotkey;
    internal static volatile bool IsExecuting;

    internal static readonly object PlayersLock = new();
    internal static readonly List<WaveOutPlayer> ActivePlayers = [];

    internal static readonly char[] MenuTextMute = "修复残响\0".ToCharArray();
    internal static readonly char[] MenuTextExit = "退出\0".ToCharArray();
    internal static readonly char[] WindowClass = "RMEFixerWin\0".ToCharArray();
    internal static readonly char[] TipText = "RME 声卡残响修复工具\0".ToCharArray();

    internal static void TrackPlayer(WaveOutPlayer player)
    {
        lock (PlayersLock) ActivePlayers.Add(player);
    }

    internal static void UntrackPlayer(WaveOutPlayer player)
    {
        lock (PlayersLock) ActivePlayers.Remove(player);
    }
}

[SupportedOSPlatform("windows")]
internal static class Program
{
    [STAThread]
    static unsafe void Main()
    {
        var currentPid = Environment.ProcessId;
        foreach (var p in System.Diagnostics.Process.GetProcessesByName("RMEFixer"))
        {
            if (p.Id != currentPid)
            {
                try { p.Kill(); p.WaitForExit(3000); } catch { }
            }
        }

        var soundDir = Path.Combine(AppContext.BaseDirectory, "sound");
        App.Wav1 = WavLoader.Load(Path.Combine(soundDir, "1-开始.wav"), out var fmt1);
        App.Wav2 = WavLoader.Load(Path.Combine(soundDir, "2-静音.wav"), out var _);
        App.Wav3 = WavLoader.Load(Path.Combine(soundDir, "3-完成.wav"), out var _);
        App.Format = fmt1;

        App.WndProcCallback = WndProc;
        App.Hotkey = ConfigManager.Load();
        var hInst = Win32.GetModuleHandleW(IntPtr.Zero);

        fixed (char* clsName = App.WindowClass)
        {
            var wc = new WNDCLASSW
            {
                lpfnWndProc = Marshal.GetFunctionPointerForDelegate(App.WndProcCallback),
                hInstance = hInst,
                lpszClassName = (IntPtr)clsName,
            };
            Win32.RegisterClassW(ref wc);
        }

        fixed (char* clsName = App.WindowClass)
        {
            App.HWnd = Win32.CreateWindowExW(
                Win32.WS_EX_TOOLWINDOW | Win32.WS_EX_NOACTIVATE,
                (IntPtr)clsName, IntPtr.Zero,
                Win32.WS_OVERLAPPED,
                0, 0, 0, 0,
                IntPtr.Zero, IntPtr.Zero, hInst, IntPtr.Zero);
        }

        if (App.HWnd == IntPtr.Zero)
            return;

        Win32.RegisterHotKey(App.HWnd, Win32.HOTKEY_ID,
            App.Hotkey.Modifiers, App.Hotkey.VirtualKey);

        SetupTrayIcon();
        SetupMenu();

        PlayOnDefaultDevice(App.Wav1);

        var msg = new MSG();
        while (Win32.GetMessageW(out msg, IntPtr.Zero, 0, 0) > 0)
        {
            Win32.TranslateMessage(ref msg);
            Win32.DispatchMessageW(ref msg);
        }

        Cleanup();
    }

    private static void SetupTrayIcon()
    {
        App.HIcon = Win32.LoadImageW(IntPtr.Zero, (IntPtr)Win32.IDI_APPLICATION,
            Win32.IMAGE_ICON, 16, 16, Win32.LR_DEFAULTCOLOR);

        var nid = new NOTIFYICONDATAW();
        nid.cbSize = (uint)Marshal.SizeOf<NOTIFYICONDATAW>();
        nid.hWnd = App.HWnd;
        nid.uID = 1;
        nid.uFlags = Win32.NIF_MESSAGE | Win32.NIF_ICON | Win32.NIF_TIP;
        nid.uCallbackMessage = Win32.WM_APP + 1;
        nid.hIcon = App.HIcon;

        unsafe
        {
            fixed (char* p = App.TipText)
                Buffer.MemoryCopy(p, nid.szTip, 256, (App.TipText.Length) * 2);
        }

        Win32.Shell_NotifyIconW(Win32.NIM_ADD, ref nid);
    }

    private static void SetupMenu()
    {
        App.HMenu = Win32.CreatePopupMenu();

        unsafe
        {
            fixed (char* p = App.MenuTextMute)
                Win32.AppendMenuW(App.HMenu, Win32.MF_STRING, Win32.ID_MUTE, (IntPtr)p);
            fixed (char* p = App.MenuTextExit)
                Win32.AppendMenuW(App.HMenu, Win32.MF_STRING, Win32.ID_EXIT, (IntPtr)p);
        }
    }

    private static void Cleanup()
    {
        Win32.UnregisterHotKey(App.HWnd, Win32.HOTKEY_ID);

        if (App.HMenu != IntPtr.Zero)
        {
            Win32.DestroyMenu(App.HMenu);
            App.HMenu = IntPtr.Zero;
        }

        if (App.HWnd != IntPtr.Zero)
        {
            var nid = new NOTIFYICONDATAW();
            nid.cbSize = (uint)Marshal.SizeOf<NOTIFYICONDATAW>();
            nid.hWnd = App.HWnd;
            nid.uID = 1;
            Win32.Shell_NotifyIconW(Win32.NIM_DELETE, ref nid);
        }

        if (App.HIcon != IntPtr.Zero)
        {
            Win32.DestroyIcon(App.HIcon);
            App.HIcon = IntPtr.Zero;
        }

        if (App.HWnd != IntPtr.Zero)
        {
            Win32.DestroyWindow(App.HWnd);
            App.HWnd = IntPtr.Zero;
        }

        lock (App.PlayersLock)
        {
            foreach (var p in App.ActivePlayers)
                p.Dispose();
            App.ActivePlayers.Clear();
        }
    }

    private static IntPtr WndProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam)
    {
        switch (msg)
        {
            case Win32.WM_DESTROY:
                Win32.PostQuitMessage(0);
                return IntPtr.Zero;

            case Win32.WM_HOTKEY:
                if ((int)wParam == Win32.HOTKEY_ID)
                    ExecuteMute();
                return IntPtr.Zero;

            case Win32.WM_COMMAND:
                var id = (uint)(int)wParam;
                if (id == Win32.ID_MUTE)
                    ExecuteMute();
                else if (id == Win32.ID_EXIT)
                    Win32.PostMessageW(App.HWnd, Win32.WM_DESTROY, IntPtr.Zero, IntPtr.Zero);
                return IntPtr.Zero;

            default:
                if (msg == Win32.WM_APP + 1)
                {
                    var evt = (int)lParam;
                    if (evt == Win32.WM_LBUTTONDOWN ||
                        evt == Win32.WM_RBUTTONDOWN ||
                        evt == Win32.WM_CONTEXTMENU)
                        ShowMenu();
                    return IntPtr.Zero;
                }
                break;
        }

        return Win32.DefWindowProcW(hWnd, msg, wParam, lParam);
    }

    private static void ShowMenu()
    {
        Win32.GetCursorPos(out var pt);
        var prevFg = Win32.GetForegroundWindow();
        Win32.SetForegroundWindow(App.HWnd);
        Win32.TrackPopupMenu(App.HMenu,
            Win32.TPM_LEFTALIGN | Win32.TPM_BOTTOMALIGN | Win32.TPM_LEFTBUTTON,
            pt.x, pt.y, 0, App.HWnd, IntPtr.Zero);
        Win32.PostMessageW(App.HWnd, Win32.WM_NULL, IntPtr.Zero, IntPtr.Zero);
        if (prevFg != IntPtr.Zero && prevFg != App.HWnd)
            Win32.SetForegroundWindow(prevFg);
    }

    private static void ExecuteMute()
    {
        if (App.IsExecuting) return;
        App.IsExecuting = true;

        var devices = WaveOutPlayer.EnumerateDevices()
            .Where(d => !d.IsDefault)
            .ToList();

        if (devices.Count == 0)
        {
            PlayOnDefaultDevice(App.Wav2,
                () => PlayOnDefaultDevice(App.Wav3, () => App.IsExecuting = false));
            return;
        }

        var remaining = devices.Count;
        var players = new List<WaveOutPlayer>(devices.Count);

        foreach (var dev in devices)
        {
            try
            {
                var player = new WaveOutPlayer(
                    App.Format.SampleRate, App.Format.Channels, App.Format.BitsPerSample, dev.Id);

                App.TrackPlayer(player);

                player.PlaybackComplete += (_, _) =>
                {
                    App.UntrackPlayer(player);
                    var count = Interlocked.Decrement(ref remaining);
                    if (count == 0)
                    {
                        var list = players;
                        Task.Run(() =>
                        {
                            foreach (var p in list)
                                if (p != player)
                                    p.Dispose();
                            player.Dispose();
                            PlayOnDefaultDevice(App.Wav3, () => App.IsExecuting = false);
                        });
                    }
                };

                player.Feed(App.Wav2);
                players.Add(player);
            }
            catch (Exception ex)
            {
                var count = Interlocked.Decrement(ref remaining);
                System.Diagnostics.Debug.WriteLine($"设备 {dev.Name} 打开失败: {ex.Message}");

                if (count == 0)
                {
                    if (players.Count > 0)
                    {
                        var list = players;
                        Task.Run(() =>
                        {
                            foreach (var p in list)
                                p.Dispose();
                            PlayOnDefaultDevice(App.Wav3, () => App.IsExecuting = false);
                        });
                    }
                    else
                    {
                        PlayOnDefaultDevice(App.Wav3, () => App.IsExecuting = false);
                    }
                }
            }
        }
    }

    private static void PlayOnDefaultDevice(byte[] pcm, Action? onComplete = null)
    {
        try
        {
            var player = new WaveOutPlayer(
                App.Format.SampleRate, App.Format.Channels, App.Format.BitsPerSample, -1);

            App.TrackPlayer(player);

            player.PlaybackComplete += (_, _) =>
            {
                App.UntrackPlayer(player);
                var p = player;
                Task.Run(() => p.Dispose());
                onComplete?.Invoke();
            };

            player.Feed(pcm);
        }
        catch
        {
            onComplete?.Invoke();
        }
    }
}
