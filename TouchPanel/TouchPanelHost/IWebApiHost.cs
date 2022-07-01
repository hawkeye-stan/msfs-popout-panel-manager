using Microsoft.Extensions.Hosting;
using MSFSPopoutPanelManager.TouchPanel.Shared;
using System.Threading;
using System.Threading.Tasks;

namespace MSFSPopoutPanelManager.TouchPanel.TouchPanelHost
{
    public interface IWebApiHost : IHost
    {
        Task RestartAsync(CancellationToken cancellationToken = default);

        public TouchPanelConfigSetting TouchPanelConfigSetting { get; set; }
    }
}
