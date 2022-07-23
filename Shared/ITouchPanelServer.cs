using System;
using System.Threading;
using System.Threading.Tasks;

namespace MSFSPopoutPanelManager.Shared
{
    public interface ITouchPanelServer
    {
        public IntPtr WindowHandle { set; }

        public bool ServerStarted { get; set; }

        public int DataRefreshInterval { set; }

        public int MapRefreshInterval { set; }

        public bool IsUsedArduino { set; }

        public bool IsEnabledSound { set; }

        public Task StartAsync(CancellationToken cancellationToken);

        public Task StopAsync(CancellationToken cancellationToken);

        public Task RestartAsync(CancellationToken cancellationToken);
    }
}
