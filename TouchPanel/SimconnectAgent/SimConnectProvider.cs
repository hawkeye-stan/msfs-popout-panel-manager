using MSFSPopoutPanelManager.TouchPanel.ArduinoAgent;
using MSFSPopoutPanelManager.TouchPanel.FSConnector;
using MSFSPopoutPanelManager.TouchPanel.Shared;
using System;
using System.Threading;

namespace MSFSPopoutPanelManager.TouchPanel.SimConnectAgent
{
    public class SimConnectProvider
    {
        private ArduinoProvider _arduinoProvider;
        private SimConnector _simConnector;

        private DataProvider _dataProvider;
        private ActionProvider _actionProvider;

        private bool _isHandlingCriticalError;
        private bool _isUsedArduino;

        public event EventHandler OnMsfsConnected;
        public event EventHandler OnMsfsDisconnected;
        public event EventHandler OnMsfsException;
        public event EventHandler<EventArgs<string>> OnDataRefreshed;
        public event EventHandler<EventArgs<string>> OnReceiveSystemEvent;
        public event EventHandler<EventArgs<bool>> OnArduinoConnectionChanged;

        public SimConnectProvider(IntPtr windowHandle)
        {
            _simConnector = new SimConnector();
            _simConnector.OnConnected += HandleSimConnected;
            _simConnector.OnDisconnected += HandleSimDisonnected;
            _simConnector.OnException += HandleSimException;
            _simConnector.OnReceiveSystemEvent += (source, e) => OnReceiveSystemEvent?.Invoke(this, new EventArgs<string>(e.Value));

            _isHandlingCriticalError = false;
        }

        public void Start(bool isUsedArduino)
        {
            _isUsedArduino = isUsedArduino;

            _simConnector.Start();
            InitializeProviders();
        }

        public void Stop()
        {
            if (_arduinoProvider != null)
                _arduinoProvider.Stop();

            _dataProvider.Stop();
            _actionProvider.Stop();
            _simConnector.Stop();
            OnMsfsDisconnected?.Invoke(this, null);
        }

        public void StopAndReconnect()
        {
            Stop();
            Thread.Sleep(1000);     // wait for everything to stop

            _simConnector.Start();
            InitializeProviders();
        }

        public void ExecAction(SimConnectActionData actionData)
        {
            _actionProvider.ExecAction(actionData);
        }

        public void ResetSimConnectDataArea(string planeId)
        {
            _simConnector.ResetSimConnectDataArea(planeId);
        }

        public string GetFlightPlan()
        {
            return _dataProvider.GetFlightPlan();
        }

        private void InitializeProviders()
        {
            _dataProvider = new DataProvider(_simConnector);
            _dataProvider.OnDataRefreshed += (source, e) => OnDataRefreshed?.Invoke(this, e);
            _dataProvider.OnCriticalErrorReceived += HandleSimException;

            if (_isUsedArduino)
            {
                _arduinoProvider = new ArduinoProvider();
                _arduinoProvider.OnConnectionChanged += (source, e) => OnArduinoConnectionChanged?.Invoke(this, new EventArgs<bool>(e.Value));

                _actionProvider = new ActionProvider(_simConnector);
                _arduinoProvider.OnDataReceived += _actionProvider.ArduinoInputHandler;
            }
            else
            {
                _actionProvider = new ActionProvider(_simConnector);
                _arduinoProvider = null;
            }
        }

        private void HandleSimConnected(object source, EventArgs e)
        {
            if (_arduinoProvider != null)
                _arduinoProvider.Start();

            _dataProvider.Start();
            _actionProvider.Start();

            OnMsfsConnected?.Invoke(this, null);

            TouchPanelLogger.ServerLog("MSFS connected", LogLevel.INFO);
        }

        private void HandleSimDisonnected(object source, EventArgs e)
        {
            StopAndReconnect();
            TouchPanelLogger.ServerLog("MSFS disconnected", LogLevel.INFO);
        }

        private void HandleSimException(object source, EventArgs<string> e)
        {
            if (!_isHandlingCriticalError)
            {
                _isHandlingCriticalError = true;     // Prevent restarting to occur in parallel

                TouchPanelLogger.ServerLog($"Restarting MSFS connection...", LogLevel.INFO);

                StopAndReconnect();
                _isHandlingCriticalError = false;
            }
        }
    }
}
