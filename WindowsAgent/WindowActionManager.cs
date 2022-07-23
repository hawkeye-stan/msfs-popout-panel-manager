using MSFSPopoutPanelManager.Shared;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;

namespace MSFSPopoutPanelManager.WindowsAgent
{
    public class WindowActionManager
    {
        public static event EventHandler<bool> OnPopOutManagerAlwaysOnTopChanged;

        public static void ApplyHidePanelTitleBar(IntPtr hwnd, bool hideTitleBar)
        {
            var currentStyle = PInvoke.GetWindowLong(hwnd, PInvokeConstant.GWL_STYLE).ToInt64();

            if (hideTitleBar)
                PInvoke.SetWindowLong(hwnd, PInvokeConstant.GWL_STYLE, (uint)(currentStyle & ~(PInvokeConstant.WS_CAPTION | PInvokeConstant.WS_SIZEBOX)));
            else
                PInvoke.SetWindowLong(hwnd, PInvokeConstant.GWL_STYLE, (uint)(currentStyle | (PInvokeConstant.WS_CAPTION | PInvokeConstant.WS_SIZEBOX)));
        }

        public static void ApplyAlwaysOnTop(IntPtr hwnd, PanelType panelType, bool alwaysOnTop, Rectangle panelRectangle)
        {
            // Override weird size adjustment for Touch Panel WPF window
            int newWidth = panelType == PanelType.MSFSTouchPanel ? panelRectangle.Width - 16 : panelRectangle.Width;
            int newHeight = panelType == PanelType.MSFSTouchPanel ? panelRectangle.Height - 39 : panelRectangle.Height;

            if (panelType == PanelType.PopOutManager)
            {
                OnPopOutManagerAlwaysOnTopChanged?.Invoke(null, alwaysOnTop);
            }
            else
            {
                if (alwaysOnTop)
                    PInvoke.SetWindowPos(hwnd, new IntPtr(PInvokeConstant.HWND_TOPMOST), panelRectangle.Left, panelRectangle.Top, newWidth, newHeight, PInvokeConstant.SWP_ALWAYS_ON_TOP);
                else
                    PInvoke.SetWindowPos(hwnd, new IntPtr(PInvokeConstant.HWND_NOTOPMOST), panelRectangle.Left, panelRectangle.Top, newWidth, newHeight, 0);
            }
        }

        public static void ApplyAlwaysOnTop(IntPtr hwnd, PanelType panelType, bool alwaysOnTop)
        {
            Rectangle rect;
            PInvoke.GetWindowRect(hwnd, out rect);

            Rectangle clientRectangle;
            PInvoke.GetClientRect(hwnd, out clientRectangle);

            ApplyAlwaysOnTop(hwnd, panelType, alwaysOnTop, new Rectangle(rect.X, rect.Y, clientRectangle.Width, clientRectangle.Height));
        }

        public static void CloseWindow(IntPtr hwnd)
        {
            PInvoke.SendMessage(hwnd, PInvokeConstant.WM_CLOSE, IntPtr.Zero, IntPtr.Zero);
        }

        public static void MoveWindow(IntPtr hwnd, int x, int y)
        {
            Rectangle rectangle;
            PInvoke.GetClientRect(hwnd, out rectangle);
            PInvoke.MoveWindow(hwnd, x, y, rectangle.Width, rectangle.Height, true);
        }

        public static void MoveWindow(IntPtr hwnd, PanelType panelType, int x, int y, int width, int height)
        {
            // Override weird size adjustment for Touch Panel WPF window
            int newWidth = panelType == PanelType.MSFSTouchPanel ? width - 16 : width;
            int newHeight = panelType == PanelType.MSFSTouchPanel ? height - 39 : height;

            PInvoke.MoveWindow(hwnd, x, y, newWidth, newHeight, true);
        }

