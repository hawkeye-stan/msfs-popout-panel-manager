using MSFSPopoutPanelManager.Shared;

namespace MSFSPopoutPanelManager.DomainModel.Setting
{
    public class RefocusSetting : ObservableObject
    {
        public RefocusSetting()
        {
            RefocusGameWindow = new RefocusGameWindow();

            InitializeChildPropertyChangeBinding();
        }

        public RefocusGameWindow RefocusGameWindow { get; set; }
    }
}
