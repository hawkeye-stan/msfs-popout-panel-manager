using System;
using System.Diagnostics;
using System.Drawing;
using WindowsHook;

namespace MSFSPopoutPanelManager.WindowsAgent
{
    public class InputHookManager
    {
        // Mouse hooks
        private static IKeyboardMouseEvents _mouseHook;
        public static event EventHandler<Point> OnLeftClick;

        // Keyboard hooks
        private static IKeyboardMouseEvents _keyboardHook;
        public static event EventHandler<KeyUpEventArgs> OnKeyUp;

        public static void StartMouseHook()
        {
            if (_mouseHook == null)
            {
                Debug.WriteLine("Start Mouse Hook...");
                _mouseHook = Hook.GlobalEvents();
                _mouseHook.MouseDownExt += HandleMouseHookMouseDownExt;
            }
        }

        public static void EndMouseHook()
        {
            if (_mouseHook != null)
            {
                Debug.WriteLine("End Mouse Hook...");
                _mouseHook.MouseDownExt -= HandleMouseHookMouseDownExt;
                _mouseHook.Dispose();
                _mouseHook = null;
            }

            if (OnLeftClick != null)
            {
                foreach (Delegate d in OnLeftClick.GetInvocationList())
                    OnLeftClick -= (EventHandler<Point>)d;
            }
        }

        private static void HandleMouseHookMouseDownExt(object sender, MouseEventExtArgs e)
        {
            if (_mouseHook == null)
                return;

            if (e.Button == MouseButtons.Left)
                OnLeftClick?.Invoke(null, new Point(e.X, e.Y));
        }

        public static void StartKeyboardHook()
        {
            if (_keyboardHook == null)
            {
                Debug.WriteLine("Starting Keyboard Hook...");

                _keyboardHook = Hook.GlobalEvents();
                _keyboardHook.KeyUp += HandleKeyboardHookKeyUp;
            }
        }

        public static void EndKeyboardHook()
        {
            if (_keyboardHook != null)
            {
                Debug.WriteLine("Ending Keyboard Hook...");
                _keyboardHook.KeyUp -= HandleKeyboardHookKeyUp;
                _keyboardHook.Dispose();
                _keyboardHook = null;

                if (OnKeyUp != null)
                {
                    foreach (Delegate d in OnKeyUp.GetInvocationList())
                        OnKeyUp -= (EventHandler<KeyUpEventArgs>)d;
                }
            }
        }

        private static void HandleKeyboardHookKeyUp(object sender, KeyEventArgs e)
        {
            OnKeyUp?.Invoke(null, new KeyUpEventArgs() { KeyCode = e.KeyCode.ToString(), IsHoldControl = e.Control, IsHoldShift = e.Shift });
        }
    }

    public class KeyUpEventArgs : EventArgs
    {
        public string KeyCode { get; set; }

        public bool IsHoldControl { get; set; }

        public bool IsHoldShift { get; set; }
    }
}