        public static void MoveWindowWithMsfsBugOverrirde(IntPtr hwnd, PanelType panelType, int x, int y, int width, int height)
        {
            int originalX = x;

            // Override weird size adjustment for Touch Panel WPF window
            int newWidth = panelType == PanelType.MSFSTouchPanel ? width - 16 : width;
            int newHeight = panelType == PanelType.MSFSTouchPanel ? height - 39 : height;

            PInvoke.MoveWindow(hwnd, x, y, newWidth, newHeight, true);

            // Fixed MSFS bug, create workaround where on 2nd or later instance of width adjustment, the panel shift to the left by itself
            // Wait for system to catch up on panel coordinate that were just applied
            System.Threading.Thread.Sleep(200);

            Rectangle rectangle;
            PInvoke.GetWindowRect(hwnd, out rectangle);

            if (rectangle.Left != originalX)
                PInvoke.MoveWindow(hwnd, originalX, y, newWidth, newHeight, false);
        }

        public static void MinimizeWindow(IntPtr hwnd)
        {
            PInvoke.ShowWindow(hwnd, PInvokeConstant.SW_MINIMIZE);
        }

        public static void BringWindowToForeground(IntPtr hwnd)
        {
            PInvoke.ShowWindowAsync(new HandleRef(null, hwnd), PInvokeConstant.SW_RESTORE);
            PInvoke.SetForegroundWindow(hwnd);
        }

        public static IntPtr FindWindowByCaption(string caption)
        {
            return PInvoke.FindWindowByCaption(IntPtr.Zero, caption);
        }

        public static Rectangle GetClientRect(IntPtr hwnd)
        {
            Rectangle rectangle;
            PInvoke.GetClientRect(hwnd, out rectangle);

            return rectangle;
        }

        public static Rectangle GetWindowRect(IntPtr hwnd)
        {
            Rectangle rectangle;
            PInvoke.GetWindowRect(hwnd, out rectangle);

            return rectangle;
        }

        public static bool IsWindow(IntPtr hwnd)
        {
            return PInvoke.IsWindow(hwnd);
        }

        public static int GetWindowsCountByPanelType(List<PanelType> panelTypes)
        {
            var count = 0;

            PInvoke.EnumWindows((IntPtr hwnd, int lParam) =>
            {
                var panelType = GetWindowPanelType(hwnd);

                if (panelTypes.Contains(panelType))
                    count++;

                return true;
            }, 0);

            return count;
        }

        public static PanelType GetWindowPanelType(IntPtr hwnd)
        {
            var className = PInvoke.GetClassName(hwnd);
            var caption = PInvoke.GetWindowText(hwnd);

            if (className == "AceApp")      // MSFS windows designation
            {
                if (String.IsNullOrEmpty(caption) || caption.IndexOf("(Custom)") > -1)        // Pop Out window
                    return PanelType.CustomPopout;
                else if (caption.IndexOf("Microsoft Flight Simulator") > -1)                // MSFS main game window
                    return PanelType.FlightSimMainWindow;
                else if (caption.IndexOf("WINDOW") > -1)                                    // MSFS multi-monitor window
                    return PanelType.MultiMonitorWindow;
                else
                    return PanelType.BuiltInPopout;                                         // MSFS built-in window such as ATC, VFRMap
            }
            else if (className.IndexOf("HwndWrapper[MSFSPopoutPanelManager") > -1)
            {
                if (caption.IndexOf("(Touch Panel)") > -1)
                    return PanelType.MSFSTouchPanel;
                else
                    return PanelType.PopOutManager;
            }

            return PanelType.Unknown;
        }

        public static void CloseAllPopOuts()
        {
            PInvoke.EnumWindows((IntPtr hwnd, int lParam) =>
            {
                var panelType = GetWindowPanelType(hwnd);

                if (panelType == PanelType.CustomPopout || panelType == PanelType.MSFSTouchPanel)
                    WindowActionManager.CloseWindow(hwnd);

                return true;
            }, 0);
        }
    }
}
