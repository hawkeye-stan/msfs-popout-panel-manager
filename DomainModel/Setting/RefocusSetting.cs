using MSFSPopoutPanelManager.Shared;

namespace MSFSPopoutPanelManager.DomainModel.Setting
{
    public class RefocusSetting : ObservableObject
    {
        public RefocusSetting()
        {
            InitializeChildPropertyChangeBinding();
        }

        public RefocusGameWindow RefocusGameWindow { get; set; } = new();
    }
}
