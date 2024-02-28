using MSFSPopoutPanelManager.Shared;
using Newtonsoft.Json;

namespace MSFSPopoutPanelManager.DomainModel.Setting
{
    public class GeneralSetting : ObservableObject
    {
        public GeneralSetting()
        {
            InitializeChildPropertyChangeBinding();
        }

        public bool AlwaysOnTop { get; set; } = true;

        public bool AutoClose { get; set; } = true;

        public bool MinimizeToTray { get; set; } = false;

        public bool StartMinimized { get; set; } = false;

        public bool CheckForUpdate { get; set; } = true;

        public bool TurboMode { get; set; } = false;

        [JsonIgnore, IgnorePropertyChanged]
        public bool AutoStart
        {
            get => AppAutoStart.CheckIsAutoStart();
            set
            {
                if (value)
                    AppAutoStart.Activate();
                else
                    AppAutoStart.Deactivate();
            }
        }
    }
}
