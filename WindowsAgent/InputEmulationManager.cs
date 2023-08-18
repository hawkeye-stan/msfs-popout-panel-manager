using MSFSPopoutPanelManager.DomainModel.Profile;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Threading;
using WindowsInput;

namespace MSFSPopoutPanelManager.WindowsAgent
{
    public class InputEmulationManager
    {
        const uint MOUSEEVENTF_LEFTDOWN = 0x02;
        const uint MOUSEEVENTF_LEFTUP = 0x04;
        const uint MOUSEEVENTF_RIGHTDOWN = 0x08;
        const uint MOUSEEVENTF_RIGHTUP = 0x10;
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

        public static void LeftClick(int x, int y)
        {
            PInvoke.SetCursorPos(x, y);
            Thread.Sleep(300);

            PInvoke.mouse_event(MOUSEEVENTF_LEFTDOWN, x, y, 0, 0);
            Thread.Sleep(200);
            PInvoke.mouse_event(MOUSEEVENTF_LEFTUP, x, y, 0, 0);
            Thread.Sleep(200);
        }

        public static void LeftClickFast(int x, int y)
        {
            PInvoke.mouse_event(MOUSEEVENTF_LEFTDOWN, x, y, 0, 0);
            Thread.Sleep(50);
            PInvoke.mouse_event(MOUSEEVENTF_LEFTUP, x, y, 0, 0);
        }

        public static void PopOutPanel(int x, int y, bool useSecondaryKeys)
        {
            Debug.WriteLine($"Pop out panel at: {x}/{y} ...");

            PInvoke.SetForegroundWindow(WindowProcessManager.SimulatorProcess.Handle);
            Thread.Sleep(200);

            MoveAppWindowFromLeftClickPoint(x, y);

            LeftClick(x, y);

            if (useSecondaryKeys)
            {
                InputSimulator.Keyboard.KeyDown(WindowsInput.Native.VirtualKeyCode.LCONTROL);
                InputSimulator.Keyboard.KeyDown(WindowsInput.Native.VirtualKeyCode.RCONTROL);
                PInvoke.mouse_event(MOUSEEVENTF_LEFTDOWN, x, y, 0, 0);
                Thread.Sleep(200);
                PInvoke.mouse_event(MOUSEEVENTF_LEFTUP, x, y, 0, 0);
                Thread.Sleep(200);
                PInvoke.mouse_event(MOUSEEVENTF_LEFTDOWN, x, y, 0, 0);
                Thread.Sleep(200);
                PInvoke.mouse_event(MOUSEEVENTF_LEFTUP, x, y, 0, 0);
                InputSimulator.Keyboard.KeyUp(WindowsInput.Native.VirtualKeyCode.RCONTROL);
                InputSimulator.Keyboard.KeyUp(WindowsInput.Native.VirtualKeyCode.LCONTROL);
                Thread.Sleep(100);
                InputSimulator.Keyboard.KeyUp(WindowsInput.Native.VirtualKeyCode.RCONTROL);     // resend to make sure Ctrl key is up
                InputSimulator.Keyboard.KeyUp(WindowsInput.Native.VirtualKeyCode.LCONTROL);
            }
            else
            {
                InputSimulator.Keyboard.KeyDown(WindowsInput.Native.VirtualKeyCode.RMENU);
                PInvoke.mouse_event(MOUSEEVENTF_LEFTDOWN, x, y, 0, 0);
                Thread.Sleep(200);
                PInvoke.mouse_event(MOUSEEVENTF_LEFTUP, x, y, 0, 0);
                Thread.Sleep(200);
                PInvoke.mouse_event(MOUSEEVENTF_LEFTDOWN, x, y, 0, 0);
                Thread.Sleep(200);
                PInvoke.mouse_event(MOUSEEVENTF_LEFTUP, x, y, 0, 0);
                InputSimulator.Keyboard.KeyUp(WindowsInput.Native.VirtualKeyCode.RMENU);
                Thread.Sleep(100);
                InputSimulator.Keyboard.KeyUp(WindowsInput.Native.VirtualKeyCode.RMENU);        // resend to make sure Alt key is up
            }
        }

