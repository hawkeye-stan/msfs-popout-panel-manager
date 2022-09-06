using System;
using System.Drawing;
using System.Threading;
using WindowsInput;

namespace MSFSPopoutPanelManager.WindowsAgent
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
        const uint VK_RCONTROL = 0xA3;
        const uint VK_SPACE = 0x20;
        const uint VK_ENT = 0x0D;
        const uint KEY_0 = 0x30;

        private static InputSimulator InputSimulator = new InputSimulator();

        public static void LeftClickGameWindow()
        {
            var simualatorProcess = WindowProcessManager.GetSimulatorProcess();
            if (simualatorProcess == null)
                return;

            Rectangle rectangle;
            PInvoke.GetWindowRect(simualatorProcess.Handle, out rectangle);

            Rectangle clientRectangle;
            PInvoke.GetClientRect(simualatorProcess.Handle, out clientRectangle);

            var x = Convert.ToInt32(rectangle.X + (clientRectangle.Width) * 0.5);
            var y = Convert.ToInt32(rectangle.Y + (clientRectangle.Height) * 0.5);

            LeftClick(x, y);
        }

        public static void LeftClick(int x, int y)
        {
            PInvoke.SetCursorPos(x, y);     // Need to do this twice to overcome MSFS bug for separating pop out panels
            PInvoke.SetCursorPos(x, y);
            Thread.Sleep(300);

            PInvoke.mouse_event(MOUSEEVENTF_LEFTDOWN, x, y, 0, 0);
            Thread.Sleep(200);
            PInvoke.mouse_event(MOUSEEVENTF_LEFTUP, x, y, 0, 0);

            PInvoke.SetCursorPos(x + 5, y);
        }

        public static void LeftClickFast(int x, int y)
        {
            PInvoke.mouse_event(MOUSEEVENTF_LEFTDOWN, x, y, 0, 0);
            Thread.Sleep(50);
            PInvoke.mouse_event(MOUSEEVENTF_LEFTUP, x, y, 0, 0);
        }

        public static void LeftClickMouseDown(int x, int y)
        {
            PInvoke.mouse_event(MOUSEEVENTF_LEFTDOWN, x, y, 0, 0);
        }

        public static void LeftClickMouseUp(int x, int y)
        {
            InputSimulator.Mouse.LeftButtonUp();
        }

        public static void PopOutPanel(int x, int y, bool useSecondaryKeys)
        {
            LeftClick(x, y);
            Thread.Sleep(300);

            PInvoke.SetCursorPos(x, y);
            PInvoke.SetCursorPos(x, y);
            Thread.Sleep(300);

            if (useSecondaryKeys)
            {
                InputSimulator.Keyboard.KeyDown(WindowsInput.Native.VirtualKeyCode.LCONTROL);
                InputSimulator.Keyboard.KeyDown(WindowsInput.Native.VirtualKeyCode.RCONTROL);
                PInvoke.mouse_event(MOUSEEVENTF_LEFTDOWN, x, y, 0, 0);
                Thread.Sleep(200);
                PInvoke.mouse_event(MOUSEEVENTF_LEFTUP, x, y, 0, 0);
                InputSimulator.Keyboard.KeyUp(WindowsInput.Native.VirtualKeyCode.RCONTROL);
                InputSimulator.Keyboard.KeyUp(WindowsInput.Native.VirtualKeyCode.LCONTROL);
            }
            else
            {
                PInvoke.keybd_event(Convert.ToByte(VK_RMENU), 0, KEYEVENTF_EXTENDEDKEY, 0);
                PInvoke.mouse_event(MOUSEEVENTF_LEFTDOWN, x, y, 0, 0);
                Thread.Sleep(200);
                PInvoke.mouse_event(MOUSEEVENTF_LEFTUP, x, y, 0, 0);
                PInvoke.keybd_event(Convert.ToByte(VK_RMENU), 0, KEYEVENTF_KEYUP, 0);
            }
        }

        public static void CenterView()
        {
            var simualatorProcess = WindowProcessManager.GetSimulatorProcess();
            if (simualatorProcess == null)
                return;

            CenterView(simualatorProcess);
        }

        public static void CenterView(WindowProcess simualatorProcess)
        {
            Rectangle rectangle;
            PInvoke.GetWindowRect(simualatorProcess.Handle, out rectangle);

            Rectangle clientRectangle;
            PInvoke.GetClientRect(simualatorProcess.Handle, out clientRectangle);

            var x = Convert.ToInt32(rectangle.X + (clientRectangle.Width) * 0.5);
            var y = Convert.ToInt32(rectangle.Y + (clientRectangle.Height) * 0.5);

            PInvoke.SetForegroundWindow(simualatorProcess.Handle);
            LeftClick(x, y);

            // First center view using Ctrl-Space
            PInvoke.keybd_event(Convert.ToByte(VK_RCONTROL), 0, KEYEVENTF_KEYDOWN, 0);
            PInvoke.keybd_event(Convert.ToByte(VK_SPACE), 0, KEYEVENTF_KEYDOWN, 0);
            Thread.Sleep(200);
            PInvoke.keybd_event(Convert.ToByte(VK_SPACE), 0, KEYEVENTF_KEYUP, 0);
            PInvoke.keybd_event(Convert.ToByte(VK_RCONTROL), 0, KEYEVENTF_KEYUP, 0);
            Thread.Sleep(200);
        }

        public static void SaveCustomView(string keybinding)
        {
            var simualatorProcess = WindowProcessManager.GetSimulatorProcess();
            if (simualatorProcess == null)
                return;

            var hwnd = simualatorProcess.Handle;
            uint customViewKey = (uint)(Convert.ToInt32(keybinding) + KEY_0);

            PInvoke.SetForegroundWindow(hwnd);
            Thread.Sleep(500);

            PInvoke.SetFocus(hwnd);
            Thread.Sleep(300);

            // Set view using Ctrl-Alt-0
            PInvoke.keybd_event(Convert.ToByte(VK_LCONTROL), 0, KEYEVENTF_KEYDOWN, 0);
            PInvoke.keybd_event(Convert.ToByte(VK_LMENU), 0, KEYEVENTF_KEYDOWN, 0);
            PInvoke.keybd_event(Convert.ToByte(customViewKey), 0, KEYEVENTF_KEYDOWN, 0);
            Thread.Sleep(200);
            PInvoke.keybd_event(Convert.ToByte(customViewKey), 0, KEYEVENTF_KEYUP, 0);
            PInvoke.keybd_event(Convert.ToByte(VK_LMENU), 0, KEYEVENTF_KEYUP, 0);
            PInvoke.keybd_event(Convert.ToByte(VK_LCONTROL), 0, KEYEVENTF_KEYUP, 0);

        }

        public static void LoadCustomView(string keybinding)
        {
            var simualatorProcess = WindowProcessManager.GetSimulatorProcess();
            if (simualatorProcess == null)
                return;

            // First center view to make sure recalling custom camera works on the first press
            InputEmulationManager.CenterView();
            Thread.Sleep(500);

            uint customViewKey = (uint)(Convert.ToInt32(keybinding) + KEY_0);

            // Then load view using Alt-0
            PInvoke.keybd_event(Convert.ToByte(VK_LMENU), 0, KEYEVENTF_KEYDOWN, 0);
            PInvoke.keybd_event(Convert.ToByte(customViewKey), 0, KEYEVENTF_KEYDOWN, 0);
            Thread.Sleep(200);
            PInvoke.keybd_event(Convert.ToByte(customViewKey), 0, KEYEVENTF_KEYUP, 0);
            PInvoke.keybd_event(Convert.ToByte(VK_LMENU), 0, KEYEVENTF_KEYUP, 0);

            Thread.Sleep(500);
        }

        public static void ToggleFullScreenPanel(IntPtr hwnd)
        {
            PInvoke.SetForegroundWindow(hwnd);
            Thread.Sleep(500);

            PInvoke.SetFocus(hwnd);
            Thread.Sleep(300);

            PInvoke.keybd_event(Convert.ToByte(VK_RMENU), 0, KEYEVENTF_KEYDOWN, 0);
            PInvoke.keybd_event(Convert.ToByte(VK_ENT), 0, KEYEVENTF_KEYDOWN, 0);
            Thread.Sleep(200);
            PInvoke.keybd_event(Convert.ToByte(VK_ENT), 0, KEYEVENTF_KEYUP, 0);
            PInvoke.keybd_event(Convert.ToByte(VK_RMENU), 0, KEYEVENTF_KEYUP, 0);
        }

        public static void RefocusGameWindow()
        {
            var simualatorProcess = WindowProcessManager.GetSimulatorProcess();
            if (simualatorProcess == null)
                return;

            var rectangle = WindowActionManager.GetWindowRect(simualatorProcess.Handle);
            var clientRectangle = WindowActionManager.GetClientRect(simualatorProcess.Handle);

            PInvoke.SetCursorPos(rectangle.X + clientRectangle.Width / 2, rectangle.Y + clientRectangle.Height / 2);
        }

    }
}