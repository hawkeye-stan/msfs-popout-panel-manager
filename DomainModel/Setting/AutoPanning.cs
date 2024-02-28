using MSFSPopoutPanelManager.Shared;

namespace MSFSPopoutPanelManager.DomainModel.Setting
{
    public class AutoPanning : ObservableObject
    {
        public bool IsEnabled { get; set; } = true;

        public string KeyBinding { get; set; } = "0";
    }
}
