using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

public enum AppCommand
{
    PlayPause = 14,
    NextTrack = 11,
    PreviousTrack = 12,
    Stop = 13,
    Mute = 8,
    VolumeDown = 9,
    VolumeUp = 10
}

public static class AppCommandSender
{
    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool PostMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

    private const uint WM_APPCOMMAND = 0x0319;

    public static bool Send(string processName, AppCommand command)
    {
        var process = Process.GetProcessesByName(processName).FirstOrDefault();
        if (process == null || process.MainWindowHandle == IntPtr.Zero)
        {
            Console.WriteLine($"Process '{processName}' not found or has no window.");
            return false;
        }

        IntPtr hwnd = process.MainWindowHandle;
        IntPtr lParam = (IntPtr)((int)command << 16);

        return PostMessage(hwnd, WM_APPCOMMAND, hwnd, lParam);
    }
}
