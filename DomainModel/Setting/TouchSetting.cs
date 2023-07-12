using MSFSPopoutPanelManager.Shared;

namespace MSFSPopoutPanelManager.DomainModel.Setting
{
    public class TouchSetting : ObservableObject
    {
        public TouchSetting()
        {
            TouchDownUpDelay = 0;
        }

        public int TouchDownUpDelay { get; set; }
    }
}
