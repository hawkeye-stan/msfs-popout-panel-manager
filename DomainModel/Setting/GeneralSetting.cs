using MSFSPopoutPanelManager.Shared;
using Newtonsoft.Json;

namespace MSFSPopoutPanelManager.DomainModel.Setting
{
    public class GeneralSetting : ObservableObject
    {
        public GeneralSetting()
        {
            AlwaysOnTop = true;
            MinimizeToTray = false;
            StartMinimized = false;
            AutoClose = true;
            CheckForUpdate = true;

            InitializeChildPropertyChangeBinding();
        }

        public bool AlwaysOnTop { get; set; }

        public bool AutoClose { get; set; }

        public bool MinimizeToTray { get; set; }

        public bool StartMinimized { get; set; }

        public bool CheckForUpdate { get; set; }

        [JsonIgnore, IgnorePropertyChanged]
        public bool AutoStart
        {
            get
            {
                return AppAutoStart.CheckIsAutoStart();
            }
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
