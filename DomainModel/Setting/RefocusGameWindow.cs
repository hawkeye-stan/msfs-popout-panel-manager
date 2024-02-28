using MSFSPopoutPanelManager.Shared;

namespace MSFSPopoutPanelManager.DomainModel.Setting
{
    public class RefocusGameWindow : ObservableObject
    {
        public bool IsEnabled { get; set; } = true;

        public double Delay { get; set; } = 1;
    }
}
