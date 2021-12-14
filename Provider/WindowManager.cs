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
        private const int SWP_NOMOVE = 0x0002;
        private const int SWP_NOSIZE = 0x0001;
        private const int SWP_ALWAYS_ON_TOP = SWP_NOMOVE | SWP_NOSIZE;

        private const int GWL_STYLE = -16;
        private const int WS_SIZEBOX = 0x00040000;
        private const int WS_BORDER = 0x00800000;
        private const int WS_DLGFRAME = 0x00400000;
        private const int WS_CAPTION = WS_BORDER | WS_DLGFRAME;

        private const int HWND_TOPMOST = -1;
        private const int HWND_NOTOPMOST = -2;

        private const uint WM_CLOSE = 0x0010;

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
            var currentStyle = PInvoke.GetWindowLong(handle, GWL_STYLE).ToInt64();

            if (hideTitleBar)
                PInvoke.SetWindowLong(handle, GWL_STYLE, (uint)(currentStyle & ~(WS_CAPTION | WS_SIZEBOX)));
            else
                PInvoke.SetWindowLong(handle, GWL_STYLE, (uint)(currentStyle | (WS_CAPTION | WS_SIZEBOX)));
        }

        public static void ApplyAlwaysOnTop(IntPtr handle, bool alwaysOnTop, Rectangle panelRectangle)
        {
            if (alwaysOnTop)
                PInvoke.SetWindowPos(handle, HWND_TOPMOST, panelRectangle.Left, panelRectangle.Top, panelRectangle.Width, panelRectangle.Height, SWP_ALWAYS_ON_TOP);
            else
                PInvoke.SetWindowPos(handle, HWND_NOTOPMOST, panelRectangle.Left, panelRectangle.Top, panelRectangle.Width, panelRectangle.Height, 0);
        }

        public static void CloseWindow(IntPtr handle)
        {
            PInvoke.SendMessage(handle, WM_CLOSE, IntPtr.Zero, IntPtr.Zero);
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
    }
}
