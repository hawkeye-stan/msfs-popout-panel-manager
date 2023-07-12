using MSFSPopoutPanelManager.DomainModel.Profile;
using System.Collections.ObjectModel;

namespace MSFSPopoutPanelManager.DomainModel.Legacy
{
    public class LegacyProfile
    {
        public int ProfileId { get; set; }

        public string ProfileName { get; set; }

        public ObservableCollection<string> BindingAircrafts { get; set; }

        public ObservableCollection<LegacyPanelSourceCoordinate> PanelSourceCoordinates { get; set; }

        public ObservableCollection<LegacyPanelConfig> PanelConfigs { get; set; }

        public ObservableCollection<LegacyTouchPanelBinding> TouchPanelBindings { get; set; }

        public bool IsLocked { get; set; }

        public bool PowerOnRequiredForColdStart { get; set; }

        public bool IncludeInGamePanels { get; set; }

        public bool RealSimGearGTN750Gen1Override { get; set; }

        public LegacyMsfsGameWindowConfig MsfsGameWindowConfig { get; set; }
    }

    public class LegacyPanelSourceCoordinate
    {
        public int PanelIndex { get; set; }

        public int X { get; set; }

        public int Y { get; set; }
    }

    public class LegacyPanelConfig
    {
        public int PanelIndex { get; set; }

        public string PanelName { get; set; }

        public PanelType PanelType { get; set; }

        public int Top { get; set; }

        public int Left { get; set; }

        public int Width { get; set; }

        public int Height { get; set; }

        public bool AlwaysOnTop { get; set; }

        public bool HideTitlebar { get; set; }

        public bool FullScreen { get; set; }

        public bool TouchEnabled { get; set; }

        public bool DisableGameRefocus { get; set; }
    }

    public class LegacyTouchPanelBinding
    {
        public string PlaneId { get; set; }

        public string PanelId { get; set; }
    }

    public class LegacyMsfsGameWindowConfig
    {
        public int Top { get; set; }

        public int Left { get; set; }

        public int Width { get; set; }

        public int Height { get; set; }
    }
}
