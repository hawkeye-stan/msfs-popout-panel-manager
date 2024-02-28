using MSFSPopoutPanelManager.Shared;

namespace MSFSPopoutPanelManager.DomainModel.Setting
{
    public class WindowedModeSetting : ObservableObject
    {
        public bool AutoResizeMsfsGameWindow { get; set; } = true;
    }
}
