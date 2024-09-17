using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;

namespace MSFSPopoutPanelManager.WindowsAgent
{
    internal static class PInvokeConstant
    {
        public const int SW_SHOWNORMAL = 1;
        public const int SW_SHOWMINIMIZED = 2;
        public const int SW_SHOWMAXIMIZED = 3;
        public const int SW_SHOW = 5;
        public const int SW_MINIMIZE = 6;
        public const int SW_RESTORE = 9;

        public const uint EVENT_SYSTEM_CAPTURESTART = 0x0008;
        public const uint EVENT_SYSTEM_CAPTUREEND = 0x0009;
        public const uint EVENT_OBJECT_STATECHANGE = 0x800A;
        public const uint EVENT_SYSTEM_MOVESIZEEND = 0x000B;

        public const int SWP_NOMOVE = 0x0002;
        public const int SWP_NOSIZE = 0x0001;
        public const int SWP_ALWAYS_ON_TOP = SWP_NOMOVE | SWP_NOSIZE;

        public const int GWL_STYLE = -16;
        public const uint WS_SIZEBOX = 0x00040000;
        public const uint WS_BORDER = 0x00800000;
        public const uint WS_DLGFRAME = 0x00400000;
        public const uint WS_CAPTION = WS_BORDER | WS_DLGFRAME;

        public const int HWND_TOPMOST = -1;
        public const int HWND_NOTOPMOST = -2;

        public const uint WM_CLOSE = 0x0010;
        public const int WINEVENT_OUTOFCONTEXT = 0;
    }

    public class PInvoke
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr OpenProcess(int dwDesiredAccess, bool bInheritHandle, int dwProcessId);

        [DllImport("ntdll.dll", SetLastError = true)]
        public static extern IntPtr NtWriteVirtualMemory(int processHandle, long baseAddress, byte[] buffer, int numberOfBytesToWrite, out int numberOfBytesWritten);

        [DllImport("ntdll.dll", SetLastError = true)]
        public static extern bool NtReadVirtualMemory(int processHandle, long baseAddress, byte[] buffer, int numberOfBytesToRead, out int numberOfBytesRead);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern int EnumWindows(CallBack callback, int lParam);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int GetClassName(IntPtr hWnd, StringBuilder strPtrClassName, Int32 nMaxCount);

        public static string GetClassName(IntPtr hWnd)
        {
            var sb = new StringBuilder(255);
            GetClassName(hWnd, sb, sb.Capacity);
            return sb.ToString();
        }

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool GetCursorPos(out Point lpPoint);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern uint GetWindowThreadProcessId(IntPtr hWnd, IntPtr processId);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern uint GetCurrentThreadId();

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool AttachThreadInput(uint idAttach, uint idAttachTo, bool fAttach);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool BringWindowToTop(IntPtr hWnd);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetWindowPlacement(IntPtr hWnd, ref WINDOWPLACEMENT lpWndPl);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern int GetWindowRect(IntPtr hWnd, out Rectangle lpRect);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int GetWindowText(IntPtr hWnd, StringBuilder lpWindowText, int nMaxCount);

        public static string GetWindowText(IntPtr hWnd)
        {
            try
            {
                var sb = new StringBuilder(255);
                GetWindowText(hWnd, sb, sb.Capacity);
                return sb.ToString();
            }
            catch { return string.Empty; }
        }


        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool MoveWindow(IntPtr hWnd, int x, int y, int width, int height, bool repaint);
        

