using Microsoft.Extensions.Hosting;
using MSFSPopoutPanelManager.Shared;
using System.Threading;
using System.Threading.Tasks;

namespace MSFSPopoutPanelManager.WebServer
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
            FileLogger.WriteLog("Web Host server started", StatusMessageType.Info);
        }

        private void OnStopping()
        {
            FileLogger.WriteLog("Web Host server stopping", StatusMessageType.Info);
        }

        private void OnStopped()
        {
            FileLogger.WriteLog("Web Host server stopped", StatusMessageType.Info);
        }
    }
}
