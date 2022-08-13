using MSFSPopoutPanelManager.ArduinoAgent;
using MSFSPopoutPanelManager.Shared;
using System;
using System.Threading;

namespace MSFSPopoutPanelManager.SimConnectAgent.TouchPanel
{
    public class TouchPanelSimConnectProvider
    {
        private ArduinoProvider _arduinoProvider;
        private TouchPanelSimConnector _simConnector;

        private DataProvider _dataProvider;
        private ActionProvider _actionProvider;

        private bool _isHandlingCriticalError;
        private bool _isUsedArduino;

        public event EventHandler OnConnected;
        public event EventHandler OnDisconnected;
        public event EventHandler<string> OnDataRefreshed;
        public event EventHandler<SimConnectSystemEvent> OnReceiveSystemEvent;
        public event EventHandler<bool> OnArduinoConnectionChanged;

        public TouchPanelSimConnectProvider(IntPtr windowHandle)
        {
            _simConnector = new TouchPanelSimConnector();
            _simConnector.OnConnected += HandleSimConnected;
            _simConnector.OnCriticalException += HandleSimException;
            _simConnector.OnReceiveSystemEvent += (source, e) => OnReceiveSystemEvent?.Invoke(this, e);

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
            OnDisconnected?.Invoke(this, null);
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
                _arduinoProvider.OnConnectionChanged += (source, e) => OnArduinoConnectionChanged?.Invoke(this, e);

                _actionProvider = new ActionProvider(_simConnector, true);
                _arduinoProvider.OnDataReceived += _actionProvider.ArduinoInputHandler;
            }
            else
            {
                _actionProvider = new ActionProvider(_simConnector, false);
                _arduinoProvider = null;
            }
        }

        private void HandleSimConnected(object source, EventArgs e)
        {
            if (_arduinoProvider != null)
                _arduinoProvider.Start();

            _dataProvider.Start();
            _actionProvider.Start();

            OnConnected?.Invoke(this, null);

            FileLogger.WriteLog("TouchPanel: MSFS connected", StatusMessageType.Info);
        }

        private void HandleSimException(object source, string e)
        {
            if (!_isHandlingCriticalError)
            {
                _isHandlingCriticalError = true;     // Prevent restarting to occur in parallel
                FileLogger.WriteLog(e, StatusMessageType.Error);
                StopAndReconnect();
                _isHandlingCriticalError = false;
            }
        }
    }
}
