using MSFSPopoutPanelManager.Shared;

namespace MSFSPopoutPanelManager.DomainModel.Profile
{
    public class FloatingPanel : ObservableObject
    {
        public bool IsEnabled { get; set; }

        public string KeyBinding { get; set; }
    }
}
