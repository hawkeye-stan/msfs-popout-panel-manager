using Microsoft.Extensions.Hosting;
using MSFSPopoutPanelManager.Shared;
using System.Threading;
using System.Threading.Tasks;

namespace MSFSPopoutPanelManager.WebServer
{
    internal class WebApiHostService : IHostedService
    {
        private readonly IHostApplicationLifetime _appLifetime;
        private readonly ISimConnectService _simConnectService;

        public WebApiHostService(IHostApplicationLifetime appLifetime, ISimConnectService simConnectService)
        {
            _appLifetime = appLifetime;
            _simConnectService = simConnectService;
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
            FileLogger.WriteLog("API Host server started", StatusMessageType.Info);
            _simConnectService.Start();
        }

        private void OnStopping()
        {
            FileLogger.WriteLog("API Host server stopping", StatusMessageType.Info);
            _simConnectService.Stop();
        }

        private void OnStopped()
        {
            FileLogger.WriteLog("API Host server stopped", StatusMessageType.Info);
        }
    }
}
