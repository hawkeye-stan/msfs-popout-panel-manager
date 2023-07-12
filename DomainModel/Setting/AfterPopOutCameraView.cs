using MSFSPopoutPanelManager.Shared;
using Newtonsoft.Json;

namespace MSFSPopoutPanelManager.DomainModel.Setting
{
    public class AfterPopOutCameraView : ObservableObject
    {
        public AfterPopOutCameraView()
        {
            // Default values
            IsEnabled = true;
            CameraView = AfterPopOutCameraViewType.CockpitCenterView;
            KeyBinding = "1";
        }

        public bool IsEnabled { get; set; }

        public AfterPopOutCameraViewType CameraView { get; set; }

        public string KeyBinding { get; set; }

        // Use for MVVM binding only
        [JsonIgnore]
        public bool IsEnabledCustomCameraKeyBinding { get { return IsEnabled && CameraView == AfterPopOutCameraViewType.CustomCameraView; } }
    }

    public enum AfterPopOutCameraViewType
    {
        CockpitCenterView,
        CustomCameraView
    }
}
