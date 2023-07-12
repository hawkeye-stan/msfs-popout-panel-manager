using MSFSPopoutPanelManager.DomainModel.Setting;

namespace MSFSPopoutPanelManager.DomainModel.Legacy
{
    public class LegacyAppSetting
    {
        public string AutoUpdaterUrl { get; set; }

        public int LastUsedProfileId { get; set; }

        public bool MinimizeToTray { get; set; }

        public bool AlwaysOnTop { get; set; }

        public bool AutoClose { get; set; }

        public bool UseAutoPanning { get; set; }

        public bool MinimizeAfterPopOut { get; set; }

        public string AutoPanningKeyBinding { get; set; }

        public bool StartMinimized { get; set; }

        public bool AutoPopOutPanels { get; set; }

        public bool AutoDisableTrackIR { get; set; }

        public int OnScreenMessageDuration { get; set; }

        public bool UseLeftRightControlToPopOut { get; set; }

        public bool IsEnabledTouchPanelServer { get; set; }

        public bool AutoResizeMsfsGameWindow { get; set; }

        public LegacyAfterPopOutCameraView AfterPopOutCameraView { get; set; }

        public LegacyTouchScreenSettings TouchScreenSettings { get; set; }

        public LegacyTouchPanelSettings TouchPanelSettings { get; set; }
    }

    public class LegacyAfterPopOutCameraView
    {
        public bool EnableReturnToCameraView { get; set; }

        public AfterPopOutCameraViewType CameraView { get; set; }

        public string CustomCameraKeyBinding { get; set; }
    }

    public class LegacyTouchScreenSettings
    {
        public int TouchDownUpDelay { get; set; }

        public bool RefocusGameWindow { get; set; }

        public int RefocusGameWindowDelay { get; set; }

        public bool RealSimGearGTN750Gen1Override { get; set; }
    }

    public class LegacyTouchPanelSettings
    {
        public bool EnableTouchPanelIntegration { get; set; }

        public bool AutoStart { get; set; }

        public int DataRefreshInterval { get; set; }

        public int MapRefreshInterval { get; set; }

        public bool UseArduino { get; set; }

        public bool EnableSound { get; set; }
    }
}
