using System;
using System.Runtime.InteropServices;
using UnityEngine;

public class Alice : MonoBehaviour { }

public static class Win32 {
    [DllImport("user32.dll")]
    public static extern IntPtr GetForegroundWindow();

    [DllImport("user32.dll", SetLastError = true)]
    public static extern bool SetWindowPos(IntPtr hWnd, int hWndInsertAfter, int x, int y, int cx, int cy, uint uFlags);


    public static void SetAlwaysOnTop(IntPtr hWnd, bool onTop)
        => SetWindowPos(hWnd,
            onTop ? -1 : -2,
            0, 0, 0, 0, 67);

    public static void FocusOn(IntPtr hWnd) {
        SetAlwaysOnTop(hWnd, true);
        SetAlwaysOnTop(hWnd, false);
    }
}
