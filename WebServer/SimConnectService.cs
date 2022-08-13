using Microsoft.Extensions.Caching.Memory;
using MSFSPopoutPanelManager.SimConnectAgent.TouchPanel;
using System;

namespace MSFSPopoutPanelManager.WebServer
{
    public class SimConnectService : ISimConnectService
    {
        private IMemoryCache _memCache;
        private TouchPanelSimConnectProvider _simConnectorProvider;

        public SimConnectService(IntPtr windowHandle)
        {
            _simConnectorProvider = new TouchPanelSimConnectProvider(windowHandle);

            _simConnectorProvider.OnConnected += (source, e) =>
            {
                try { _memCache.Set("msfsStatus", true); } catch { }
            };

            _simConnectorProvider.OnDisconnected += (source, e) =>
            {
                try { _memCache.Set("msfsStatus", false); } catch { }
            };

            _simConnectorProvider.OnDataRefreshed += (source, e) =>
            {
                try { _memCache.Set("simdata", e); } catch { }
            };

            _simConnectorProvider.OnReceiveSystemEvent += (source, e) =>
            {
                try
                {
                    var value = $"{e}-{DateTime.Now.Ticks}";
                    _memCache.Set("simSystemEvent", value.ToString());
                }
                catch { }
            };

            _simConnectorProvider.OnArduinoConnectionChanged += (source, e) =>
            {
                try { _memCache.Set("arduinoStatus", e); } catch { }
            };
        }

        public TouchPanelConfigSetting TouchPanelConfigSetting { get; set; }

        public void SetMemoryCache(IMemoryCache memCache)
        {
            _memCache = memCache;
        }

        public void Start()
        {
            _simConnectorProvider.Start(TouchPanelConfigSetting.IsUsedArduino);
        }

        public void Stop()
        {
            if (_simConnectorProvider != null)
            {
                _simConnectorProvider.Stop();
            }
        }

        public void ExecAction(SimConnectActionData actionData)
        {
            _simConnectorProvider.ExecAction(actionData);
        }

        public void ResetSimConnectDataArea(string planeId)
        {
            _simConnectorProvider.ResetSimConnectDataArea(planeId);
        }

        public string GetFlightPlan()
        {
            return _simConnectorProvider.GetFlightPlan();
        }
    }

    public interface ISimConnectService
    {
        public void SetMemoryCache(IMemoryCache memCache);

        public void Start();

        public void Stop();

        public void ExecAction(SimConnectActionData data);

        public void ResetSimConnectDataArea(string planeId);

        public string GetFlightPlan();

        public TouchPanelConfigSetting TouchPanelConfigSetting { get; set; }
    }
}
