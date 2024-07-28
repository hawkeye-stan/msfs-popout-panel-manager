using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace MSFSPopoutPanelManager.Orchestration
{
    public class KeyboardOrchestrator : BaseOrchestrator
    {
        private GlobalKeyboardHook _globalKeyboardHook;

        private readonly List<KeyboardHookClientType> _keyboardHookClients = new();
        private List<string> _keyPressCaptureList = new();
        private bool _isCapturingKeyPress;
        private KeyboardHookClientType _clientType;
        private Guid? _panelId;

        public event EventHandler<DetectKeystrokeEventArg> OnKeystrokeDetected;

        public KeyboardOrchestrator(SharedStorage sharedStorage) : base(sharedStorage)
        {
        }
        
        public void Initialize()
        {
            if (AppSettingData.ApplicationSetting.KeyboardShortcutSetting.IsEnabled)
            {
                StartGlobalKeyboardHook(KeyboardHookClientType.StartPopOutKeyboardShortcut);
            }

            AppSettingData.ApplicationSetting.OnIsUsedKeyboardShortcutChanged += (_, e) =>
            {
                if (e)
                    StartGlobalKeyboardHook(KeyboardHookClientType.StartPopOutKeyboardShortcut);
                else
                    EndGlobalKeyboardHook(KeyboardHookClientType.StartPopOutKeyboardShortcut);
            };
        }
        
        public void StartGlobalKeyboardHook(KeyboardHookClientType clientType, Guid? panelId = null)
        {
            if(!_keyboardHookClients.Exists(x => x == clientType))
                _keyboardHookClients.Add(clientType);

            _clientType = clientType;
            _isCapturingKeyPress = true;
            _panelId = panelId;

            if (_globalKeyboardHook != null) 
                return;

            Debug.WriteLine("Starts Global Keyboard Hook");
            _globalKeyboardHook ??= new GlobalKeyboardHook();
            _globalKeyboardHook.OnKeyboardPressed -= HandleGlobalKeyboardHookOnKeyboardPressed;
            _globalKeyboardHook.OnKeyboardPressed += HandleGlobalKeyboardHookOnKeyboardPressed;
        }

        public void EndGlobalKeyboardHook(KeyboardHookClientType clientType)
        {
            _keyboardHookClients.Remove(clientType);

            if (_globalKeyboardHook == null || _keyboardHookClients.Count > 0) 
                return;

            EndGlobalKeyboardHookForced();
        }

        public void EndGlobalKeyboardHookForced()
        {
            Debug.WriteLine("Ends Global Keyboard Hook (Forced)");
            _keyPressCaptureList = new List<string>();

            if (_globalKeyboardHook != null)
            {
                _globalKeyboardHook.OnKeyboardPressed -= HandleGlobalKeyboardHookOnKeyboardPressed;
                _globalKeyboardHook?.Dispose();
                _globalKeyboardHook = null;
            }
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

                    if(CheckForRegisteredKeystrokeEvent(keyBinding))
                        OnKeystrokeDetected?.Invoke(this, new DetectKeystrokeEventArg {PanelId = _panelId, KeyBinding = keyBinding});

                    _clientType = KeyboardHookClientType.Unknown;
                    _panelId = null;
                    _keyPressCaptureList.Clear();
                    break;
                }
            }
        }

        private bool CheckForRegisteredKeystrokeEvent(string keyBinding)
        {
            if (_clientType is KeyboardHookClientType.FloatingPanelDetection or KeyboardHookClientType.PreferenceConfigurationDetection)
                return true;

            if (!FlightSimData.IsFlightStarted)
                return false;

            // Check if keystrokes are registered keyboard events
            bool isRegistered = AppSettingData.ApplicationSetting.KeyboardShortcutSetting.IsEnabled;

            if (ProfileData.ActiveProfile != null)
            {
                foreach (var panelConfig in ProfileData.ActiveProfile.PanelConfigs)
                {
                    if (panelConfig.FloatingPanel.IsEnabled && 
                        panelConfig.PanelHandle != IntPtr.MaxValue && 
                        panelConfig.PanelHandle != IntPtr.Zero &&  
                        panelConfig.FloatingPanel.KeyboardBinding == keyBinding)
                        isRegistered = true;
                }
            }

            return isRegistered;
        }
    }

    public class DetectKeystrokeEventArg : EventArgs
    {
        public Guid? PanelId { get; set; }

        public string KeyBinding { get; set; }
    }

    public enum KeyboardHookClientType
    {
        Unknown,
        PreferenceConfigurationDetection,
        StartPopOutKeyboardShortcut,
        FloatingPanelDetection,
        FloatingPanel
    }
}
