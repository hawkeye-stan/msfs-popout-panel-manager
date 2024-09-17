using MSFSPopoutPanelManager.Shared;

namespace MSFSPopoutPanelManager.DomainModel.Profile
{
    public class ProfileSetting : ObservableObject
    {
        public ProfileSetting()
        {
            InitializeChildPropertyChangeBinding();
        }

        public bool PowerOnRequiredForColdStart { get; set; }

        public bool IncludeInGamePanels { get; set; }

        public HudBarConfig HudBarConfig { get; set; } = new();

        public RefocusOnDisplay RefocusOnDisplay { get; set; } = new();

        public NumPadConfig NumPadConfig { get; set; } = new();

        public SwitchWindowConfig SwitchWindowConfig { get; set; } = new();
    }
}
