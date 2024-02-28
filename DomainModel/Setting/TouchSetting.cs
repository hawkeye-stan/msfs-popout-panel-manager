using MSFSPopoutPanelManager.Shared;

namespace MSFSPopoutPanelManager.DomainModel.Setting
{
    public class TouchSetting : ObservableObject
    {
        public int TouchDownUpDelay { get; set; } = 0;
    }
}
