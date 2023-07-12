using MSFSPopoutPanelManager.Shared;

namespace MSFSPopoutPanelManager.DomainModel.Setting
{
    public class ApplicationSetting : ObservableObject
    {
        public ApplicationSetting()
        {
            GeneralSetting = new GeneralSetting();
            AutoPopOutSetting = new AutoPopOutSetting();
            PopOutSetting = new PopOutSetting();
            RefocusSetting = new RefocusSetting();
            TouchSetting = new TouchSetting();
            TrackIRSetting = new TrackIRSetting();
            WindowedModeSetting = new WindowedModeSetting();
            SystemSetting = new SystemSetting();

            InitializeChildPropertyChangeBinding();
        }

        public GeneralSetting GeneralSetting { get; set; }

        public AutoPopOutSetting AutoPopOutSetting { get; set; }

        public PopOutSetting PopOutSetting { get; set; }

        public RefocusSetting RefocusSetting { get; set; }

        public TouchSetting TouchSetting { get; set; }

        public TrackIRSetting TrackIRSetting { get; set; }

        public WindowedModeSetting WindowedModeSetting { get; set; }

        public SystemSetting SystemSetting { get; set; }
    }
}
