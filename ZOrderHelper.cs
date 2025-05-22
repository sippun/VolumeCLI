using System;
using System.Runtime.InteropServices;

public static class ZOrderHelper
{
    [DllImport("user32.dll")]
    private static extern IntPtr GetTopWindow(IntPtr hWnd);

    [DllImport("user32.dll")]
    private static extern IntPtr GetWindow(IntPtr hWnd, uint uCmd);
    private const uint GW_HWNDNEXT = 2;

    [DllImport("user32.dll")]
    private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter,
        int X, int Y, int cx, int cy, uint uFlags);

    private const uint SWP_NOMOVE = 0x0002;
    private const uint SWP_NOSIZE = 0x0001;
    private const uint SWP_NOACTIVATE = 0x0010;

    public static void RestoreZOrderAfterAction(IntPtr targetHwnd, Action action)
    {
        if (targetHwnd == IntPtr.Zero || action == null)
            return;

        IntPtr belowHwnd = GetWindowBelow(targetHwnd);

        action.Invoke(); // Perform whatever we bring the window to front for

        // Push it back behind the previous window
        if (belowHwnd != IntPtr.Zero && belowHwnd != targetHwnd)
        {
            SetWindowPos(targetHwnd, belowHwnd, 0, 0, 0, 0,
                SWP_NOMOVE | SWP_NOSIZE | SWP_NOACTIVATE);
        }
    }

    private static IntPtr GetWindowBelow(IntPtr hwnd)
    {
        IntPtr current = GetTopWindow(IntPtr.Zero);
        IntPtr previous = IntPtr.Zero;

        while (current != IntPtr.Zero)
        {
            if (current == hwnd)
                return previous;
            previous = current;
            current = GetWindow(current, GW_HWNDNEXT);
        }

        return IntPtr.Zero;
    }
}
