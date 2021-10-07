using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;

namespace MSFSPopoutPanelManager
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

        public static void AddPanelLocationSelectionOverlay(string text, int x, int y)
        {
            PopoutCoorOverlayForm frm = new PopoutCoorOverlayForm();
            frm.Location = new Point(x - frm.Width / 2, y - frm.Height / 2);
            frm.StartPosition = FormStartPosition.Manual;
            ((Label)frm.Controls.Find("lblPanelIndex", true)[0]).Text = text;
            frm.Show();
        }

        private static IntPtr CreateLParam(int LoWord, int HiWord)
        {
            return (IntPtr)((HiWord << 16) | (LoWord & 0xffff));
        }

        public static void ExecutePopout(IntPtr simulatorHandle, List<PanelSourceCoordinate> screenCoordinates)
        {
            PInvoke.SetForegroundWindow(simulatorHandle);

            const uint MOUSEEVENTF_LEFTDOWN = 0x02;
            const uint MOUSEEVENTF_LEFTUP = 0x04;
            const uint KEYEVENTF_EXTENDEDKEY = 0x01;
            const uint KEYEVENTF_KEYUP = 0x2;
            const uint VK_RMENU = 0xA5;         // Right Alternate key

            foreach (var coor in screenCoordinates)
            {
                // Move the cursor to the flight simulator screen then move the cursor into position
                PInvoke.SetCursorPos(0, 0);
                PInvoke.SetCursorPos(coor.X, coor.Y);

                // Wait for mouse to get into position
                Thread.Sleep(1000);

                // Force a left click first
                PInvoke.mouse_event(MOUSEEVENTF_LEFTDOWN, coor.X, coor.Y, 0, 0);
                Thread.Sleep(200);
                PInvoke.mouse_event(MOUSEEVENTF_LEFTUP, coor.X, coor.Y, 0, 0);

                Thread.Sleep(300);

                // Pop out the screen by Alt Left click
                PInvoke.keybd_event(Convert.ToByte(VK_RMENU), 0, KEYEVENTF_EXTENDEDKEY, 0);
                PInvoke.mouse_event(MOUSEEVENTF_LEFTDOWN, coor.X, coor.Y, 0, 0);
                Thread.Sleep(200);
                PInvoke.mouse_event(MOUSEEVENTF_LEFTUP, coor.X, coor.Y, 0, 0);
                PInvoke.keybd_event(Convert.ToByte(VK_RMENU), 0, KEYEVENTF_KEYUP, 0);
            }
        }

        public static void ApplyHidePanelTitleBar(IntPtr handle, bool hideTitleBar)
        {
            var currentStyle = PInvoke.GetWindowLong(handle, GWL_STYLE).ToInt64();

            if (hideTitleBar)
                PInvoke.SetWindowLong(handle, GWL_STYLE, (uint)(currentStyle & ~(WS_CAPTION | WS_SIZEBOX)));
            else
                PInvoke.SetWindowLong(handle, GWL_STYLE, (uint)(currentStyle | (WS_CAPTION | WS_SIZEBOX)));
        }

        public static void ApplyAlwaysOnTop(IntPtr handle, bool alwaysOnTop)
        {
            if (alwaysOnTop)
            {
                Rect rect = new Rect();
                PInvoke.GetWindowRect(handle, out rect);
                PInvoke.SetWindowPos(handle, -1, rect.Left, rect.Top, rect.Right - rect.Left, rect.Bottom - rect.Top, SWP_ALWAYS_ON_TOP);
            }
            else
            {
                Rect rect = new Rect();
                PInvoke.GetWindowRect(handle, out rect);
                PInvoke.SetWindowPos(handle, -2, rect.Left, rect.Top, rect.Right - rect.Left, rect.Bottom - rect.Top, 0);
            }
        }
    }
}
