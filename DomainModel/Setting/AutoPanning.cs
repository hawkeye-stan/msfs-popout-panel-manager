using MSFSPopoutPanelManager.Shared;

namespace MSFSPopoutPanelManager.DomainModel.Setting
{
    public class AutoPanning : ObservableObject
    {
        public AutoPanning()
        {
            IsEnabled = true;
            KeyBinding = "0";
        }

        public bool IsEnabled { get; set; }

        public string KeyBinding { get; set; }
    }
}
