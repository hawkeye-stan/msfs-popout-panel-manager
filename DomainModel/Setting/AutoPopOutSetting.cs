using MSFSPopoutPanelManager.Shared;

namespace MSFSPopoutPanelManager.DomainModel.Setting
{
    public class AutoPopOutSetting : ObservableObject
    {
        public bool IsEnabled { get; set; } = true;

        public int ReadyToFlyDelay { get; set; } = 3;
    }
}
