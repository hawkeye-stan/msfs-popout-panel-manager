using System;
using System.Diagnostics;
using System.Drawing;
using WindowsHook;

namespace MSFSPopoutPanelManager.WindowsAgent
{
    public class InputHookManager
    {
        private static IKeyboardMouseEvents _mouseHook;

        public static event EventHandler<Point> OnCtrlLeftClick;
        public static event EventHandler<Point> OnShiftLeftClick;
        public static event EventHandler<Point> OnLeftClick;

        public static void StartHook()
        {
            if (_mouseHook == null)
            {
                Debug.WriteLine("Start Mouse Hook.........");
                _mouseHook = Hook.GlobalEvents();
                _mouseHook.MouseDownExt += HandleMouseHookMouseDownExt;
            }
        }

        public static void EndHook()
        {
            if (_mouseHook != null)
            {
                Debug.WriteLine("End Mouse Hook.........");
                _mouseHook.MouseDownExt -= HandleMouseHookMouseDownExt;
                _mouseHook.Dispose();
                _mouseHook = null;
            }
        }

        private static void HandleMouseHookMouseDownExt(object sender, MouseEventExtArgs e)
        {
            if (_mouseHook == null)
                return;

            if (e.Button == MouseButtons.Left)
            {
                var shiftPress = PInvoke.GetAsyncKeyState(0xA0) <= -127 || PInvoke.GetAsyncKeyState(0xA1) <= -127;
                var ctrlPress = PInvoke.GetAsyncKeyState(0xA2) <= -127 || PInvoke.GetAsyncKeyState(0xA3) <= -127;

                if (ctrlPress)
                    OnCtrlLeftClick?.Invoke(null, new Point(e.X, e.Y));
                else if (shiftPress)
                    OnShiftLeftClick?.Invoke(null, new Point(e.X, e.Y));
                else
                    OnLeftClick?.Invoke(null, new Point(e.X, e.Y));
            }
        }
    }
}
