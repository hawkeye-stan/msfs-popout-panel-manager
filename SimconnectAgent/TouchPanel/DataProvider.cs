using MSFSPopoutPanelManager.Shared;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Timers;

namespace MSFSPopoutPanelManager.SimConnectAgent.TouchPanel
{
    public class DataProvider
    {
        private const int MSFS_DATA_REFRESH_TIMEOUT = 50;

        private TouchPanelSimConnector _simConnector;
        private Timer _requestDataTimer;
        private bool _isHandlingCriticalError;

        public event EventHandler<string> OnDataRefreshed;
        public event EventHandler<string> OnCriticalErrorReceived;

        public DataProvider(TouchPanelSimConnector simConnector)
        {
            _simConnector = simConnector;
        }

        public void Start()
        {
            _requestDataTimer = new Timer();
            _requestDataTimer.Interval = MSFS_DATA_REFRESH_TIMEOUT;
            _requestDataTimer.Enabled = true;
            _requestDataTimer.Elapsed += HandleDataRequested;
            _requestDataTimer.Elapsed += HandleMessageReceived;

            _simConnector.OnReceivedData += HandleDataReceived;
        }

        public void Stop()
        {
            if (_requestDataTimer != null)
            {
                _requestDataTimer.Enabled = false;
                _simConnector.OnReceivedData -= HandleDataReceived;
                OnDataRefreshed?.Invoke(this, null);
            }
        }

        private void HandleDataRequested(object sender, ElapsedEventArgs e)
        {
            try
            {
                _simConnector.RequestData();
            }
            catch (Exception ex)
            {
                if (!_isHandlingCriticalError)
                {
                    _isHandlingCriticalError = true;
                    OnCriticalErrorReceived?.Invoke(this, $"TouchPanel DataProvider: SimConnect HandleDataRequested Error - {ex.Message}");
                }
            }
        }

        private void HandleMessageReceived(object sender, ElapsedEventArgs e)
        {
            try
            {
                _simConnector.ReceiveMessage();
            }
            catch (Exception ex)
            {
                FileLogger.WriteLog($"TouchPanel DataProvider: SimConnect HandleMessageReceived Error - {ex.Message}", StatusMessageType.Error);
            }
        }

        public void HandleDataReceived(object sender, List<SimConnectDataDefinition> e)
        {
            var jsonData = JsonConvert.SerializeObject(e, Formatting.Indented, new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() });

            // Invoke on data refresh event by listener
            OnDataRefreshed?.Invoke(this, jsonData);
        }
    }
}


