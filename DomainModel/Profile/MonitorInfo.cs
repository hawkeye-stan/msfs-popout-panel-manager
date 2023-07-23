using MSFSPopoutPanelManager.Shared;

namespace MSFSPopoutPanelManager.DomainModel.Profile
{
    public class MonitorInfo : ObservableObject
    {
        public MonitorInfo()
        {
            IsSelected = false;

            InitializeChildPropertyChangeBinding();
        }

        public string Name { get; set; }

        public bool IsSelected { get; set; }

        public int X { get; set; }

        public int Y { get; set; }

        public int Width { get; set; }

        public int Height { get; set; }
    }
}
