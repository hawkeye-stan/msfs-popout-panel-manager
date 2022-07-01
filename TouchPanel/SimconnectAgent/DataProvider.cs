using MSFSPopoutPanelManager.TouchPanel.FSConnector;
using MSFSPopoutPanelManager.TouchPanel.Shared;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Timers;

namespace MSFSPopoutPanelManager.TouchPanel.SimConnectAgent
{
    public class DataProvider
    {
        private const int MSFS_DATA_REFRESH_TIMEOUT = 50;

        private SimConnector _simConnector;
        private Timer _requestDataTimer;
        private bool _isHandlingCriticalError;

        public event EventHandler<EventArgs<string>> OnDataRefreshed;
        public event EventHandler<EventArgs<string>> OnCriticalErrorReceived;

        public DataProvider(SimConnector simConnector)
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
                OnDataRefreshed?.Invoke(this, new EventArgs<string>(null));
            }
        }

        public string GetFlightPlan()
        {
            // MSFS 2020 Windows Store version: C:\Users\{username}\AppData\Local\Packages\Microsoft.FlightSimulator_8wekyb3d8bbwe\LocalState\MISSIONS\Custom\CustomFlight\CustomFlight.FLT
            // MSFS 2020 Steam version: C:\Users\{username}\AppData\Roaming\Microsoft Flight Simulator\MISSIONS\Custom\CustomFlight\CustomFlight.FLT
            var filePathMSStore = Environment.ExpandEnvironmentVariables("%LocalAppData%") + @"\Packages\Microsoft.FlightSimulator_8wekyb3d8bbwe\LocalState\MISSIONS\Custom\CustomFlight\CustomFlight.FLT";
            var filePathSteam = Environment.ExpandEnvironmentVariables("%AppData%") + @"\Microsoft Flight Simulator\MISSIONS\Custom\CustomFlight\CustomFlight.FLT";

            string filePath;

            if (File.Exists(filePathMSStore))
                filePath = filePathMSStore;
            else if (File.Exists(filePathSteam))
                filePath = filePathSteam;
            else
                filePath = null;

            // cannot find CustomFlight.PLN, return empty set of waypoints
            if (filePath == null)
                return JsonConvert.SerializeObject(new List<ExpandoObject>());

            return FlightPlanProvider.ParseCustomFLT(filePath);
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
                    TouchPanelLogger.ServerLog($"SimConnect HandleDataRequested Error {ex.Message}", LogLevel.ERROR);
                    OnCriticalErrorReceived?.Invoke(this, new EventArgs<string>(ex.Message));
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
                TouchPanelLogger.ServerLog($"SimConnect HandleMessageReceived Error {ex.Message}", LogLevel.ERROR);
            }
        }

        public void HandleDataReceived(object sender, EventArgs<List<SimConnectDataDefinition>> e)
        {
            var jsonData = JsonConvert.SerializeObject(e.Value, Formatting.Indented, new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() });

            // Invoke on data refresh event by listener
            OnDataRefreshed?.Invoke(this, new EventArgs<string>(jsonData));
        }
    }
}


