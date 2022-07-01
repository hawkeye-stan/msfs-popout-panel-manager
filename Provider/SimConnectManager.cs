using MSFSPopoutPanelManager.FsConnector;
using MSFSPopoutPanelManager.Shared;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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

            _simConnector.Start();
        }

        public void Stop()
        {
            _simConnector.Stop();
        }

        public void Restart()
        {
            _simConnector.StopAndReconnect();
        }

        public void TurnOnPower(bool isRequiredForColdStart)
        {
            // Wait for _simData.AtcOnParkingSpot to refresh
            Thread.Sleep(MSFS_DATA_REFRESH_TIMEOUT + 250);

            if (isRequiredForColdStart && _simData != null && (_simData.AtcOnParkingSpot || !_simData.ElectricalMasterBattery))
            {
                _isPowerOnForPopOut = true;
                _simConnector.TransmitActionEvent(ActionEvent.KEY_MASTER_BATTERY_SET, 1);
                Thread.Sleep(100);
                _simConnector.TransmitActionEvent(ActionEvent.KEY_TOGGLE_AVIONICS_MASTER, 1);
                Thread.Sleep(100);
            }
        }

        public void TurnOffpower()
        {
            if (_isPowerOnForPopOut)
            {
                _simConnector.TransmitActionEvent(ActionEvent.KEY_TOGGLE_AVIONICS_MASTER, 1);
                Thread.Sleep(100);
                _simConnector.TransmitActionEvent(ActionEvent.KEY_MASTER_BATTERY_SET, 0);
                Thread.Sleep(100);

                _isPowerOnForPopOut = false;
            }
        }

        public void TurnOffTrackIR()
        {
            if (_simData != null && _simData.TrackIREnable)
            {
                SetTrackIREnable(false);
                _isTrackIRManaged = true;
            }
        }

        public void TurnOnTrackIR()
        {
            if (_isTrackIRManaged && _simData != null && !_simData.TrackIREnable)
            {
                SetTrackIREnable(true);
                _isTrackIRManaged = false;
            }
        }

        private void SetTrackIREnable(bool enable)
        {
            // Wait for _simData.ElectricalMasterBattery to refresh
            Thread.Sleep(MSFS_DATA_REFRESH_TIMEOUT + 250);

            // It is prop3 in SimConnectStruct (by DataDefinitions.cs)
            SimConnectStruct simConnectStruct = new SimConnectStruct();

            simConnectStruct.Prop01 = _simData.Title;                                                                   // must set "Title" for TrackIR variable to write correctly
            simConnectStruct.Prop02 = _simData.ElectricalMasterBattery ? Convert.ToDouble(1) : Convert.ToDouble(0);     // must set "ElectricalMasterBattery" for TrackIR variable to write correctly
            simConnectStruct.Prop03 = enable ? Convert.ToDouble(1) : Convert.ToDouble(0);                               // this is the TrackIR variable
            simConnectStruct.Prop04 = _simData.AtcOnParkingSpot ? Convert.ToDouble(1) : Convert.ToDouble(0);            // must set "AtcOnParkingSpot" for TrackIR variable to write correctly
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

        private List<SimConnectSystemEvent> _systemEventBuffer;
        private List<SimConnectSystemEvent> FlightRestartBeginBufferDef = new List<SimConnectSystemEvent>() { SimConnectSystemEvent.SIMSTOP, SimConnectSystemEvent.VIEW };
        private List<SimConnectSystemEvent> FlightRestartEndBufferDef = new List<SimConnectSystemEvent>() { SimConnectSystemEvent.SIMSTART, SimConnectSystemEvent.VIEW };
        private List<SimConnectSystemEvent> FlightStartBufferDef = new List<SimConnectSystemEvent>() { SimConnectSystemEvent.SIMSTOP, SimConnectSystemEvent.SIMSTART, SimConnectSystemEvent.SIMSTOP, SimConnectSystemEvent.SIMSTART, SimConnectSystemEvent.VIEW };
        private List<SimConnectSystemEvent> FlightEndBufferDef = new List<SimConnectSystemEvent>() { SimConnectSystemEvent.SIMSTOP, SimConnectSystemEvent.SIMSTART, SimConnectSystemEvent.VIEW, SimConnectSystemEvent.SIMSTOP, SimConnectSystemEvent.SIMSTART };
        private bool _systemEventInFlightRestartSequence;

        private void HandleReceiveSystemEvent(object sender, EventArgs<SimConnectSystemEvent> e)
        {
            if (_systemEventBuffer == null)
                _systemEventBuffer = new List<SimConnectSystemEvent>();

            var systemEvent = e.Value;

            _systemEventBuffer.Add(systemEvent);

            Debug.WriteLine($"SimConnectSystemEvent Received: {systemEvent}");

            if (_systemEventBuffer.TakeLast(2).SequenceEqual(FlightRestartBeginBufferDef))
            {
                OnFlightStopped?.Invoke(this, null);
                _systemEventBuffer = null;
                _systemEventInFlightRestartSequence = true;
            }
            else if (_systemEventInFlightRestartSequence && _systemEventBuffer.TakeLast(2).SequenceEqual(FlightRestartEndBufferDef))
            {
                OnFlightStarted?.Invoke(this, null);
                _systemEventBuffer = null;
                _systemEventInFlightRestartSequence = false;
            }
            else if (_systemEventBuffer.TakeLast(5).SequenceEqual(FlightStartBufferDef))
            {
                OnFlightStarted?.Invoke(this, null);
                _systemEventBuffer = null;
            }
            else if (_systemEventBuffer.TakeLast(5).SequenceEqual(FlightEndBufferDef))
            {
                OnFlightStopped?.Invoke(this, null);
                _systemEventBuffer = null;
            }
        }
    }
}
