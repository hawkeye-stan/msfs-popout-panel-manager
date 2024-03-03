using MSFSPopoutPanelManager.Orchestration;
using Prism.Commands;
using System.Windows.Input;

namespace MSFSPopoutPanelManager.MainApp.ViewModel
{
    public class PreferenceDrawerViewModel : BaseViewModel
    {
        private readonly KeyboardOrchestrator _keyboardOrchestrator;

        public ICommand DetectStartPopOutKeyBindingCommand { get; }

        public bool IsDetectingKeystroke { get; set; }

        public PreferenceDrawerViewModel(SharedStorage sharedStorage, KeyboardOrchestrator keyboardOrchestrator) : base(sharedStorage)
        {
            _keyboardOrchestrator = keyboardOrchestrator;

            DetectStartPopOutKeyBindingCommand = new DelegateCommand(OnDetectStartPopOutKeyBinding);
        }

        private void KeyboardOrchestrator_OnKeystrokeDetected(object sender, DetectKeystrokeEventArg e)
        {
            IsDetectingKeystroke = false;
            AppSettingData.ApplicationSetting.KeyboardShortcutSetting.PopOutKeyboardBinding = e.KeyBinding;
            _keyboardOrchestrator.OnKeystrokeDetected -= KeyboardOrchestrator_OnKeystrokeDetected;
            _keyboardOrchestrator.EndGlobalKeyboardHook(KeyboardHookClientType.PreferenceConfigurationDetection);
        }

        private void OnDetectStartPopOutKeyBinding()
        {
            IsDetectingKeystroke = true;
            _keyboardOrchestrator.OnKeystrokeDetected += KeyboardOrchestrator_OnKeystrokeDetected;
            _keyboardOrchestrator.StartGlobalKeyboardHook(KeyboardHookClientType.PreferenceConfigurationDetection);
        }
    }
}
