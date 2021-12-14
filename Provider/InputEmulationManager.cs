using MSFSPopoutPanelManager.Shared;
using System;
using System.Threading;

namespace MSFSPopoutPanelManager.Provider
{
    public class InputEmulationManager
    {
        const uint MOUSEEVENTF_LEFTDOWN = 0x02;
        const uint MOUSEEVENTF_LEFTUP = 0x04;
        const uint KEYEVENTF_EXTENDEDKEY = 0x01;
        const uint KEYEVENTF_KEYDOWN = 0x0;
        const uint KEYEVENTF_KEYUP = 0x2;
        const uint VK_RMENU = 0xA5;
        const uint VK_LMENU = 0xA4;
        const uint VK_LCONTROL = 0xA2;
        const uint VK_SPACE = 0x20;
        const uint KEY_0 = 0x30;

        public static void SendMouseToLocation(IntPtr hwnd, int x, int y)
        {
            // Move the cursor to the flight simulator screen then move the cursor into position
            //PInvoke.SetCursorPos(0, 0);
            PInvoke.SetFocus(hwnd);
            PInvoke.SetCursorPos(x, y);
        }

        public static void LeftClick(int x, int y)
        {
            PInvoke.SetCursorPos(x, y);
            Thread.Sleep(300);

            PInvoke.mouse_event(MOUSEEVENTF_LEFTDOWN, x, y, 0, 0);
            Thread.Sleep(200);
            PInvoke.mouse_event(MOUSEEVENTF_LEFTUP, x, y, 0, 0);
        }

        public static void PopOutPanel(int x, int y)
        {
            PInvoke.SetCursorPos(x, y);
            Thread.Sleep(300);

            PInvoke.keybd_event(Convert.ToByte(VK_RMENU), 0, KEYEVENTF_EXTENDEDKEY, 0);
            PInvoke.mouse_event(MOUSEEVENTF_LEFTDOWN, x, y, 0, 0);
            Thread.Sleep(200);
            PInvoke.mouse_event(MOUSEEVENTF_LEFTUP, x, y, 0, 0);
            PInvoke.keybd_event(Convert.ToByte(VK_RMENU), 0, KEYEVENTF_KEYUP, 0);
        }

        public static void CenterView(IntPtr hwnd)
        {
            PInvoke.SetForegroundWindow(hwnd);
            Thread.Sleep(500);

            PInvoke.SetFocus(hwnd);
            Thread.Sleep(300);

            // First center view using Ctrl-Space
            PInvoke.keybd_event(Convert.ToByte(VK_LCONTROL), 0, KEYEVENTF_KEYDOWN, 0);
            PInvoke.keybd_event(Convert.ToByte(VK_SPACE), 0, KEYEVENTF_KEYDOWN, 0);
            Thread.Sleep(200);
            PInvoke.keybd_event(Convert.ToByte(VK_SPACE), 0, KEYEVENTF_KEYUP, 0);
            PInvoke.keybd_event(Convert.ToByte(VK_LCONTROL), 0, KEYEVENTF_KEYUP, 0);
            Thread.Sleep(200);
        }

        public static void SaveCustomViewZero(IntPtr hwnd)
        {
            PInvoke.SetForegroundWindow(hwnd);
            Thread.Sleep(500);

            PInvoke.SetFocus(hwnd);
            Thread.Sleep(300);

            // Set view using Ctrl-Alt-0
            PInvoke.keybd_event(Convert.ToByte(VK_LCONTROL), 0, KEYEVENTF_KEYDOWN, 0);
            PInvoke.keybd_event(Convert.ToByte(VK_LMENU), 0, KEYEVENTF_KEYDOWN, 0);
            PInvoke.keybd_event(Convert.ToByte(KEY_0), 0, KEYEVENTF_KEYDOWN, 0);
            Thread.Sleep(200);
            PInvoke.keybd_event(Convert.ToByte(KEY_0), 0, KEYEVENTF_KEYUP, 0);
            PInvoke.keybd_event(Convert.ToByte(VK_LMENU), 0, KEYEVENTF_KEYUP, 0);
            PInvoke.keybd_event(Convert.ToByte(VK_LCONTROL), 0, KEYEVENTF_KEYUP, 0);

        }

        public static void LoadCustomViewZero(IntPtr hwnd)
        {
            PInvoke.SetForegroundWindow(hwnd);
            Thread.Sleep(500);

            PInvoke.SetFocus(hwnd);
            Thread.Sleep(300);

            // First center view using Ctrl-Space
            PInvoke.keybd_event(Convert.ToByte(VK_LCONTROL), 0, KEYEVENTF_KEYDOWN, 0);
            PInvoke.keybd_event(Convert.ToByte(VK_SPACE), 0, KEYEVENTF_KEYDOWN, 0);
            Thread.Sleep(200);
            PInvoke.keybd_event(Convert.ToByte(VK_SPACE), 0, KEYEVENTF_KEYUP, 0);
            PInvoke.keybd_event(Convert.ToByte(VK_LCONTROL), 0, KEYEVENTF_KEYUP, 0);
            Thread.Sleep(200);

            // Then load view using Alt-0
            PInvoke.keybd_event(Convert.ToByte(VK_LMENU), 0, KEYEVENTF_KEYDOWN, 0);
            PInvoke.keybd_event(Convert.ToByte(KEY_0), 0, KEYEVENTF_KEYDOWN, 0);
            Thread.Sleep(200);
            PInvoke.keybd_event(Convert.ToByte(KEY_0), 0, KEYEVENTF_KEYUP, 0);
            PInvoke.keybd_event(Convert.ToByte(VK_LMENU), 0, KEYEVENTF_KEYUP, 0);
        }
    }
}