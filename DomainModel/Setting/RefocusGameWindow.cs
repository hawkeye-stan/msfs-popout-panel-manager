using MSFSPopoutPanelManager.Shared;

namespace MSFSPopoutPanelManager.DomainModel.Setting
{
    public class RefocusGameWindow : ObservableObject
    {
        public RefocusGameWindow()
        {
            IsEnabled = true;
            Delay = 1;
        }

        public bool IsEnabled { get; set; }

        public double Delay { get; set; }
    }
}
