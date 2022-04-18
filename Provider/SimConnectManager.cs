using MSFSPopoutPanelManager.FsConnector;
using MSFSPopoutPanelManager.Shared;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;
using System.Timers;

namespace MSFSPopoutPanelManager.Provider
{
    public class SimConnectManager 
    { 
        private const int MSFS_DATA_REFRESH_TIMEOUT = 1000;

        private SimConnector _simConnector;
        private dynamic _simData;

        private System.Timers.Timer _requestDataTimer;
        private SimConnectSystemEvent _lastSystemEvent;
        private bool _isSimActive;
        private bool _isPowerOnForPopOut;

        public event EventHandler OnConnected;
        public event EventHandler OnDisconnected;
        public event EventHandler<EventArgs<dynamic>> OnSimConnectDataRefreshed;
        public event EventHandler OnFlightStarted;
        public event EventHandler OnFlightStopped;

        public bool IsSimConnectStarted { get; set; }

        public SimConnectManager()
        {
            _simConnector = new SimConnector();
            _simConnector.OnConnected += (sender, e) => { OnConnected?.Invoke(this, null); };
            _simConnector.OnDisconnected += (sender, e) => { OnDisconnected?.Invoke(this, null); };
            _simConnector.OnReceivedData += HandleDataReceived;
            _simConnector.OnReceiveSystemEvent += HandleReceiveSystemEvent;
            _simConnector.OnConnected += (sender, e) =>
            {
                _requestDataTimer = new System.Timers.Timer();
                _requestDataTimer.Interval = MSFS_DATA_REFRESH_TIMEOUT;
                _requestDataTimer.Enabled = true;
                _requestDataTimer.Elapsed += HandleDataRequested;
                _requestDataTimer.Elapsed += HandleMessageReceived;
            };

            _isSimActive = false;
            _simConnector.Start();
        }

        public void Stop()
        {
            _isSimActive = false;
            _simConnector.Stop();
        }

        public void Restart()
        {
            _isSimActive = false;
            _simConnector.StopAndReconnect();
        }

        public void TurnOnPower(bool isRequiredForColdStart)
        {
            if (isRequiredForColdStart && _simData != null && !_simData.ElectricalMasterBattery)
            {
                _isPowerOnForPopOut = true;
                _simConnector.TransmitActionEvent(ActionEvent.KEY_MASTER_BATTERY_SET, 1);
                Thread.Sleep(100);
                _simConnector.TransmitActionEvent(ActionEvent.KEY_ALTERNATOR_SET, 1);
                Thread.Sleep(100);
                _simConnector.TransmitActionEvent(ActionEvent.KEY_AVIONICS_MASTER_1_ON, 1);
                Thread.Sleep(100);
                _simConnector.TransmitActionEvent(ActionEvent.KEY_AVIONICS_MASTER_2_ON, 1);
                Thread.Sleep(100);
                _simConnector.TransmitActionEvent(ActionEvent.KEY_AVIONICS_MASTER_SET, 1);
            }
        }

        public void TurnOffpower()
        {
            if(_isPowerOnForPopOut)
            {
                _simConnector.TransmitActionEvent(ActionEvent.KEY_AVIONICS_MASTER_1_OFF, 1);
                Thread.Sleep(100);
                _simConnector.TransmitActionEvent(ActionEvent.KEY_AVIONICS_MASTER_2_OFF, 1);
                Thread.Sleep(100);
                _simConnector.TransmitActionEvent(ActionEvent.KEY_AVIONICS_MASTER_SET, 0);
                Thread.Sleep(100);
                _simConnector.TransmitActionEvent(ActionEvent.KEY_ALTERNATOR_SET, 0);
                Thread.Sleep(100);
                _simConnector.TransmitActionEvent(ActionEvent.KEY_MASTER_BATTERY_SET, 0);

                _isPowerOnForPopOut = false;
            }
        }

        private void HandleDataRequested(object sender, ElapsedEventArgs e)
        {
            try
            {
                _simConnector.RequestData();
            }
            catch { }
        }

        private void HandleMessageReceived(object sender, ElapsedEventArgs e)
        {
            try
            {
                _simConnector.ReceiveMessage();
            }
            catch { }
        }

        public void HandleDataReceived(object sender, EventArgs<dynamic> e)
        {
            _simData = e.Value;
            OnSimConnectDataRefreshed?.Invoke(this, new EventArgs<dynamic>(e.Value));
        }

        private void HandleReceiveSystemEvent(object sender, EventArgs<Tuple<SimConnectSystemEvent, uint>> e)
        {
            var systemEvent = e.Value.Item1;
            var dwData = e.Value.Item2;

            Debug.WriteLine($"SimConnectSystemEvent Received: {systemEvent}  dwData: {dwData}");

            // to detect flight start at the "Ready to Fly" screen, it has a SIMSTART follows by a VIEW event
            if (_lastSystemEvent == SimConnectSystemEvent.SIMSTART && systemEvent == SimConnectSystemEvent.VIEW && dwData == 4)
            {
                _isSimActive = true;
                OnFlightStarted?.Invoke(this, null);
                return;
            }
            
            // look for pair of events denoting sim ended after sim is active
            if ((_isSimActive && _lastSystemEvent == SimConnectSystemEvent.SIMSTOP && systemEvent == SimConnectSystemEvent.VIEW && dwData == 4) ||
                (_isSimActive && _lastSystemEvent == SimConnectSystemEvent.SIMSTOP && systemEvent == SimConnectSystemEvent.SIMSTART && dwData == 1))
            {
                _isSimActive = false;
                _lastSystemEvent = SimConnectSystemEvent.NONE;
                OnFlightStopped?.Invoke(this, null);
                return;
            }

            _lastSystemEvent = systemEvent;
        }
    }
}