        [DllImport("user32.dll", SetLastError = true)]
        public static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, int dwExtraInfo);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern void mouse_event(uint dwFlags, int dx, int dy, uint cButtons, uint dwExtraInfo);

        [DllImport("User32.dll", SetLastError = true)]
        public static extern bool SetCursorPos(int x, int y);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr SetFocus(IntPtr hWnd);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr SendMessage(IntPtr hWnd, UInt32 msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool ShowWindowAsync(HandleRef hWnd, int nCmdShow);

        [DllImport("USER32.dll", SetLastError = true)]
        public static extern int SetWindowLong(IntPtr hWnd, int nIndex, uint dwNewLong);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy, uint wFlags);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool SetWindowText(IntPtr hWnd, string lpString);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr SetWinEventHook(uint eventMin, uint eventMax, IntPtr hModWinEventProc, WinEventProc lpFnWinEventProc, int idProcess, int idThread, uint dwFlags);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern void SwitchToThisWindow(IntPtr hWnd, bool turnOn);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern int UnhookWinEvent(IntPtr hWinEventHook);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern IntPtr GetModuleHandle(string lpModuleName);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr SetWindowsHookEx(HookType hookType, WindowsHookExProc lpFn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool UnhookWindowsHookEx(IntPtr hook);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern int CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        public delegate int WindowsHookExProc(int code, IntPtr wParam, IntPtr lParam);

        public delegate bool CallBack(IntPtr hWnd, int lParam);

        public delegate void WinEventProc(IntPtr hWinEventHook, uint iEvent, IntPtr hWnd, int idObject, int idChild, int dwEventThread, int dwmsEventTime);


        [DllImport("dwmapi.dll", SetLastError = true)]
        public static extern int DwmGetWindowAttribute(IntPtr hWnd, int dwAttribute, out RECT pvAttribute, int cbAttribute);

        [DllImport("dwmapi.dll", PreserveSig = true)]
        public static extern int DwmSetWindowAttribute(IntPtr hWnd, DwmWindowAttribute attr, ref int attrValue, int attrSize);

        public static Rectangle GetWindowRectShadow(IntPtr handle)
        {
            var excludeShadow = GetWindowRectangleDwm(handle);
            GetWindowRect(handle, out var includeShadow);

            var left = includeShadow.Left - excludeShadow.Left;
            var right = includeShadow.Width - excludeShadow.Right;
            var top = includeShadow.Top - excludeShadow.Top;
            var bottom = includeShadow.Height - excludeShadow.Bottom;
            var width = right - left;
            var height = bottom - top;

            return new Rectangle(left, top, width, height);
        }

        internal static Rectangle GetWindowRectangleDwm(IntPtr hWnd)
        {
            var size = Marshal.SizeOf(typeof(RECT));
            DwmGetWindowAttribute(hWnd, (int)DwmWindowAttribute.DWMWA_EXTENDED_FRAME_BOUNDS, out var rect, size);

            return new Rectangle(rect.Left, rect.Top, rect.Right - rect.Left, rect.Bottom - rect.Top);
        }

        public static IntPtr GetWindowHandle(string windowCaption)
        {
            IntPtr windowHandle = IntPtr.Zero;

            EnumWindows((hwnd, _) =>
            {
                var caption = GetWindowText(hwnd);

                if (caption == windowCaption)
                    windowHandle = hwnd;

                return true;
            }, 0);

            return windowHandle;
        }
    }

    public struct RECT
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
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

    [Flags]
    public enum DwmWindowAttribute : uint
    {
        DWMWA_NCRENDERING_ENABLED = 1,
        DWMWA_NCRENDERING_POLICY,
        DWMWA_TRANSITIONS_FORCEDISABLED,
        DWMWA_ALLOW_NCPAINT,
        DWMWA_CAPTION_BUTTON_BOUNDS,
        DWMWA_NONCLIENT_RTL_LAYOUT,
        DWMWA_FORCE_ICONIC_REPRESENTATION,
        DWMWA_FLIP3D_POLICY,
        DWMWA_EXTENDED_FRAME_BOUNDS,
        DWMWA_HAS_ICONIC_BITMAP,
        DWMWA_DISALLOW_PEEK,
        DWMWA_EXCLUDED_FROM_PEEK,
        DWMWA_CLOAK,
        DWMWA_CLOAKED,
        DWMWA_FREEZE_REPRESENTATION,
        DWMWA_PASSIVE_UPDATE_MODE,
        DWMWA_USE_HOSTBACKDROPBRUSH,
        DWMWA_USE_IMMERSIVE_DARK_MODE = 20,
        DWMWA_WINDOW_CORNER_PREFERENCE = 33,
        DWMWA_BORDER_COLOR,
        DWMWA_CAPTION_COLOR,
        DWMWA_TEXT_COLOR,
        DWMWA_VISIBLE_FRAME_BORDER_THICKNESS,
        DWMWA_SYSTEMBACKDROP_TYPE,
        DWMWA_LAST
    }
}
