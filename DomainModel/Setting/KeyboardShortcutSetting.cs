using MSFSPopoutPanelManager.Shared;

namespace MSFSPopoutPanelManager.DomainModel.Setting
{
    public class KeyboardShortcutSetting : ObservableObject
    {
        public KeyboardShortcutSetting()
        {
            IsEnabled = true;
            StartPopOutKeyBinding = "O";
        }

        public bool IsEnabled { get; set; }

        public string StartPopOutKeyBinding { get; set; }
    }
}
