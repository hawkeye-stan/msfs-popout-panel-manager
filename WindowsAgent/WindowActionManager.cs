using MSFSPopoutPanelManager.DomainModel.Profile;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Threading;

namespace MSFSPopoutPanelManager.WindowsAgent
{
    public class WindowActionManager
    {
        public static event EventHandler<bool> OnPopOutManagerAlwaysOnTopChanged;

        public static void ApplyHidePanelTitleBar(IntPtr hwnd, bool hideTitleBar)
        {
            if (hideTitleBar)
            {
                var rect = WindowActionManager.GetWindowRectangle(hwnd);
                var rectShadow = PInvoke.GetWindowRectShadow(hwnd);

                MoveWindow(hwnd, rect.Left - rectShadow.Left, rect.Top, rect.Width - rectShadow.Width, rect.Height - rectShadow.Height);
                Thread.Sleep(250);
            }

            var currentStyle = (uint)PInvoke.GetWindowLong(hwnd, PInvokeConstant.GWL_STYLE);

            if (hideTitleBar)
                currentStyle &= ~(PInvokeConstant.WS_CAPTION | PInvokeConstant.WS_SIZEBOX);
            else
                currentStyle |= (PInvokeConstant.WS_CAPTION | PInvokeConstant.WS_SIZEBOX);

            PInvoke.SetWindowLong(hwnd, PInvokeConstant.GWL_STYLE, currentStyle);
            Thread.Sleep(250);

            if (!hideTitleBar)
            {
                var rect = WindowActionManager.GetWindowRectangle(hwnd);
                var rectShadow = PInvoke.GetWindowRectShadow(hwnd);
                MoveWindow(hwnd, rect.Left + rectShadow.Left, rect.Top, rect.Width + rectShadow.Width, rect.Height + rectShadow.Height);
            }
        }

        public static void ApplyAlwaysOnTop(IntPtr hwnd, PanelType panelType, bool alwaysOnTop)
        {
            var rectangle = WindowActionManager.GetWindowRectangle(hwnd);

            if (panelType == PanelType.PopOutManager)
            {
                OnPopOutManagerAlwaysOnTopChanged?.Invoke(null, alwaysOnTop);
                return;
            }

            if (alwaysOnTop)
                PInvoke.SetWindowPos(hwnd, new IntPtr(PInvokeConstant.HWND_TOPMOST), 0, 0, 0, 0, PInvokeConstant.SWP_ALWAYS_ON_TOP);
            else
            {
                PInvoke.SetWindowPos(hwnd, new IntPtr(PInvokeConstant.HWND_NOTOPMOST), rectangle.Left, rectangle.Top, rectangle.Width, rectangle.Height, 0);
                Thread.Sleep(250);
                MoveWindow(hwnd, rectangle.Left, rectangle.Top, rectangle.Width, rectangle.Height);
            }
        }

        public static IntPtr FindWindowByClass(string className)
        {
            return PInvoke.FindWindow(className, null);
        }

        public static void CloseWindow(IntPtr hwnd)
        {
            PInvoke.SendMessage(hwnd, PInvokeConstant.WM_CLOSE, IntPtr.Zero, IntPtr.Zero);
        }

        public static void MoveWindow(IntPtr hwnd, int x, int y, int width, int height)
        {
            var rectShadow = PInvoke.GetWindowRectShadow(hwnd);
            PInvoke.MoveWindow(hwnd, x + rectShadow.Left, y + rectShadow.Top, width + rectShadow.Width, height + rectShadow.Height, true);
        }

