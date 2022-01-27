using MSFSPopoutPanelManager.Shared;
using System;
using System.Drawing;
using System.Runtime.InteropServices;

namespace MSFSPopoutPanelManager.Provider
{
    public class WindowManager
    {
        public static void ApplyHidePanelTitleBar(IntPtr handle, bool hideTitleBar)
        {
            var currentStyle = PInvoke.GetWindowLong(handle, PInvokeConstant.GWL_STYLE).ToInt64();

            if (hideTitleBar)
                PInvoke.SetWindowLong(handle, PInvokeConstant.GWL_STYLE, (uint)(currentStyle & ~(PInvokeConstant.WS_CAPTION | PInvokeConstant.WS_SIZEBOX)));
            else
                PInvoke.SetWindowLong(handle, PInvokeConstant.GWL_STYLE, (uint)(currentStyle | (PInvokeConstant.WS_CAPTION | PInvokeConstant.WS_SIZEBOX)));
        }

        public static void ApplyAlwaysOnTop(IntPtr handle, bool alwaysOnTop, Rectangle panelRectangle)
        {
            if (alwaysOnTop)
                PInvoke.SetWindowPos(handle, new IntPtr(PInvokeConstant.HWND_TOPMOST), panelRectangle.Left, panelRectangle.Top, panelRectangle.Width, panelRectangle.Height, PInvokeConstant.SWP_ALWAYS_ON_TOP);
            else
                PInvoke.SetWindowPos(handle, new IntPtr(PInvokeConstant.HWND_NOTOPMOST), panelRectangle.Left, panelRectangle.Top, panelRectangle.Width, panelRectangle.Height, 0);
        }

        public static void ApplyAlwaysOnTop(IntPtr handle, bool alwaysOnTop)
        {
            Rectangle rect;
            PInvoke.GetWindowRect(handle, out rect);

            Rectangle clientRectangle;
            PInvoke.GetClientRect(handle, out clientRectangle);

            ApplyAlwaysOnTop(handle, alwaysOnTop, new Rectangle(rect.X, rect.Y, clientRectangle.Width, clientRectangle.Height));
        }

        public static void CloseWindow(IntPtr handle)
        {
            PInvoke.SendMessage(handle, PInvokeConstant.WM_CLOSE, IntPtr.Zero, IntPtr.Zero);
        }

        public static void MoveWindow(IntPtr handle, int x, int y)
        {
            Rectangle rectangle;
            PInvoke.GetClientRect(handle, out rectangle);
            PInvoke.MoveWindow(handle, x, y, rectangle.Width, rectangle.Height, false);
        }

        public static void MinimizeWindow(IntPtr handle)
        {
            PInvoke.ShowWindow(handle, PInvokeConstant.SW_MINIMIZE);
        }

        public static void BringWindowToForeground(IntPtr handle)
        {
            PInvoke.ShowWindowAsync(new HandleRef(null, handle), PInvokeConstant.SW_RESTORE);
            PInvoke.SetForegroundWindow(handle);
        }

        public static void CloseAllCustomPopoutPanels()
        {
            PInvoke.EnumWindows(new PInvoke.CallBack(EnumAllCustomPopoutPanels), 1);
        }

        public static void MinimizeAllPopoutPanels(bool active)
        {
            if (active)
            {
                PInvoke.EnumWindows(new PInvoke.CallBack(EnumToMinimizePopoutPanels), 0);
            }
            else
            {
                PInvoke.EnumWindows(new PInvoke.CallBack(EnumToMinimizePopoutPanels), 1);
            }
        }

        private static bool EnumToMinimizePopoutPanels(IntPtr hwnd, int index)
        {
            var className = PInvoke.GetClassName(hwnd);
            var caption = PInvoke.GetWindowText(hwnd);

            if (className == "AceApp" && caption.IndexOf("Microsoft Flight Simulator") == -1)      // MSFS windows designation
            {
                if (index == 0)
                    PInvoke.ShowWindow(hwnd, PInvokeConstant.SW_MINIMIZE);
                else
                    PInvoke.ShowWindow(hwnd, PInvokeConstant.SW_RESTORE);
            }

            return true;
        }

        private static bool EnumAllCustomPopoutPanels(IntPtr hwnd, int index)
        {
            var className = PInvoke.GetClassName(hwnd);
            var caption = PInvoke.GetWindowText(hwnd);

            if (className == "AceApp" && (caption.IndexOf("(Custom)") > -1 || caption == String.Empty))      // Only close non-builtin pop out panels
            {
                WindowManager.CloseWindow(hwnd);
            }
            else if (className == "AceApp" && caption.IndexOf("Microsoft Flight Simulator") == -1)    // for builtin pop out  (ATC, VFR Map, ect)
            {
                WindowManager.MoveWindow(hwnd, 0, 0);
            }

            return true;
        }
    }
}
