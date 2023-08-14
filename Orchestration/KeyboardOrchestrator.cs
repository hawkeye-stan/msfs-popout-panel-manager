using MSFSPopoutPanelManager.Shared;
using MSFSPopoutPanelManager.WindowsAgent;

namespace MSFSPopoutPanelManager.Orchestration
{
    public class KeyboardOrchestrator : ObservableObject
    {
        private AppSettingData _appSettingData;
        private FlightSimData _flightSimData;

        public KeyboardOrchestrator(AppSettingData appSettingData, FlightSimData flightSimData)
        {
            _appSettingData = appSettingData;
            _flightSimData = flightSimData;
        }

        internal PanelPopOutOrchestrator PanelPopOutOrchestrator { get; set; }

        public void Initialize()
        {
            if (_appSettingData.ApplicationSetting.KeyboardShortcutSetting.IsEnabled)
            {
                InputHookManager.StartKeyboardHook();
                InputHookManager.OnKeyUp -= HandleKeyboardHookKeyUpEvent;
                InputHookManager.OnKeyUp += HandleKeyboardHookKeyUpEvent;
            }

            _appSettingData.ApplicationSetting.IsUsedKeyboardShortcutChanged += (sender, e) =>
            {
                if (e)
                {
                    InputHookManager.StartKeyboardHook();
                    InputHookManager.OnKeyUp -= HandleKeyboardHookKeyUpEvent;
                    InputHookManager.OnKeyUp += HandleKeyboardHookKeyUpEvent;
                }
                else
                {
                    InputHookManager.EndKeyboardHook();
                    InputHookManager.OnKeyUp -= HandleKeyboardHookKeyUpEvent;
                }
            };
        }

        public void HandleKeyboardHookKeyUpEvent(object sender, KeyUpEventArgs e)
        {
            // Start pop out
            if (e.IsHoldControl && e.IsHoldShift && e.KeyCode.ToUpper() == _appSettingData.ApplicationSetting.KeyboardShortcutSetting.StartPopOutKeyBinding)
                PanelPopOutOrchestrator.ManualPopOut();
        }
    }
}
