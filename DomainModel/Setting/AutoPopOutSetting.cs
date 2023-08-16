using MSFSPopoutPanelManager.Shared;

namespace MSFSPopoutPanelManager.DomainModel.Setting
{
    public class AutoPopOutSetting : ObservableObject
    {
        public AutoPopOutSetting()
        {
            IsEnabled = true;
            ReadyToFlyDelay = 3;
        }

        public bool IsEnabled { get; set; }

        public int ReadyToFlyDelay { get; set; }
    }
}