        public static void CenterView()
        {
            Debug.WriteLine("Centering view......");

            var hwnd = WindowProcessManager.SimulatorProcess.Handle;
            PInvoke.SetForegroundWindow(hwnd);
            Thread.Sleep(200);

            // First center view using Ctrl-Space
            PInvoke.keybd_event(Convert.ToByte(VK_RCONTROL), 0, KEYEVENTF_KEYDOWN, 0);
            PInvoke.keybd_event(Convert.ToByte(VK_SPACE), 0, KEYEVENTF_KEYDOWN, 0);
            Thread.Sleep(200);
            PInvoke.keybd_event(Convert.ToByte(VK_SPACE), 0, KEYEVENTF_KEYUP, 0);
            PInvoke.keybd_event(Convert.ToByte(VK_RCONTROL), 0, KEYEVENTF_KEYUP, 0);
            Thread.Sleep(200);

            // Wait for center view to complete
            Thread.Sleep(500);
        }

        public static void SaveCustomView(string keybinding)
        {
            Debug.WriteLine("Saving custom view...");

            var hwnd = WindowProcessManager.SimulatorProcess.Handle;
            uint customViewKey = (uint)(Convert.ToInt32(keybinding) + KEY_0);

            PInvoke.SetForegroundWindow(hwnd);
            Thread.Sleep(200);

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
            Thread.Sleep(200);
        }

        public static void LoadCustomView(string keybinding)
        {
            Debug.WriteLine("Loading custom view...");

            var hwnd = WindowProcessManager.SimulatorProcess.Handle;
            PInvoke.SetForegroundWindow(hwnd);
            Thread.Sleep(200);

            uint customViewKey = (uint)(Convert.ToInt32(keybinding) + KEY_0);

            // Then load view using Alt-0
            PInvoke.keybd_event(Convert.ToByte(VK_LMENU), 0, KEYEVENTF_KEYDOWN, 0);
            PInvoke.keybd_event(Convert.ToByte(customViewKey), 0, KEYEVENTF_KEYDOWN, 0);
            Thread.Sleep(200);
            PInvoke.keybd_event(Convert.ToByte(customViewKey), 0, KEYEVENTF_KEYUP, 0);
            PInvoke.keybd_event(Convert.ToByte(VK_LMENU), 0, KEYEVENTF_KEYUP, 0);
            Thread.Sleep(200);
        }

        public static void ToggleFullScreenPanel(IntPtr hwnd)
        {
            PInvoke.SetForegroundWindow(hwnd);
            Thread.Sleep(200);

            PInvoke.SetFocus(hwnd);
            Thread.Sleep(300);

            PInvoke.keybd_event(Convert.ToByte(VK_LMENU), 0, KEYEVENTF_KEYDOWN, 0);
            PInvoke.keybd_event(Convert.ToByte(VK_ENT), 0, KEYEVENTF_KEYDOWN, 0);
            Thread.Sleep(200);
            PInvoke.keybd_event(Convert.ToByte(VK_ENT), 0, KEYEVENTF_KEYUP, 0);
            PInvoke.keybd_event(Convert.ToByte(VK_LMENU), 0, KEYEVENTF_KEYUP, 0);
            Thread.Sleep(200);
        }

        public static void RefocusGameWindow(PanelType panelType)
        {
            var rect = WindowActionManager.GetWindowRectangle(WindowProcessManager.SimulatorProcess.Handle);
            PInvoke.SetCursorPos(rect.X + rect.Width / 2, rect.Y + rect.Height / 2);
        }

