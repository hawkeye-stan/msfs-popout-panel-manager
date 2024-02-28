using MSFSPopoutPanelManager.Shared;
using System;

namespace MSFSPopoutPanelManager.DomainModel.Setting
{
    public class SystemSetting : ObservableObject
    {
        public string AutoUpdaterUrl { get; set; } = "https://raw.githubusercontent.com/hawkeye-stan/msfs-popout-panel-manager/master/autoupdate.xml";

        public Guid LastUsedProfileId { get; set; } = Guid.Empty;
    }
}
