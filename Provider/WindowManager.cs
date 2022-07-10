using MSFSPopoutPanelManager.Model;
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

        public static void ApplyAlwaysOnTop(IntPtr handle, PanelType panelType, bool alwaysOnTop, Rectangle panelRectangle)
        {
            // Override weird size adjustment for Touch Panel WPF window
            int newWidth = panelType == PanelType.MSFSTouchPanel ? panelRectangle.Width - 16 : panelRectangle.Width;
            int newHeight = panelType == PanelType.MSFSTouchPanel ? panelRectangle.Height - 39 : panelRectangle.Height;

            if (alwaysOnTop)
                PInvoke.SetWindowPos(handle, new IntPtr(PInvokeConstant.HWND_TOPMOST), panelRectangle.Left, panelRectangle.Top, newWidth, newHeight, PInvokeConstant.SWP_ALWAYS_ON_TOP);
            else
                PInvoke.SetWindowPos(handle, new IntPtr(PInvokeConstant.HWND_NOTOPMOST), panelRectangle.Left, panelRectangle.Top, newWidth, newHeight, 0);
        }

        public static void ApplyAlwaysOnTop(IntPtr handle, bool alwaysOnTop)
        {
            Rectangle rect;
            PInvoke.GetWindowRect(handle, out rect);

            Rectangle clientRectangle;
            PInvoke.GetClientRect(handle, out clientRectangle);

            ApplyAlwaysOnTop(handle, PanelType.WPFWindow, alwaysOnTop, new Rectangle(rect.X, rect.Y, clientRectangle.Width, clientRectangle.Height));
        }

        public static void CloseWindow(IntPtr handle)
        {
            PInvoke.SendMessage(handle, PInvokeConstant.WM_CLOSE, IntPtr.Zero, IntPtr.Zero);
        }

        public static void MoveWindow(IntPtr handle, int x, int y)
        {
            Rectangle rectangle;
            PInvoke.GetClientRect(handle, out rectangle);
            PInvoke.MoveWindow(handle, x, y, rectangle.Width, rectangle.Height, true);
        }

        public static void MoveWindow(IntPtr handle, PanelType panelType, int x, int y, int width, int height)
        {
            // Override weird size adjustment for Touch Panel WPF window
            int newWidth = panelType == PanelType.MSFSTouchPanel ? width - 16 : width;
            int newHeight = panelType == PanelType.MSFSTouchPanel ? height - 39 : height;

            PInvoke.MoveWindow(handle, x, y, newWidth, newHeight, true);
        }

        public static void MoveWindowWithMsfsBugOverrirde(IntPtr handle, PanelType panelType, int x, int y, int width, int height)
        {
            int originalX = x;

            // Override weird size adjustment for Touch Panel WPF window
            int newWidth = panelType == PanelType.MSFSTouchPanel ? width - 16 : width;
            int newHeight = panelType == PanelType.MSFSTouchPanel ? height - 39 : height;

            PInvoke.MoveWindow(handle, x, y, newWidth, newHeight, true);

            // Fixed MSFS bug, create workaround where on 2nd or later instance of width adjustment, the panel shift to the left by itself
            // Wait for system to catch up on panel coordinate that were just applied
            System.Threading.Thread.Sleep(200);

            Rectangle rectangle;
            PInvoke.GetWindowRect(handle, out rectangle);

            if (rectangle.Left != originalX)
                PInvoke.MoveWindow(handle, originalX, y, newWidth, newHeight, false);
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

        public static IntPtr FindWindowByCaption(string caption)
        {
            return PInvoke.FindWindowByCaption(IntPtr.Zero, caption);
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

            if (className == "AceApp" && caption.IndexOf("WINDOW") > -1)    // For multi monitor window, do nothing
                return true;

            if (className == "AceApp" && (caption.IndexOf("(Custom)") > -1 || caption == String.Empty))      // Only close non-builtin pop out panels
            {
                WindowManager.CloseWindow(hwnd);
            }
            else if (className == "AceApp" && caption.IndexOf("Microsoft Flight Simulator") == -1)    // For builtin pop out  (ATC, VFR Map, ect)
            {
                WindowManager.MoveWindow(hwnd, 0, 0);
            }

            return true;
        }
    }
}
