using MSFSPopoutPanelManager.Shared;
using System;

namespace MSFSPopoutPanelManager.DomainModel.Setting
{
    public class SystemSetting : ObservableObject
    {
        public SystemSetting()
        {
            LastUsedProfileId = Guid.Empty;
            AutoUpdaterUrl = "https://raw.githubusercontent.com/hawkeye-stan/msfs-popout-panel-manager/master/autoupdate.xml";
        }

        public string AutoUpdaterUrl { get; set; }

        public Guid LastUsedProfileId { get; set; }
    }
}
