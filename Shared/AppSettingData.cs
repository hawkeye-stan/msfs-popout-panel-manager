using Newtonsoft.Json;

namespace MSFSPopoutPanelManager.Shared
{
    public class AppSettingData
    {
        public AppSettingData()
        {
            // Set defaults
            MinimizeToTray = false;
            AlwaysOnTop = true;
            UseAutoPanning = true;
        }

        public bool MinimizeToTray { get; set; }

        public bool AlwaysOnTop { get; set; }

        public bool UseAutoPanning { get; set; }

        [JsonIgnore]
        public bool AutoStart { get; set; }
    }
}
