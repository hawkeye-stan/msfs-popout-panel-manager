using MSFSPopoutPanelManager.Shared;

namespace MSFSPopoutPanelManager.DomainModel.Profile
{
    public class HudBarConfig : ObservableObject
    {
        public HudBarConfig()
        {
            IsEnabled = false;
            HudBarType = HudBarType.Generic_Aircraft;
        }

        public bool IsEnabled { get; set; }

        public HudBarType HudBarType { get; set; }
    }

    public enum HudBarType
    {
        None = 0,       // not selectable
        Generic_Aircraft = 1,
        PMDG_737 = 2
    }
}