        public static void MoveWindow(IntPtr hwnd, Rectangle rect)
        {
            var rectShadow = PInvoke.GetWindowRectShadow(hwnd);
            PInvoke.MoveWindow(hwnd, rect.Left + rectShadow.Left, rect.Top + rectShadow.Top, rect.Width + rectShadow.Width, rect.Height + rectShadow.Height, true);
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

        public static void SetWindowFocus(IntPtr hwnd)
        {
            var style = (uint) PInvoke.GetWindowLong(hwnd, PInvokeConstant.GWL_STYLE);

            // Minimize and restore to be able to make it active.
            if ((style & PInvokeConstant.SW_MINIMIZE) == PInvokeConstant.SW_MINIMIZE)
            {
                PInvoke.ShowWindow(hwnd, PInvokeConstant.SW_RESTORE);
            }

            uint currentlyFocusedWindowProcessId = PInvoke.GetWindowThreadProcessId(PInvoke.GetForegroundWindow(), IntPtr.Zero);
            uint appThread = PInvoke.GetCurrentThreadId();

            if (currentlyFocusedWindowProcessId != appThread)
            {
                PInvoke.AttachThreadInput(currentlyFocusedWindowProcessId, appThread, true);
                PInvoke.BringWindowToTop(hwnd);
                PInvoke.ShowWindow(hwnd, PInvokeConstant.SW_SHOW);
                PInvoke.AttachThreadInput(currentlyFocusedWindowProcessId, appThread, false);
            }
            else
            {
                PInvoke.BringWindowToTop(hwnd);
                PInvoke.ShowWindow(hwnd, PInvokeConstant.SW_SHOW);
            }
        }

        public static IntPtr FindWindowByCaption(string caption)
        {
            return PInvoke.FindWindowByCaption(IntPtr.Zero, caption);
        }

        public static string GetWindowCaption(IntPtr hwnd)
        {
            try { return PInvoke.GetWindowText(hwnd); }
            catch { return string.Empty; }
        }

        public static void SetWindowCaption(IntPtr hwnd, string caption)
        {
            PInvoke.SetWindowText(hwnd, caption);
        }

        public static Rectangle GetWindowRectangle(IntPtr hwnd)
        {
            return PInvoke.GetWindowRectangleDwm(hwnd);
        }

        public static Rectangle GetClientRectangle(IntPtr hwnd)
        {
            Rectangle rect;
            PInvoke.GetClientRect(hwnd, out rect);

            return rect;
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

        public static List<IntPtr> GetWindowsByPanelType(List<PanelType> panelTypes)
        {
            List<IntPtr> windowHandles = new List<IntPtr>();

            PInvoke.EnumWindows((IntPtr hwnd, int lParam) =>
            {
                var panelType = GetWindowPanelType(hwnd);

                if (panelTypes.Contains(panelType))
                    windowHandles.Add(hwnd);

                return true;
            }, 0);

            return windowHandles;
        }

        public static PanelType GetWindowPanelType(IntPtr hwnd)
        {
            var className = PInvoke.GetClassName(hwnd);
            var caption = GetWindowCaption(hwnd);

            if (className == "AceApp")      // MSFS windows designation
            {
                if (String.IsNullOrEmpty(caption) || caption.IndexOf("(Custom)") > -1)      // Pop Out window
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
                if (caption.IndexOf("HudBar") > -1)
                    return PanelType.HudBarWindow;
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

                if (panelType == PanelType.CustomPopout || panelType == PanelType.HudBarWindow)
                    CloseWindow(hwnd);

                return true;
            }, 0);
        }

        public static MsfsGameWindowConfig GetMsfsGameWindowLocation()
        {
            var isWindowedMode = IsMsfsGameInWindowedMode();

            if (isWindowedMode)
            {
                var msfsGameWindowHandle = GetMsfsGameWindowHandle();
                var rect = WindowActionManager.GetWindowRectangle(msfsGameWindowHandle);
                return new MsfsGameWindowConfig(rect.Left, rect.Top, rect.Width, rect.Height);
            }

            return new MsfsGameWindowConfig(0, 0, 0, 0);
        }

        public static void SetMsfsGameWindowLocation(MsfsGameWindowConfig msfsGameWindowConfig)
        {
            var isWindowedMode = IsMsfsGameInWindowedMode();

            if (isWindowedMode && msfsGameWindowConfig.IsValid)
            {
                var msfsGameWindowHandle = GetMsfsGameWindowHandle();
                PInvoke.MoveWindow(msfsGameWindowHandle, msfsGameWindowConfig.Left, msfsGameWindowConfig.Top, msfsGameWindowConfig.Width, msfsGameWindowConfig.Height, true);
            }
        }

        public static bool IsMsfsGameInWindowedMode()
        {
            var msfsGameWindowHandle = GetMsfsGameWindowHandle();

            if (msfsGameWindowHandle != IntPtr.Zero)
            {
                var currentStyle = (uint)PInvoke.GetWindowLong(msfsGameWindowHandle, PInvokeConstant.GWL_STYLE);
                return (currentStyle & PInvokeConstant.WS_CAPTION) != 0;
            }

            return false;
        }

        public static IntPtr GetMsfsGameWindowHandle()
        {
            IntPtr msfsGameWindowHandle = IntPtr.Zero;

            // Get game window handle
            PInvoke.EnumWindows(new PInvoke.CallBack((IntPtr hwnd, int lParam) =>
            {
                var className = PInvoke.GetClassName(hwnd);

                if (className == "AceApp")      // MSFS windows designation
                {
                    var caption = GetWindowCaption(hwnd);

                    if (caption.IndexOf("Microsoft Flight Simulator") > -1)        // MSFS main game window
                    {
                        msfsGameWindowHandle = hwnd;
                    }
                }

                return true;
            }), 0);

            return msfsGameWindowHandle;
        }

        public static void SetWindowTitleBarColor(IntPtr hwnd, string hexColor)
        {
            int color;
            if (int.TryParse(hexColor, NumberStyles.HexNumber, null, out color))
                PInvoke.DwmSetWindowAttribute(hwnd, DwmWindowAttribute.DWMWA_CAPTION_COLOR, ref color, sizeof(Int32));
        }
    }
}
