using MSFSPopoutPanelManager.Shared;
using MSFSPopoutPanelManager.UI;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace MSFSPopoutPanelManager.Provider
{
    public class WindowManager
    {
        public static void AddPanelLocationSelectionOverlay(string text, int x, int y)
        {
            PopoutCoorOverlayForm frm = new PopoutCoorOverlayForm();
            frm.Location = new Point(x - frm.Width / 2, y - frm.Height / 2);
            frm.StartPosition = FormStartPosition.Manual;
            ((Label)frm.Controls.Find("lblPanelIndex", true)[0]).Text = text;
            frm.Show();
        }

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
                PInvoke.SetWindowPos(handle, PInvokeConstant.HWND_TOPMOST, panelRectangle.Left, panelRectangle.Top, panelRectangle.Width, panelRectangle.Height, PInvokeConstant.SWP_ALWAYS_ON_TOP);
            else
                PInvoke.SetWindowPos(handle, PInvokeConstant.HWND_NOTOPMOST, panelRectangle.Left, panelRectangle.Top, panelRectangle.Width, panelRectangle.Height, 0);
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

        public static WindowProcess GetSimulatorProcess()
        {
            return GetProcess("FlightSimulator");
        }

        public static WindowProcess GetApplicationProcess()
        {
            return GetProcess("MSFSPopoutPanelManager");
        }

        private static WindowProcess GetProcess(string processName)
        {
            foreach (var process in Process.GetProcesses())
            {
                if (process.ProcessName == processName)
                {
                    return new WindowProcess()
                    {
                        ProcessId = process.Id,
                        ProcessName = process.ProcessName,
                        Handle = process.MainWindowHandle
                    };
                }
            }

            return null;
        }

        public static void CloseAllCustomPopoutPanels()
        {
            PInvoke.EnumWindows(new PInvoke.CallBack(EnumAllCustomPopoutPanels), 1);
        }

        private static bool EnumAllCustomPopoutPanels(IntPtr hwnd, int index)
        {
            var className = PInvoke.GetClassName(hwnd);
            var caption = PInvoke.GetWindowText(hwnd);

            if (className == "AceApp" && (caption.IndexOf("(Custom)") > -1 || caption == String.Empty))      // Only close non-builtin pop out panels
            {
                WindowManager.CloseWindow(hwnd);
            }
            else if (className == "AceApp")     // for builtin pop out  (ATC, VFR Map, ect)
            {
                WindowManager.MoveWindow(hwnd, 0, 0);
            }

            return true;
        }
    }
}
