using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;

namespace MSFSPopoutPanelManager.WindowsAgent
{
    public static class PInvokeConstant
    {
        public const int SW_SHOWNORMAL = 1;
        public const int SW_SHOWMINIMIZED = 2;
        public const int SW_SHOWMAXIMIZED = 3;
        public const int SW_SHOW = 5;
        public const int SW_SHOWDEFAULT = 10;
        public const int SW_NORMAL = 1;
        public const int SW_MINIMIZE = 6;
        public const int SW_RESTORE = 9;

        public const uint EVENT_SYSTEM_MOVESIZEEND = 0x000B;
        public const uint EVENT_OBJECT_LOCATIONCHANGE = 0x800B;

        public const int SWP_NOMOVE = 0x0002;
        public const int SWP_NOSIZE = 0x0001;
        public const int SWP_FRAMECHANGED = 0x0020;
        public const int SWP_NOREDRAW = 0x0008;
        public const int SWP_ASYNCWINDOWPOS = 0x4000;
        public const int SWP_ALWAYS_ON_TOP = SWP_NOMOVE | SWP_NOSIZE;

        public const int GWL_STYLE = -16;
        public const int GWL_EXSTYLE = -20;
        public const uint WS_SIZEBOX = 0x00040000;
        public const uint WS_BORDER = 0x00800000;
        public const uint WS_DLGFRAME = 0x00400000;
        public const uint WS_CAPTION = WS_BORDER | WS_DLGFRAME;
        public const uint WS_POPUP = 0x80000000;
        public const uint WS_EX_DLGMODALFRAME = 0x00000001;
        public const uint WS_THICKFRAME = 0x00040000;
        public const uint WS_MAXIMIZEBOX = 0x10000;
        public const uint WS_MINIMIZEBOX = 0x20000;

        public const int HWND_TOPMOST = -1;
        public const int HWND_NOTOPMOST = -2;

        public const uint WM_CLOSE = 0x0010;
        public const int WINEVENT_OUTOFCONTEXT = 0;
    }

    public class PInvoke
    {
        [DllImport("user32", SetLastError = true)]
        public static extern int EnumWindows(CallBack callback, int lParam);

        [DllImport("user32", SetLastError = true)]
        public static extern bool EnumChildWindows(IntPtr window, CallBack callback, IntPtr lParam);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll", EntryPoint = "FindWindow", SetLastError = true)]
        public static extern IntPtr FindWindowByCaption(IntPtr ZeroOnly, string lpWindowName);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int GetClassName(IntPtr hwnd, StringBuilder strPtrClassName, Int32 nMaxCount);

        public static string GetClassName(IntPtr hwnd)
        {
            StringBuilder sb = new StringBuilder(255);
            GetClassName(hwnd, sb, sb.Capacity);
            return sb.ToString();
        }

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool GetClientRect(IntPtr hwnd, out Rectangle lpRect);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool GetCursorPos(out Point lpPoint);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern short GetAsyncKeyState(uint nVirtKey);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr GetWindowLong(IntPtr hwnd, int nIndex);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetWindowPlacement(IntPtr hwnd, ref WINDOWPLACEMENT lpwndpl);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern int GetWindowRect(IntPtr hwnd, out Rectangle lpRect);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern int GetWindowText(IntPtr hwnd, StringBuilder lpWindowText, int nMaxCount);

        public static string GetWindowText(IntPtr hwnd)
        {
            StringBuilder sb = new StringBuilder(255);
            GetWindowText(hwnd, sb, sb.Capacity);
            return sb.ToString();
        }

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool IsWindow(IntPtr hwnd);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, int dwExtraInfo);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern void mouse_event(uint dwFlags, int dx, int dy, uint cButtons, uint dwExtraInfo);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool MoveWindow(IntPtr hwnd, int x, int y, int width, int height, bool repaint);

        [DllImport("User32.dll", SetLastError = true)]
        public static extern bool SetCursorPos(int X, int Y);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr SetFocus(IntPtr hwnd);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool SetForegroundWindow(IntPtr hwnd);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr SendMessage(IntPtr hwnd, UInt32 Msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        public static extern int ShowCursor(bool bShow);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool ShowWindowAsync(HandleRef hwnd, int nCmdShow);

        [DllImport("USER32.dll", SetLastError = true)]
        public static extern int SetWindowLong(IntPtr hwnd, int nIndex, uint dwNewLong);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr SetWindowPos(IntPtr hwnd, IntPtr hwndInsertAfter, int x, int y, int cx, int cy, uint wFlags);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool SetWindowText(System.IntPtr hwnd, System.String lpString);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr SetWinEventHook(uint eventMin, uint eventMax, IntPtr hmodWinEventProc, WinEventProc lpfnWinEventProc, int idProcess, int idThread, uint dwflags);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool ShowWindow(IntPtr hwnd, int nCmdShow);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern int UnhookWinEvent(IntPtr hWinEventHook);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern IntPtr GetModuleHandle(string lpModuleName);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr SetWindowsHookEx(HookType hookType, WindowsHookExProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool UnhookWindowsHookEx(IntPtr hook);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern int CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        public delegate int WindowsHookExProc(int code, IntPtr wParam, IntPtr lParam);

        public delegate bool CallBack(IntPtr hwnd, int lParam);

        public delegate void WinEventProc(IntPtr hWinEventHook, uint iEvent, IntPtr hwnd, int idObject, int idChild, int dwEventThread, int dwmsEventTime);
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct WINDOWPLACEMENT
    {
        public int length;
        public int flags;
        public int showCmd;
        public Point ptMinPosition;
        public Point ptMaxPosition;
        public Rectangle rcNormalPosition;
        public Rectangle rcDevice;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct MSLLHOOKSTRUCT
    {
        public Point pt;
        public int mouseData;
        public int flags;
        public int time;
        public UIntPtr dwExtraInfo;
    }

    public enum HookType : int
    {
        WH_GETMESSAGE = 3,
        WH_MOUSE = 7,
        WH_MOUSE_LL = 14
    }
}
