using MSFSPopoutPanelManager.Shared;

namespace MSFSPopoutPanelManager.DomainModel.Setting
{
    public class WindowedModeSetting : ObservableObject
    {
        public WindowedModeSetting()
        {
            AutoResizeMsfsGameWindow = true;
        }

        public bool AutoResizeMsfsGameWindow { get; set; }
    }
}
