using MSFSPopoutPanelManager.Shared;

namespace MSFSPopoutPanelManager.DomainModel.Setting
{
    public class TrackIRSetting : ObservableObject
    {
        public bool AutoDisableTrackIR { get; set; } = false;
    }
}
