using MSFSPopoutPanelManager.WindowsAgent;

namespace MSFSPopoutPanelManager.Orchestration
{
    public class KeyboardOrchestrator : BaseOrchestrator
    {
        private readonly PanelPopOutOrchestrator _panelPopOutOrchestrator;

        public KeyboardOrchestrator(SharedStorage sharedStorage, PanelPopOutOrchestrator panelPopOutOrchestrator) : base(sharedStorage)
        {
            _panelPopOutOrchestrator = panelPopOutOrchestrator;
        }

        public void Initialize()
        {
            if (AppSettingData.ApplicationSetting.KeyboardShortcutSetting.IsEnabled)
            {
                InputHookManager.StartKeyboardHook();
                InputHookManager.OnKeyUp -= HandleKeyboardHookKeyUpEvent;
                InputHookManager.OnKeyUp += HandleKeyboardHookKeyUpEvent;
            }

            AppSettingData.ApplicationSetting.OnIsUsedKeyboardShortcutChanged += (_, e) =>
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

        public async void HandleKeyboardHookKeyUpEvent(object sender, KeyUpEventArgs e)
        {
            // Start pop out
            if (e.IsHoldControl && e.IsHoldShift && e.KeyCode.ToUpper() == AppSettingData.ApplicationSetting.KeyboardShortcutSetting.StartPopOutKeyBinding)
                await _panelPopOutOrchestrator.ManualPopOut();
        }
    }
}
