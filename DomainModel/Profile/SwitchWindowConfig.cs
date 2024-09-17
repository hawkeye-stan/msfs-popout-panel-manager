using MSFSPopoutPanelManager.Shared;

namespace MSFSPopoutPanelManager.DomainModel.Profile
{
    public class SwitchWindowConfig : ObservableObject
    {
        public bool IsEnabled { get; set; } = false;
    }
}