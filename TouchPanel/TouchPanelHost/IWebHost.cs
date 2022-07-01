using Microsoft.Extensions.Hosting;
using MSFSPopoutPanelManager.TouchPanel.Shared;
using System.Threading;
using System.Threading.Tasks;

namespace MSFSPopoutPanelManager.TouchPanel.TouchPanelHost
{
    public interface IWebHost : IHost
    {
        Task RestartAsync(CancellationToken cancellationToken = default);
    }
}
