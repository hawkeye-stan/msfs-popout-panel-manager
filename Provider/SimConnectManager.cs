using MSFSPopoutPanelManager.FsConnector;
using MSFSPopoutPanelManager.Shared;
using System;
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
        private bool _isTrackIRManaged;

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
                _simConnector.TransmitActionEvent(ActionEvent.KEY_AVIONICS_MASTER_SET, 1);
                Thread.Sleep(100);
                _simConnector.TransmitActionEvent(ActionEvent.KEY_AVIONICS_MASTER_2_SET, 1);
            }
        }

        public void TurnOffpower()
        {
            if(_isPowerOnForPopOut)
            {
                _simConnector.TransmitActionEvent(ActionEvent.KEY_AVIONICS_MASTER_2_SET, 0);
                Thread.Sleep(100);
                _simConnector.TransmitActionEvent(ActionEvent.KEY_AVIONICS_MASTER_SET, 0);
                Thread.Sleep(100);
                _simConnector.TransmitActionEvent(ActionEvent.KEY_ALTERNATOR_SET, 0);
                Thread.Sleep(100);
                _simConnector.TransmitActionEvent(ActionEvent.KEY_MASTER_BATTERY_SET, 0);

                _isPowerOnForPopOut = false;
            }
        }

        public void TurnOffTrackIR()
        {
            if(_simData.TrackIREnable)
            {
                SetTrackIREnable(false);
                _isTrackIRManaged = true;
            }
        }

        public void TurnOnTrackIR()
        {
            if (_isTrackIRManaged && !_simData.TrackIREnable)
            {
                SetTrackIREnable(true);
                _isTrackIRManaged = false;
            }
        }

        private void SetTrackIREnable(bool enable)
        {
            // It is prop3 in SimConnectStruct (by DataDefinitions.cs)
            SimConnectStruct simConnectStruct = new SimConnectStruct();
            simConnectStruct.Prop03 = enable ? Convert.ToDouble(1): Convert.ToDouble(0);
            _simConnector.SetDataObject(simConnectStruct);
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

        private void HandleReceiveSystemEvent(object sender, EventArgs<SimConnectSystemEvent> e)
        {
            var systemEvent = e.Value;

            Debug.WriteLine($"SimConnectSystemEvent Received: {systemEvent}");

            // to detect flight start at the "Ready to Fly" screen, it has a SIMSTART follows by a VIEW event
            if (_lastSystemEvent == SimConnectSystemEvent.SIMSTART && systemEvent == SimConnectSystemEvent.VIEW)
            {
                _isSimActive = true;
                _lastSystemEvent = SimConnectSystemEvent.NONE;
                OnFlightStarted?.Invoke(this, null);
                return;
            }
            
            // look for pair of events denoting sim ended after sim is active
            if ((_isSimActive && _lastSystemEvent == SimConnectSystemEvent.SIMSTOP && systemEvent == SimConnectSystemEvent.VIEW) ||
                (_isSimActive && _lastSystemEvent == SimConnectSystemEvent.SIMSTOP && systemEvent == SimConnectSystemEvent.SIMSTART))
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
