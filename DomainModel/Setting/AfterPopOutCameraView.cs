using MSFSPopoutPanelManager.Shared;
using Newtonsoft.Json;

namespace MSFSPopoutPanelManager.DomainModel.Setting
{
    public class AfterPopOutCameraView : ObservableObject
    {
        public bool IsEnabled { get; set; } = true;

        public AfterPopOutCameraViewType CameraView { get; set; } = AfterPopOutCameraViewType.CockpitCenterView;

        public string KeyBinding { get; set; } = "1";

        // Use for MVVM binding only
        [JsonIgnore]
        public bool IsEnabledCustomCameraKeyBinding => IsEnabled && CameraView == AfterPopOutCameraViewType.CustomCameraView;
    }

    public enum AfterPopOutCameraViewType
    {
        CockpitCenterView,
        CustomCameraView
    }
}
