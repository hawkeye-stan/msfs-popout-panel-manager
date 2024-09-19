using MSFSPopoutPanelManager.DomainModel.Profile;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

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
                if(alwaysOnTop)
                    PInvoke.SetWindowPos(hwnd, new IntPtr(PInvokeConstant.HWND_TOPMOST), 0, 0, 0, 0, PInvokeConstant.SWP_ALWAYS_ON_TOP);
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

        public static void RestoreWindow(IntPtr hwnd)
        {
            PInvoke.ShowWindow(hwnd, PInvokeConstant.SW_RESTORE);
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

        public static List<IntPtr> GetWindowsByPanelType(List<PanelType> panelTypes)
        {
            var windowHandles = new List<IntPtr>();

            PInvoke.EnumWindows((hwnd, _) =>
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
                if (string.IsNullOrEmpty(caption) || caption.IndexOf("(Custom)", StringComparison.Ordinal) > -1)      // Pop Out window
                    return PanelType.CustomPopout;
                
                if (caption.IndexOf("Microsoft Flight Simulator", StringComparison.Ordinal) > -1)                // MSFS main game window
                    return PanelType.FlightSimMainWindow;
                
                if (caption.IndexOf("WINDOW", StringComparison.Ordinal) > -1)                                    // MSFS multi-monitor window
                    return PanelType.MultiMonitorWindow;
                
                return PanelType.BuiltInPopout;                                         // MSFS built-in window such as ATC, VFRMap
            }

            if (className.IndexOf("HwndWrapper[MSFSPopoutPanelManager", StringComparison.Ordinal) > -1)
            {
                if (caption.IndexOf("HudBar", StringComparison.Ordinal) > -1)
                    return PanelType.HudBarWindow;
                
                if (caption.IndexOf("Virtual NumPad", StringComparison.Ordinal) > -1)
                    return PanelType.NumPadWindow;

                if (caption.IndexOf("Switch Window", StringComparison.Ordinal) > -1)
                    return PanelType.SwitchWindow;
                
                return PanelType.PopOutManager;
            }

            return PanelType.Unknown;
        }

        public static void CloseAllPopOuts()
        {
            PInvoke.EnumWindows((hwnd, _) =>
            {
                var panelType = GetWindowPanelType(hwnd);

                if (panelType == PanelType.CustomPopout || panelType == PanelType.HudBarWindow || panelType == PanelType.NumPadWindow || panelType == PanelType.SwitchWindow)
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
            PInvoke.EnumWindows((hwnd, _) =>
            {
                var className = PInvoke.GetClassName(hwnd);

                if (className == "AceApp")      // MSFS windows designation
                {
                    var caption = GetWindowCaption(hwnd);

                    if (caption.IndexOf("Microsoft Flight Simulator", StringComparison.Ordinal) > -1)        // MSFS main game window
                    {
                        msfsGameWindowHandle = hwnd;
                    }
                }

                return true;
            }, 0);

            return msfsGameWindowHandle;
        }

        public static void SetWindowTitleBarColor(IntPtr hwnd, string hexColor)
        {
            if (int.TryParse(hexColor, NumberStyles.HexNumber, null, out var color))
                PInvoke.DwmSetWindowAttribute(hwnd, DwmWindowAttribute.DWMWA_CAPTION_COLOR, ref color, sizeof(Int32));
        }

        public static List<MonitorInfo> GetMonitorsInfo()
        {
            var monitors = new List<MonitorInfo>();

            foreach (var screen in Screen.AllScreens)
                monitors.Add(new MonitorInfo { Name = screen.DeviceName.Substring(screen.DeviceName.LastIndexOf("\\", StringComparison.Ordinal) + 1), X = screen.Bounds.X, Y = screen.Bounds.Y, Width = screen.Bounds.Width, Height = screen.Bounds.Height });
            
            return monitors;
        }

        public static bool IsPointInsideAppWindow(Point point)
        {
            var appHandle = WindowProcessManager.GetApplicationProcess().Handle;

            if (appHandle == IntPtr.Zero)
                return true;

            var rect = PInvoke.GetWindowRectangleDwm(appHandle);

            var rightEdge = rect.X + rect.Width;
            var bottomEdge = rect.Y + rect.Height;

            return point.X >= rect.X && point.X <= rightEdge && point.Y >= rect.Y && point.Y <= bottomEdge;
        }

        public static bool IsMsfsInFocus()
        {
            var handle = PInvoke.GetForegroundWindow();

            var text = PInvoke.GetWindowText(handle);

            if (text == null)
                return false;

            if (text.Length < 26)
                return false;

            return text.Substring(0, 26).Equals("Microsoft Flight Simulator", StringComparison.InvariantCultureIgnoreCase);
        }
    }
}
