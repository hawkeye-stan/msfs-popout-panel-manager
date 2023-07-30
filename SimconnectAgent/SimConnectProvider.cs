using MSFSPopoutPanelManager.DomainModel.Profile;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using static MSFSPopoutPanelManager.SimConnectAgent.SimDataDefinitions;

namespace MSFSPopoutPanelManager.SimConnectAgent
{
    public class SimConnectProvider
    {
        private const int MSFS_DATA_REFRESH_TIMEOUT = 500;
        private const int MSFS_HUDBAR_DATA_REFRESH_TIMEOUT = 100;

        private SimConnector _simConnector;

        private bool _isHandlingCriticalError;
        private List<SimDataItem> _requiedSimData;

        private System.Timers.Timer _requiredRequestDataTimer;
        private System.Timers.Timer _hudBarRequestDataTimer;
        private bool _isPowerOnForPopOut;
        private bool _isAvionicsOnForPopOut;
        private bool _isTrackIRManaged;
        private bool _isHudBarDataActive;
        private HudBarType _activeHudBarType;

        public event EventHandler OnConnected;
        public event EventHandler OnDisconnected;
        public event EventHandler<bool> OnIsInCockpitChanged;
        public event EventHandler OnFlightStarted;
        public event EventHandler OnFlightStopped;
        public event EventHandler<List<SimDataItem>> OnSimConnectDataRequiredRefreshed;
        public event EventHandler<List<SimDataItem>> OnSimConnectDataHudBarRefreshed;
        public event EventHandler<string> OnActiveAircraftChanged;

        public SimConnectProvider()
        {
            _simConnector = new SimConnector();
            _simConnector.OnConnected += HandleSimConnected;
            _simConnector.OnDisconnected += HandleSimDisonnected;
            _simConnector.OnException += HandleSimException;
            _simConnector.OnReceiveSystemEvent += HandleReceiveSystemEvent;
            _simConnector.OnReceivedRequiredData += HandleRequiredDataReceived;
            _simConnector.OnReceivedHudBarData += HandleHudBarDataReceived;
            _simConnector.OnActiveAircraftChanged += (sender, e) => OnActiveAircraftChanged?.Invoke(this, e);

            _isHandlingCriticalError = false;

            _isHudBarDataActive = false;
            _activeHudBarType = HudBarType.None;
        }

