using System.Linq;
using System.Security.Cryptography.X509Certificates;
using MSFSPopoutPanelManager.DomainModel.Profile;
using MSFSPopoutPanelManager.WindowsAgent;

namespace MSFSPopoutPanelManager.Orchestration
{
    public class KeyboardOrchestrator : BaseOrchestrator
    {
        private readonly PanelPopOutOrchestrator _panelPopOutOrchestrator;
        private readonly PanelConfigurationOrchestrator _panelConfigurationOrchestrator;

        public KeyboardOrchestrator(SharedStorage sharedStorage, PanelPopOutOrchestrator panelPopOutOrchestrator, PanelConfigurationOrchestrator panelConfigurationOrchestrator) : base(sharedStorage)
        {
            _panelPopOutOrchestrator = panelPopOutOrchestrator;
            _panelConfigurationOrchestrator = panelConfigurationOrchestrator;
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

            if (ProfileData.ActiveProfile.PanelConfigs.Any(x => x.FloatingPanel.IsEnabled))
            {
                InputHookManager.StartKeyboardHook("FloatingPanel");
                InputHookManager.OnKeyUp -= HandleFloatingPanelKeyboardHookKeyUpEvent;
                InputHookManager.OnKeyUp += HandleFloatingPanelKeyboardHookKeyUpEvent;
            }


            ProfileData.ActiveProfile.OnUseFloatingPanelChanged += (_, e) =>
            {
                if (e)
                {
                    InputHookManager.StartKeyboardHook("FloatingPanel");
                    InputHookManager.OnKeyUp -= HandleFloatingPanelKeyboardHookKeyUpEvent;
                    InputHookManager.OnKeyUp += HandleFloatingPanelKeyboardHookKeyUpEvent;
                }
                else
                {
                    InputHookManager.EndKeyboardHook("FloatingPanel");
                    InputHookManager.OnKeyUp -= HandleFloatingPanelKeyboardHookKeyUpEvent;
                }
            };
        }

        public async void HandleShortcutKeyboardHookKeyUpEvent(object sender, KeyUpEventArgs e)
        {
            // Start pop out
            if (e.IsHoldControl && e.IsHoldShift && e.KeyCode.ToUpper() ==
                AppSettingData.ApplicationSetting.KeyboardShortcutSetting.StartPopOutKeyBinding)
            {
                await _panelPopOutOrchestrator.ManualPopOut();
                return;
            }
        }

        public void HandleFloatingPanelKeyboardHookKeyUpEvent(object sender, KeyUpEventArgs e)
        {
            if (e.IsHoldControl)
            {
                switch (e.KeyCode)
                {
                    case "D1":
                    case "D2":
                    case "D3":
                    case "D4":
                    case "D5":
                    case "D6":
                    case "D7":
                    case "D8":
                    case "D9":
                    case "D0":
                        _panelConfigurationOrchestrator.ToggleFloatPanel($"Ctrl-{e.KeyCode[1..]}");
                        break;
                    case "NumPad1":
                    case "NumPad2":
                    case "NumPad3":
                    case "NumPad4":
                    case "NumPad5":
                    case "NumPad6":
                    case "NumPad7":
                    case "NumPad8":
                    case "NumPad9":
                    case "NumPad0":
                        _panelConfigurationOrchestrator.ToggleFloatPanel($"Ctrl-{e.KeyCode[6..]}");
                        break;
                }
            }
        }
    }
}
