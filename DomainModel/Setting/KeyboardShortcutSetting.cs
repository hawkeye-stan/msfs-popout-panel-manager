using MSFSPopoutPanelManager.Shared;

namespace MSFSPopoutPanelManager.DomainModel.Setting
{
    public class KeyboardShortcutSetting : ObservableObject
    {
        public bool IsEnabled { get; set; } = true;

        public string StartPopOutKeyBinding { get; set; } = "O";
    }
}