        private static (int?, int?) GetGameWindowLeftClickPoint(int x, int y)
        {
            var applicationRectangle = WindowActionManager.GetWindowRectangle(WindowProcessManager.GetApplicationProcess().Handle);
            var applicationRightEdge = applicationRectangle.X + applicationRectangle.Width;
            var applicationBottomEdge = applicationRectangle.Y + applicationRectangle.Height;

            var gameRectangle = WindowActionManager.GetWindowRectangle(WindowProcessManager.SimulatorProcess.Handle);

            int offset = 10;

            if (!IsPointWithinRectangle(x, y, applicationRectangle))
                return (x, y);

            // Try lower right corner (higher than bottom edge)
            if (IsPointWithinRectangle(applicationRightEdge + offset, applicationBottomEdge - offset, gameRectangle))
                return (applicationRightEdge + offset, applicationBottomEdge - offset);

            // Try upper right corner (lower than title bar)
            if (IsPointWithinRectangle(applicationRightEdge + offset, applicationRectangle.Top + offset, gameRectangle))
                return (applicationRightEdge + offset, applicationRectangle.Top + offset);

            // Try lower left corner (higher than bottom edge)
            if (IsPointWithinRectangle(applicationRectangle.X - offset, applicationBottomEdge - offset, gameRectangle))
                return (applicationRectangle.X - offset, applicationBottomEdge - offset);

            // Try upper left corner (lower than title bar)
            if (IsPointWithinRectangle(applicationRectangle.X - offset, applicationRectangle.Top + offset, gameRectangle))
                return (applicationRectangle.X - offset, applicationRectangle.Top + offset);

            // Application window totally overlapping click point
            return (null, null);
        }

        private static void MoveAppWindowFromLeftClickPoint(int x, int y)
        {
            var appHandle = WindowProcessManager.GetApplicationProcess().Handle;
            var applicationRectangle = WindowActionManager.GetWindowRectangle(appHandle);

            if (IsPointWithinRectangle(x, y, applicationRectangle))
            {
                var top = y - applicationRectangle.Height - 50;
                WindowActionManager.MoveWindow(appHandle, applicationRectangle.X, top, applicationRectangle.Width, applicationRectangle.Height);
            }
        }

        private static bool IsPointWithinRectangle(int x, int y, Rectangle rect)
        {
            var rightEdge = rect.X + rect.Width;
            var bottomEdge = rect.Y + rect.Height;

            return x >= rect.X && x <= rightEdge && y >= rect.Y && y <= bottomEdge;
        }

        #region Deprecated

        public static void RightClickGameWindow()
        {
            if (WindowProcessManager.SimulatorProcess == null)
                return;

            var gameRectangle = WindowActionManager.GetWindowRectangle(WindowProcessManager.SimulatorProcess.Handle);
            var gameCenterPointX = gameRectangle.X + gameRectangle.Width / 2;
            var gameCenterPointY = gameRectangle.Y + gameRectangle.Height / 2;
            var (x, y) = GetGameWindowLeftClickPoint(gameCenterPointX, gameCenterPointY);

            PInvoke.SetForegroundWindow(WindowProcessManager.SimulatorProcess.Handle);

            if (x == null || y == null)
            {
                int offset = 10;
                var applicationRectangle = WindowActionManager.GetWindowRectangle(WindowProcessManager.GetApplicationProcess().Handle);

                // Move app to outside game window
                WindowActionManager.MoveWindow(WindowProcessManager.GetApplicationProcess().Handle, gameRectangle.X + gameRectangle.Width + offset, gameRectangle.Top + gameRectangle.Height + offset, applicationRectangle.Width, applicationRectangle.Height);

                (x, y) = GetGameWindowLeftClickPoint(gameCenterPointX, gameCenterPointY);

                RightClick((int)x, (int)y);

                WindowActionManager.MoveWindow(WindowProcessManager.GetApplicationProcess().Handle, applicationRectangle);
            }
            else
            {
                RightClick((int)x, (int)y);
            }
        }

        public static void RightClick(int x, int y)
        {
            PInvoke.SetCursorPos(x, y);
            Thread.Sleep(300);

            PInvoke.mouse_event(MOUSEEVENTF_RIGHTDOWN, x, y, 0, 0);
            Thread.Sleep(200);
            PInvoke.mouse_event(MOUSEEVENTF_RIGHTUP, x, y, 0, 0);

            PInvoke.SetCursorPos(x + 5, y);
        }

        #endregion
    }
}