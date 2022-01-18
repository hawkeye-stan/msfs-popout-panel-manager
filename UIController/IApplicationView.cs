using System.Windows.Forms;

namespace MSFSPopoutPanelManager.UIController
{
    public interface IApplicationView
    {
        public Form Form { get; }

        public IPanelSelectionView PanelSelection { get; }

        public IPanelConfigurationView PanelConfiguration { get; }

        public bool MinimizeToTray { get; set; }
  
        public bool AlwaysOnTop { get; set; }
        
        public bool AutoStart { get; set; }
        
        public bool AutoPanning { get; set; }
    }
}
