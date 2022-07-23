using MSFSPopoutPanelManager.Shared;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Timers;

namespace MSFSPopoutPanelManager.SimConnectAgent
{
    public class SimConnectProvider
    {
        private const int MSFS_DATA_REFRESH_TIMEOUT = 500;

        private SimConnector _simConnector;

        private bool _isHandlingCriticalError;
        private List<SimConnectDataDefinition> _simData;

        private System.Timers.Timer _requestDataTimer;
        private bool _isPowerOnForPopOut;
        private bool _isAvionicsOnForPopOut;
        private bool _isTrackIRManaged;

        public event EventHandler OnConnected;
        public event EventHandler OnDisconnected;
        public event EventHandler OnFlightStarted;
        public event EventHandler OnFlightStopped;
        public event EventHandler<List<SimConnectDataDefinition>> OnSimConnectDataRefreshed;

        public SimConnectProvider()
        {
            _simConnector = new SimConnector();
            _simConnector.OnConnected += HandleSimConnected;
            _simConnector.OnDisconnected += HandleSimDisonnected;
            _simConnector.OnException += HandleSimException;
            _simConnector.OnReceiveSystemEvent += HandleReceiveSystemEvent;
            _simConnector.OnReceivedData += HandleDataReceived;

            _isHandlingCriticalError = false;
        }

        public void Start()
        {
            _simConnector.Start();
        }

        public void Stop(bool appExit)
        {
            _simConnector.Stop();

            if (!appExit)
                OnDisconnected?.Invoke(this, null);
        }

        public void StopAndReconnect()
        {
            _simConnector.Stop();
            Thread.Sleep(2000);     // wait for everything to stop
            _simConnector.Restart();
        }

        public void TurnOnPower(bool isRequiredForColdStart)
        {
            if (!isRequiredForColdStart || _simData == null)
                return;

            // Wait for _simData.AtcOnParkingSpot to refresh
            Thread.Sleep(MSFS_DATA_REFRESH_TIMEOUT + 500);

            var planeOnParkingSpot = Convert.ToBoolean(_simData.Find(d => d.PropName == "PlaneInParkingSpot").Value);

            if (isRequiredForColdStart && planeOnParkingSpot)
            {
                Debug.Write("Battery Power ON............" + Environment.NewLine);

                _isPowerOnForPopOut = true;
                _simConnector.TransmitActionEvent(ActionEvent.KEY_MASTER_BATTERY_SET, 1);
                Thread.Sleep(200);
            }
        }

        public void TurnOffpower(bool isRequiredForColdStart)
        {
            if (!isRequiredForColdStart || _simData == null)
                return;

            if (_isPowerOnForPopOut)
            {
                Debug.Write("Battery Power OFF............" + Environment.NewLine);

                _simConnector.TransmitActionEvent(ActionEvent.KEY_TOGGLE_AVIONICS_MASTER, 1);
                Thread.Sleep(200);
                _simConnector.TransmitActionEvent(ActionEvent.KEY_MASTER_BATTERY_SET, 0);
                Thread.Sleep(200);

                _isPowerOnForPopOut = false;
            }
        }

        public void TurnOnAvionics(bool isRequiredForColdStart)
        {
            if (!isRequiredForColdStart || _simData == null)
                return;

            var planeOnParkingSpot = Convert.ToBoolean(_simData.Find(d => d.PropName == "PlaneInParkingSpot").Value);

            Debug.Write("Avionics ON............" + Environment.NewLine);

            if (isRequiredForColdStart && planeOnParkingSpot)
            {
                _isAvionicsOnForPopOut = true;
                _simConnector.TransmitActionEvent(ActionEvent.KEY_TOGGLE_AVIONICS_MASTER, 1);
                Thread.Sleep(200);
            }
        }

        public void TurnOffAvionics(bool isRequiredForColdStart)
        {
            if (!isRequiredForColdStart || _simData == null)
                return;

            if (_isAvionicsOnForPopOut)
            {
                Debug.Write("Avionics OFF............" + Environment.NewLine);

                _simConnector.TransmitActionEvent(ActionEvent.KEY_MASTER_BATTERY_SET, 0);
                Thread.Sleep(200);

                _isAvionicsOnForPopOut = false;
            }
        }

