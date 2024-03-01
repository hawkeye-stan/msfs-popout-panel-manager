using MSFSPopoutPanelManager.WindowsAgent;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace MSFSPopoutPanelManager.Orchestration
{
    public class KeyboardOrchestrator : BaseOrchestrator
    {
        private GlobalKeyboardHook _globalKeyboardHook;

        
        public KeyboardOrchestrator(SharedStorage sharedStorage) : base(sharedStorage)
        {
        }
        
        public void Initialize()
        {
            if (AppSettingData.ApplicationSetting.KeyboardShortcutSetting.IsEnabled)
            {
                InputHookManager.StartKeyboardHook("KeyboardShortcut");
                InputHookManager.OnKeyUp -= HandleShortcutKeyboardHookKeyUpEvent;
                InputHookManager.OnKeyUp += HandleShortcutKeyboardHookKeyUpEvent;
            }

            AppSettingData.ApplicationSetting.OnIsUsedKeyboardShortcutChanged += (_, e) =>
            {
                if (e)
                {
                    InputHookManager.StartKeyboardHook("KeyboardShortcut");
                    InputHookManager.OnKeyUp -= HandleShortcutKeyboardHookKeyUpEvent;
                    InputHookManager.OnKeyUp += HandleShortcutKeyboardHookKeyUpEvent;
                }
                else
                {
                    InputHookManager.EndKeyboardHook("KeyboardShortcut");
                    InputHookManager.OnKeyUp -= HandleShortcutKeyboardHookKeyUpEvent;
                }
            };



        }
        public async void HandleShortcutKeyboardHookKeyUpEvent(object sender, KeyUpEventArgs e)
        {
            // Start pop out
            if (e.IsHoldControl && e.IsHoldShift && e.KeyCode.ToUpper() ==
                AppSettingData.ApplicationSetting.KeyboardShortcutSetting.StartPopOutKeyBinding)
            {
                //await _panelPopOutOrchestrator.ManualPopOut();
                return;
            }
        }


        private List<string> _keyPressCaptureList = new();
        public event EventHandler<DetectKeystrokeEventArg> OnKeystrokeDetected;
        private bool _isCapturingKeyPress;
        private Guid? _panelId;

        public void StartGlobalKeyboardHook(Guid? panelId = null)
        {
            _isCapturingKeyPress = true;
            _panelId = panelId;

            if (_globalKeyboardHook != null) 
                return;

            Debug.WriteLine("Starts Global Keyboard Hook");
            _globalKeyboardHook ??= new GlobalKeyboardHook();
            _globalKeyboardHook.KeyboardPressed -= HandleGlobalKeyboardHookOnKeyboardPressed;
            _globalKeyboardHook.KeyboardPressed += HandleGlobalKeyboardHookOnKeyboardPressed;
        }

        public void EndGlobalKeyboardHook()
        {
            if (_globalKeyboardHook == null) 
                return;

            Debug.WriteLine("Ends Global Keyboard Hook");
            _keyPressCaptureList = new List<string>();
            _globalKeyboardHook.KeyboardPressed -= HandleGlobalKeyboardHookOnKeyboardPressed;
            _globalKeyboardHook?.Dispose();
            _globalKeyboardHook = null;
        }
       

        private void HandleGlobalKeyboardHookOnKeyboardPressed(object sender, GlobalKeyboardHookEventArgs e)
        {
            switch (e.KeyboardState)
            {
                case GlobalKeyboardHook.KeyboardState.KeyDown or GlobalKeyboardHook.KeyboardState.SysKeyDown:
                    _isCapturingKeyPress = true;
                    _keyPressCaptureList.Add(e.KeyboardData.Key.ToString());
                    break;
                case GlobalKeyboardHook.KeyboardState.KeyUp or GlobalKeyboardHook.KeyboardState.SysKeyUp when _isCapturingKeyPress:
                {
                    _isCapturingKeyPress = false;

                    var keyBinding = string.Join("|", _keyPressCaptureList.DistinctBy(x => x).OrderBy(x => x).ToArray().Select(x => x));
                    OnKeystrokeDetected?.Invoke(this, new DetectKeystrokeEventArg {PanelId = _panelId, KeyBinding = keyBinding});

                    _panelId = null;
                    _keyPressCaptureList.Clear();
                    break;
                }
            }
        }
    }

    public class DetectKeystrokeEventArg : EventArgs
    {
        public Guid? PanelId { get; set; }

        public string KeyBinding { get; set; }
    }
}
