using Microsoft.Extensions.Caching.Memory;
using MSFSPopoutPanelManager.TouchPanel.Shared;
using MSFSPopoutPanelManager.TouchPanel.SimConnectAgent;
using System;

namespace MSFSPopoutPanelManager.TouchPanel.TouchPanelHost
{
    public class SimConnectService : ISimConnectService
    {
        private IMemoryCache _memCache;
        private SimConnectProvider _simConnectorProvider;

        public SimConnectService(IntPtr windowHandle)
        {
            _simConnectorProvider = new SimConnectProvider(windowHandle);

            _simConnectorProvider.OnMsfsConnected += (source, e) =>
            {
                try { _memCache.Set("msfsStatus", true); } catch { }
            };

            _simConnectorProvider.OnMsfsDisconnected += (source, e) =>
            {
                try { _memCache.Set("msfsStatus", false); } catch { }
            };

            _simConnectorProvider.OnMsfsException += (source, e) =>
            {
                try { _memCache.Set("msfsStatus", false); } catch { }
            };

            _simConnectorProvider.OnDataRefreshed += (source, e) =>
            {
                try { _memCache.Set("simdata", e.Value); } catch { }
            };

            _simConnectorProvider.OnReceiveSystemEvent += (source, e) =>
            {
                try
                {
                    var value = $"{e.Value}-{DateTime.Now.Ticks}";
                    _memCache.Set("simSystemEvent", value);

                    // Clear G1000NXi cache
                    if (e.Value == "SIMSTART" || e.Value == "SIMSTOP")
                    {
                        _memCache.Set("g1000nxiFlightPlan", string.Empty);
                    }
                }
                catch { }
            };

            _simConnectorProvider.OnArduinoConnectionChanged += (source, e) =>
            {
                try { _memCache.Set("arduinoStatus", e.Value); } catch { }
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

        public string GetFlightPlan()
        {
            return _simConnectorProvider.GetFlightPlan();
        }

        public void ResetSimConnectDataArea(string planeId)
        {
            _simConnectorProvider.ResetSimConnectDataArea(planeId);
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