        public void TurnOffTrackIR()
        {
            if (_simData != null)
            {
                var trackIREnable = Convert.ToBoolean(_simData.Find(d => d.PropName == "TrackIREnable").Value);

                if (trackIREnable)
                {
                    Debug.Write("TrackIR OFF............" + Environment.NewLine);

                    SetTrackIREnable(false);
                    _isTrackIRManaged = true;

                    Thread.Sleep(500);      // wait for track IR camera movement to stop
                }
            }

            Thread.Sleep(200);
        }

        public void TurnOnTrackIR()
        {
            if (_simData != null)
            {
                var trackIREnable = Convert.ToBoolean(_simData.Find(d => d.PropName == "TrackIREnable").Value);

                if (_isTrackIRManaged && !trackIREnable)
                {
                    Debug.Write("TrackIR ON............" + Environment.NewLine);
                    SetTrackIREnable(true);
                    _isTrackIRManaged = false;
                }
            }

            Thread.Sleep(200);
        }

        private void SetTrackIREnable(bool enable)
        {
            var defineId = _simData.Find(d => d.PropName == "TrackIREnable").DefineId;
            _simConnector.SetDataObject(defineId, enable ? Convert.ToDouble(1) : Convert.ToDouble(0));
        }


        private void HandleSimConnected(object source, EventArgs e)
        {
            OnConnected?.Invoke(this, null);

            // Start data request timer
            _requestDataTimer = new System.Timers.Timer();
            _requestDataTimer.Interval = MSFS_DATA_REFRESH_TIMEOUT;
            _requestDataTimer.Enabled = true;
            _requestDataTimer.Elapsed += HandleDataRequested;
            _requestDataTimer.Elapsed += HandleMessageReceived;
        }

        private void HandleSimDisonnected(object source, EventArgs e)
        {
            FileLogger.WriteLog($"MSFS is closed.", StatusMessageType.Info);
            OnDisconnected?.Invoke(this, null);
            StopAndReconnect();
        }

        private void HandleSimException(object source, string e)
        {
            if (!_isHandlingCriticalError)
            {
                _isHandlingCriticalError = true;     // Prevent restarting to occur in parallel
                StopAndReconnect();
                _isHandlingCriticalError = false;
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

        public void HandleDataReceived(object sender, List<SimConnectDataDefinition> e)
        {
            _simData = e;
            OnSimConnectDataRefreshed?.Invoke(this, e);

            DetectFlightStartedOrStopped();
        }

        private const int CAMERA_STATE_COCKPIT = 2;
        private const int CAMERA_STATE_LOAD_SCREEN = 11;
        private const int CAMERA_STATE_HOME_SCREEN = 15;
        private int _currentCameraState = -1;

        private void DetectFlightStartedOrStopped()
        {
            // Determine is flight started or ended
            var cameraState = Convert.ToInt32(_simData.Find(d => d.PropName == "CameraState").Value);

            if (_currentCameraState == cameraState)
                return;

            switch (_currentCameraState)
            {
                case CAMERA_STATE_HOME_SCREEN:
                case CAMERA_STATE_LOAD_SCREEN:
                    if (cameraState == CAMERA_STATE_COCKPIT)
                    {
                        _currentCameraState = cameraState;
                        OnFlightStarted?.Invoke(this, null);
                    }

                    break;
                case CAMERA_STATE_COCKPIT:
                    if ((cameraState == CAMERA_STATE_LOAD_SCREEN || cameraState == CAMERA_STATE_HOME_SCREEN))
                    {
                        _currentCameraState = cameraState;
                        OnFlightStopped?.Invoke(this, null);
                    }

                    break;
            }

            if (cameraState == CAMERA_STATE_COCKPIT || cameraState == CAMERA_STATE_HOME_SCREEN || cameraState == CAMERA_STATE_LOAD_SCREEN)
                _currentCameraState = cameraState;
        }

        private void HandleReceiveSystemEvent(object sender, SimConnectSystemEvent e)
        {
            // TBD
        }
    }
}