        public void Start()
        {
            _simConnector.Stop();
            Thread.Sleep(2000);     // wait for everything to stop
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

        public void SetHudBarConfig(HudBarType hudBarType)
        {
            if (_hudBarRequestDataTimer.Enabled && _activeHudBarType == hudBarType)
                return;

            _activeHudBarType = hudBarType;
            _isHudBarDataActive = true;

            // shut down data request and wait for the last request to be completed
            _hudBarRequestDataTimer.Stop();
            Thread.Sleep(MSFS_HUDBAR_DATA_REFRESH_TIMEOUT);

            switch (hudBarType)
            {
                case HudBarType.Generic_Aircraft:
                    _simConnector.SetSimConnectHudBarDataDefinition(SimDataDefinitionType.GenericHudBar);
                    _hudBarRequestDataTimer.Start();
                    break;
                case HudBarType.PMDG_737:
                    _simConnector.SetSimConnectHudBarDataDefinition(SimDataDefinitionType.PMDG737HudBar);
                    _hudBarRequestDataTimer.Start();
                    break;
                default:
                    _simConnector.SetSimConnectHudBarDataDefinition(SimDataDefinitionType.NoHudBar);
                    _hudBarRequestDataTimer.Stop();
                    break;
            }
        }

        public void StopHudBar()
        {
            _hudBarRequestDataTimer.Stop();
        }

        public void TurnOnPower(bool isRequiredForColdStart)
        {
            if (!isRequiredForColdStart || _requiedSimData == null)
                return;

            // Wait for _simData.AtcOnParkingSpot to refresh
            Thread.Sleep(MSFS_DATA_REFRESH_TIMEOUT + 500);

            var planeInParkingSpot = Convert.ToBoolean(_requiedSimData.Find(d => d.PropertyName == SimDataDefinitions.PropName.PlaneInParkingSpot).Value);

            if (isRequiredForColdStart && planeInParkingSpot)
            {
                Debug.WriteLine("Turn On Battery Power...");

                _isPowerOnForPopOut = true;
                _simConnector.TransmitActionEvent(ActionEvent.MASTER_BATTERY_SET, 1);
            }
        }

        public void TurnOffPower(bool isRequiredForColdStart)
        {
            if (!isRequiredForColdStart || _requiedSimData == null)
                return;

            if (_isPowerOnForPopOut)
            {
                Debug.WriteLine("Turn Off Battery Power...");

                _simConnector.TransmitActionEvent(ActionEvent.MASTER_BATTERY_SET, 0);
                _isPowerOnForPopOut = false;
            }
        }

        public void TurnOnAvionics(bool isRequiredForColdStart)
        {
            if (!isRequiredForColdStart || _requiedSimData == null)
                return;

            var planeInParkingSpot = Convert.ToBoolean(_requiedSimData.Find(d => d.PropertyName == SimDataDefinitions.PropName.PlaneInParkingSpot).Value);

            Debug.WriteLine("Turn On Avionics...");

            if (isRequiredForColdStart && planeInParkingSpot)
            {
                _isAvionicsOnForPopOut = true;
                _simConnector.TransmitActionEvent(ActionEvent.AVIONICS_MASTER_SET, 1);
            }
        }

        public void TurnOffAvionics(bool isRequiredForColdStart)
        {
            if (!isRequiredForColdStart || _requiedSimData == null)
                return;

            if (_isAvionicsOnForPopOut)
            {
                Debug.WriteLine("Turn Off Avionics...");

                _simConnector.TransmitActionEvent(ActionEvent.AVIONICS_MASTER_SET, 0);
                _isAvionicsOnForPopOut = false;
            }
        }

        public void TurnOnTrackIR()
        {
            if (_requiedSimData != null)
            {
                var trackIREnable = Convert.ToBoolean(_requiedSimData.Find(d => d.PropertyName == SimDataDefinitions.PropName.TrackIREnable).Value);

                if (_isTrackIRManaged)
                {
                    Debug.WriteLine("Turn On TrackIR...");
                    SetTrackIREnable(true);
                    _isTrackIRManaged = false;
                }
            }
        }

        public void TurnOffTrackIR()
        {
            if (_requiedSimData != null)
            {
                var trackIREnable = Convert.ToBoolean(_requiedSimData.Find(d => d.PropertyName == SimDataDefinitions.PropName.TrackIREnable).Value);

                if (trackIREnable)
                {
                    Debug.WriteLine("Turn Off TrackIR...");

                    SetTrackIREnable(false);
                    _isTrackIRManaged = true;
                }
            }

            Thread.Sleep(200);
        }

        public void TurnOnActivePause()
        {
            Debug.WriteLine($"Acitve Pause On...");

            _simConnector.TransmitActionEvent(ActionEvent.PAUSE_SET, 1);
        }

        public void TurnOffActivePause()
        {
            Debug.WriteLine($"Acitve Pause Off...");

            _simConnector.TransmitActionEvent(ActionEvent.PAUSE_SET, 0);
        }

        public void IncreaseSimRate()
        {
            _simConnector.TransmitActionEvent(ActionEvent.SIM_RATE_INCR, 1);
            Thread.Sleep(200);
        }

        public void DecreaseSimRate()
        {
            _simConnector.TransmitActionEvent(ActionEvent.SIM_RATE_DECR, 1);
            Thread.Sleep(200);
        }

        public void SetCockpitCameraZoomLevel(int zoomLevel)
        {
            _simConnector.SetDataObject(WriteableVariableName.CockpitCameraZoom, Convert.ToDouble(zoomLevel));
        }

        private void SetTrackIREnable(bool enable)
        {
            _simConnector.SetDataObject(WriteableVariableName.TrackIREnable, enable ? Convert.ToDouble(1) : Convert.ToDouble(0));
        }

        private void HandleSimConnected(object source, EventArgs e)
        {
            // Setup required data request timer
            _requiredRequestDataTimer = new System.Timers.Timer();
            _requiredRequestDataTimer.Interval = MSFS_DATA_REFRESH_TIMEOUT;
            _requiredRequestDataTimer.Start();
            _requiredRequestDataTimer.Elapsed += (sender, e) => { try { _simConnector.RequestRequiredData(); } catch { } };
            _requiredRequestDataTimer.Elapsed += (sender, e) => { try { _simConnector.ReceiveMessage(); } catch { } };

            // Setup hudbar data request timer
            _hudBarRequestDataTimer = new System.Timers.Timer();
            _hudBarRequestDataTimer.Interval = MSFS_HUDBAR_DATA_REFRESH_TIMEOUT;
            _hudBarRequestDataTimer.Stop();
            _hudBarRequestDataTimer.Elapsed += (sender, e) => { try { _simConnector.RequestHudBarData(); } catch { } }; ;

            if (_isHudBarDataActive)
                SetHudBarConfig(_activeHudBarType);

            OnConnected?.Invoke(this, null);
        }

        private void HandleSimDisonnected(object source, EventArgs e)
        {
            _requiredRequestDataTimer.Stop();
            _hudBarRequestDataTimer.Stop();
            OnDisconnected?.Invoke(this, null);
            StopAndReconnect();
        }

        private void HandleSimException(object source, string e)
        {
            _requiredRequestDataTimer.Stop();
            _hudBarRequestDataTimer.Stop();

            if (!_isHandlingCriticalError)
            {
                _isHandlingCriticalError = true;     // Prevent restarting to occur in parallel
                StopAndReconnect();
                _isHandlingCriticalError = false;
            }
        }

        private void HandleRequiredDataReceived(object sender, List<SimDataItem> e)
        {
            _requiedSimData = e;
            DetectFlightStartedOrStopped(e);
            OnSimConnectDataRequiredRefreshed?.Invoke(this, e);
        }

        private void HandleHudBarDataReceived(object sender, List<SimDataItem> e)
        {
            OnSimConnectDataHudBarRefreshed?.Invoke(this, e);
        }

        private const int CAMERA_STATE_COCKPIT = 2;
        private const int CAMERA_STATE_LOAD_SCREEN = 11;
        private const int CAMERA_STATE_HOME_SCREEN = 15;
        private int _currentCameraState = -1;

        private void DetectFlightStartedOrStopped(List<SimDataItem> simData)
        {
            // Determine is flight started or ended
            var cameraState = Convert.ToInt32(simData.Find(d => d.PropertyName == SimDataDefinitions.PropName.CameraState).Value);

            if (_currentCameraState == cameraState)
                return;

            if (cameraState == CAMERA_STATE_COCKPIT)
                OnIsInCockpitChanged?.Invoke(this, true);

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
                    if (cameraState == CAMERA_STATE_LOAD_SCREEN || cameraState == CAMERA_STATE_HOME_SCREEN)
                    {
                        _currentCameraState = cameraState;
                        OnFlightStopped?.Invoke(this, null);
                        OnIsInCockpitChanged?.Invoke(this, false);

                        _isHudBarDataActive = false;
                        _hudBarRequestDataTimer.Stop();
                    }

                    break;
            }

            if (cameraState == CAMERA_STATE_COCKPIT || cameraState == CAMERA_STATE_HOME_SCREEN || cameraState == CAMERA_STATE_LOAD_SCREEN)
                _currentCameraState = cameraState;
        }

        private void HandleReceiveSystemEvent(object sender, SimConnectEvent e)
        {
            // TBD
        }
    }
}
