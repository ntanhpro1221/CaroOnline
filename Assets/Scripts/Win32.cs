#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
using System;
using System.Runtime.InteropServices;
using System.Text;
public class Win32 {
    [DllImport("user32.dll", SetLastError = true)]
    public static extern IntPtr GetForegroundWindow();


    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    public static extern int GetWindowText(IntPtr hwnd, StringBuilder lpString, int nMaxCount);

    public static string GetForegroundWindowText() {
        StringBuilder windowText = new(2048);
        GetWindowText(GetForegroundWindow(), windowText, windowText.Capacity);
        return windowText.ToString();
    }
}
#endif
