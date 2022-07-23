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
            AutoUpdaterUrl = "https://raw.githubusercontent.com/hawkeye-stan/msfs-popout-panel-manager/master/autoupdate.xml";
            LastUsedProfileId = -1;
            MinimizeToTray = false;
            AlwaysOnTop = true;
            UseAutoPanning = true;
            AutoPanningKeyBinding = "0";
            StartMinimized = false;
            AutoDisableTrackIR = true;
            AutoPopOutPanels = true;
            OnScreenMessageDuration = 1;
            UseLeftRightControlToPopOut = false;
            IsEnabledTouchPanelServer = false;

            AfterPopOutCameraView = new AfterPopOutCameraView();
            AfterPopOutCameraView.PropertyChanged += (source, e) =>
            {
                var arg = e as PropertyChangedExtendedEventArgs;
                OnPropertyChanged(arg.PropertyName, arg.OldValue, arg.NewValue);
            };

            TouchScreenSettings = new TouchScreenSettings();
            TouchScreenSettings.PropertyChanged += (source, e) =>
            {
                var arg = e as PropertyChangedExtendedEventArgs;
                OnPropertyChanged(arg.PropertyName, arg.OldValue, arg.NewValue);
            };

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

        public bool UseAutoPanning { get; set; }

        public string AutoPanningKeyBinding { get; set; }

        public bool StartMinimized { get; set; }

        public bool IncludeBuiltInPanel { get; set; }

        public bool AutoPopOutPanels { get; set; }

        public bool AutoDisableTrackIR { get; set; }

        public int OnScreenMessageDuration { get; set; }

        public bool UseLeftRightControlToPopOut { get; set; }

        public bool IsEnabledTouchPanelServer { get; set; }

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
            MouseUpDownDelay = 25;
            RefocusGameWindow = true;
        }

        public int MouseUpDownDelay { get; set; }

        public bool RefocusGameWindow { get; set; }
    }

    public class TouchPanelSettings : ObservableObject
    {
        public TouchPanelSettings()
        {
            // Default values
            EnableTouchPanelIntegration = false;
            DataRefreshInterval = 200;
            MapRefreshInterval = 250;
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
