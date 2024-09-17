using System.Collections.Generic;
using MSFSPopoutPanelManager.Shared;

namespace MSFSPopoutPanelManager.DomainModel.Profile
{
    public class SwitchWindowConfig : ObservableObject
    {
        public bool IsEnabled { get; set; } = false;

        public List<SwitchWindowPanel> Panels { get; set; }
    }

    public class SwitchWindowPanel
    {
        public string DisplayName { get; set; }

        public string PanelCaption { get; set; }
    }
}