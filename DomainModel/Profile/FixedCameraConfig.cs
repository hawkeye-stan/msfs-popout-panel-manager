using MSFSPopoutPanelManager.Shared;

namespace MSFSPopoutPanelManager.DomainModel.Profile
{
    public class FixedCameraConfig : ObservableObject
    {
        public int Id { get; set; } = 0;

        public CameraType CameraType { get; set; } = CameraType.Cockpit;

        public int CameraIndex { get; set; } = 1;

        public string Name { get; set; } = "Cockpit Pilot";
    }

    public enum CameraType
    {
        Cockpit = 1,
        Instrument = 2
    }
}
