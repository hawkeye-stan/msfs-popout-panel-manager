using MSFSPopoutPanelManager.Shared;

namespace MSFSPopoutPanelManager.DomainModel.Setting
{
    public class TrackIRSetting : ObservableObject
    {
        public TrackIRSetting()
        {
            AutoDisableTrackIR = true;
        }

        public bool AutoDisableTrackIR { get; set; }
    }
}
