using MSFSPopoutPanelManager.Shared;

namespace MSFSPopoutPanelManager.DomainModel.Profile
{
    public class MonitorInfo : ObservableObject
    {
        public MonitorInfo()
        {
            InitializeChildPropertyChangeBinding();
        }

        public string Name { get; set; }

        public bool IsSelected { get; set; } = false;

        public int X { get; set; } = 0;

        public int Y { get; set; } = 0;

        public int Width { get; set; } = 0;

        public int Height { get; set; } = 0;
    }
}
