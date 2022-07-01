using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using MSFSPopoutPanelManager.TouchPanel.Shared;

namespace MSFSPopoutPanelManager.TouchPanel.TouchPanelHost
{
    internal class WebHostService : IHostedService
    {
        private readonly IHostApplicationLifetime _appLifetime;

        public WebHostService(IHostApplicationLifetime appLifetime)
        {
            _appLifetime = appLifetime;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _appLifetime.ApplicationStarted.Register(OnStarted);
            _appLifetime.ApplicationStopping.Register(OnStopping);
            _appLifetime.ApplicationStopped.Register(OnStopped);

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        private void OnStarted()
        {
            TouchPanelLogger.ServerLog("Web Host server started", LogLevel.INFO);
        }

        private void OnStopping()
        {
            TouchPanelLogger.ServerLog("Web Host server stopping", LogLevel.INFO);
        }

        private void OnStopped()
        {
            TouchPanelLogger.ServerLog("Web Host server stopped", LogLevel.INFO);
        }
    }
}
