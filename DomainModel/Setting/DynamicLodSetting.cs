using MSFSPopoutPanelManager.Shared;

namespace MSFSPopoutPanelManager.DomainModel.Setting
{
    public class DynamicLodSetting : ObservableObject
    {
        public DynamicLodSetting()
        {
            InitializeChildPropertyChangeBinding();
        }

        public bool IsEnabled { get; set; } = false;

        public bool PauseWhenMsfsLoseFocus { get; set; } = true;

        public bool PauseOutsideCockpitView { get; set; } = true;

        public bool ResetEnabled { get; set; } = false;

        public int ResetTlod { get; set; } = 100;

        public int ResetOlod { get; set; } = 100;

        public int TargetedFps { get; set; } = 60;

        public int FpsTolerance { get; set; } = 4;

        public bool TlodMinOnGround { get; set; } = true;

        public int AltTlodBase { get; set; } = 1000;

        public int TlodMin { get; set; } = 50;

        public int TlodMax { get; set; } = 200;

        public int CloudRecoveryTlod { get; set; } = 100;

        public bool DecreaseCloudQuality { get; set; } = false;

        public int OlodTop { get; set; } = 50;

        public int OlodBase { get; set; } = 200;

        public int AltOlodBase { get; set; } = 1000; 

        public int AltOlodTop { get; set; } = 10000;
    }
}
