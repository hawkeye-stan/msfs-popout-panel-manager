using Microsoft.Extensions.Hosting;
using System.Threading;
using System.Threading.Tasks;

namespace MSFSPopoutPanelManager.WebServer
{
    public interface IWebHost : IHost
    {
        Task RestartAsync(CancellationToken cancellationToken = default);

        bool ServerStarted { get; set; }
    }
}
