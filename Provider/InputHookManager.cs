using Gma.System.MouseKeyHook;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace MSFSPopoutPanelManager.Provider
{
    public class InputHookManager
    {
        private const string CTRL_KEY = "Control";
        private const string SHIFT_KEY = "Shift";

        private static IKeyboardMouseEvents _mouseHook;

        public static bool SubscribeToPanelSelectionEvent { get; set; }
        public static bool SubscribeToStartPopOutEvent { get; set; }

        public static event EventHandler OnPanelSelectionCompleted;
        public static event EventHandler<Point> OnPanelSelectionAdded;
        public static event EventHandler OnPanelSelectionRemoved;
        public static event EventHandler OnStartPopout;

        public static void StartHook()
        {
            if (_mouseHook == null)
            {
                _mouseHook = Hook.GlobalEvents();

                _mouseHook.OnCombination(new Dictionary<Combination, Action>
                {
                    {Combination.FromString("Control+Alt+P"), () => { if(SubscribeToStartPopOutEvent) OnStartPopout?.Invoke(null, null); }}
                });

                _mouseHook.MouseDownExt += HandleMouseHookMouseDownExt;
            }
        }

        public static void EndHook()
        {
            if (_mouseHook != null)
            {
                _mouseHook.MouseDownExt -= HandleMouseHookMouseDownExt;
                _mouseHook.Dispose();
            }
        }

        private static void HandleMouseHookMouseDownExt(object sender, MouseEventExtArgs e)
        {
            if (_mouseHook == null || !SubscribeToPanelSelectionEvent)
                return;

            if (e.Button == MouseButtons.Left)
            {
                var ctrlPressed = Control.ModifierKeys.ToString() == CTRL_KEY;
                var shiftPressed = Control.ModifierKeys.ToString() == SHIFT_KEY;

                if (ctrlPressed)
                {
                    OnPanelSelectionCompleted?.Invoke(null, null);
                }
                else if (shiftPressed)
                {
                    OnPanelSelectionRemoved?.Invoke(null, null);
                }
                else if (!shiftPressed)
                {
                    OnPanelSelectionAdded?.Invoke(null, new Point(e.X, e.Y));
                }
            }
        }
    }
}
