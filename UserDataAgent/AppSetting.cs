using MSFSPopoutPanelManager.Shared;
using Newtonsoft.Json;
using PropertyChanged;

namespace MSFSPopoutPanelManager.UserDataAgent
{
    public class AppSetting : ObservableObject
    {
        public AppSetting()
        {
            // Set defaults
            LastUsedProfileId = -1;
            AutoUpdaterUrl = "https://raw.githubusercontent.com/hawkeye-stan/msfs-popout-panel-manager/master/autoupdate.xml";

            AlwaysOnTop = true;
            MinimizeToTray = false;
            StartMinimized = false;
            AutoClose = true;

            AutoPopOutPanels = true;

            UseAutoPanning = true;
            AutoPanningKeyBinding = "0";
            MinimizeAfterPopOut = false;
            OnScreenMessageDuration = 1;
            UseLeftRightControlToPopOut = false;
            AfterPopOutCameraView = new AfterPopOutCameraView();
            AfterPopOutCameraView.PropertyChanged += (source, e) =>
            {
                var arg = e as PropertyChangedExtendedEventArgs;
                OnPropertyChanged(arg.PropertyName, arg.OldValue, arg.NewValue);
            };

            AutoDisableTrackIR = true;

            AutoResizeMsfsGameWindow = true;

            TouchScreenSettings = new TouchScreenSettings();
            TouchScreenSettings.PropertyChanged += (source, e) =>
            {
                var arg = e as PropertyChangedExtendedEventArgs;
                OnPropertyChanged(arg.PropertyName, arg.OldValue, arg.NewValue);
            };

            IsEnabledTouchPanelServer = false;
            TouchPanelSettings = new TouchPanelSettings();
            TouchPanelSettings.PropertyChanged += (source, e) =>
            {
                var arg = e as PropertyChangedExtendedEventArgs;
                OnPropertyChanged(arg.PropertyName, arg.OldValue, arg.NewValue);
            };
        }

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

        public AfterPopOutCameraView AfterPopOutCameraView { get; set; }

        public TouchScreenSettings TouchScreenSettings { get; set; }

        public TouchPanelSettings TouchPanelSettings { get; set; }


        [JsonIgnore, DoNotNotify]
        public bool AutoStart
        {
            get
            {
                return AppAutoStart.CheckIsAutoStart();
            }
            set
            {
                if (value)
                    AppAutoStart.Activate();
                else
                    AppAutoStart.Deactivate();
            }
        }
    }

    public class AfterPopOutCameraView : ObservableObject
    {
        public AfterPopOutCameraView()
        {
            // Default values
            EnableReturnToCameraView = true;
            CameraView = AfterPopOutCameraViewType.CockpitCenterView;
            CustomCameraKeyBinding = "1";
        }

        public bool EnableReturnToCameraView { get; set; }

        public AfterPopOutCameraViewType CameraView { get; set; }

        public string CustomCameraKeyBinding { get; set; }

        // Use for MVVM binding only
        [JsonIgnore]
        public bool IsReturnToCustomCameraView { get { return CameraView == AfterPopOutCameraViewType.CustomCameraView; } }

        // Use for MVVM binding only
        [JsonIgnore]
        public bool IsEnabledCustomCameraKeyBinding { get { return EnableReturnToCameraView && CameraView == AfterPopOutCameraViewType.CustomCameraView; } }
    }

    public enum AfterPopOutCameraViewType
    {
        CockpitCenterView,
        CustomCameraView
    }

    public class TouchScreenSettings : ObservableObject
    {
        public TouchScreenSettings()
        {
            TouchDownUpDelay = 0;
            RefocusGameWindow = true;
            RefocusGameWindowDelay = 500;
            RealSimGearGTN750Gen1Override = false;
        }

        public int TouchDownUpDelay { get; set; }

        public bool RefocusGameWindow { get; set; }

        public int RefocusGameWindowDelay { get; set; }

        public bool RealSimGearGTN750Gen1Override { get; set; }
    }

    public class TouchPanelSettings : ObservableObject
    {
        public TouchPanelSettings()
        {
            // Default values
            EnableTouchPanelIntegration = false;
            DataRefreshInterval = 200;
            MapRefreshInterval = 1000;
            UseArduino = false;
            EnableSound = true;
        }

        public bool EnableTouchPanelIntegration { get; set; }

        public bool AutoStart { get; set; }

        public int DataRefreshInterval { get; set; }

        public int MapRefreshInterval { get; set; }

        public bool UseArduino { get; set; }

        public bool EnableSound { get; set; }
    }
}
