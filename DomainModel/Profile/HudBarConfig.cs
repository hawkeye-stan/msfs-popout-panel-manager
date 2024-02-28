using MSFSPopoutPanelManager.Shared;

namespace MSFSPopoutPanelManager.DomainModel.Profile
{
    public class HudBarConfig : ObservableObject
    {
        public bool IsEnabled { get; set; } = false;

        public HudBarType HudBarType { get; set; } = HudBarType.Generic_Aircraft;
    }

    public enum HudBarType
    {
        None = 0,       // not selectable
        Generic_Aircraft = 1,
        PMDG_737 = 2
    }
}
