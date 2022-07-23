namespace MSFSPopoutPanelManager.WebServer
{
    public class TouchPanelConfigSetting
    {
        public TouchPanelConfigSetting()
        {
            // Default values
            DataRefreshInterval = 100;
            MapRefreshInterval = 250;
            IsUsedArduino = false;
            IsEnabledSound = true;
        }

        public int DataRefreshInterval { get; set; }

        public int MapRefreshInterval { get; set; }

        public bool IsUsedArduino { get; set; }

        public bool IsEnabledSound { get; set; }
    }
}
