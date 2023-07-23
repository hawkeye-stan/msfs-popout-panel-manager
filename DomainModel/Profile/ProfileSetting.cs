using MSFSPopoutPanelManager.Shared;

namespace MSFSPopoutPanelManager.DomainModel.Profile
{
    public class ProfileSetting : ObservableObject
    {
        public ProfileSetting()
        {
            HudBarConfig = new HudBarConfig();
            RefocusOnDisplay = new RefocusOnDisplay();

            InitializeChildPropertyChangeBinding();
        }

        public bool PowerOnRequiredForColdStart { get; set; }

        public bool IncludeInGamePanels { get; set; }

        public HudBarConfig HudBarConfig { get; set; }

        public RefocusOnDisplay RefocusOnDisplay { get; set; }
    }
}
