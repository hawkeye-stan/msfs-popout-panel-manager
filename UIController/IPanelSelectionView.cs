using System.Windows.Forms;

namespace MSFSPopoutPanelManager.UIController
{
    public interface IPanelSelectionView
    {
        public Form Form { get; }

        public bool ShowPanelLocationOverlay { get; set; }
    }
}
