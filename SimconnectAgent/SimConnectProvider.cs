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
        private const int MSFS_HUDBAR_DATA_REFRESH_TIMEOUT = 200;
        private const int MSFS_DYNAMICLOD_DATA_REFRESH_TIMEOUT = 300;

        private readonly SimConnector _simConnector;

        private bool _isHandlingCriticalError;
        private List<SimDataItem> _requiredSimData;

        private System.Timers.Timer _requiredRequestDataTimer;
        private System.Timers.Timer _hudBarRequestDataTimer;
        private System.Timers.Timer _dynamicLodRequestDataTimer;
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
        public event EventHandler OnException;
        public event EventHandler<List<SimDataItem>> OnSimConnectDataRequiredRefreshed;
        public event EventHandler<List<SimDataItem>> OnSimConnectDataHudBarRefreshed;
        public event EventHandler<List<SimDataItem>> OnSimConnectDataDynamicLodRefreshed;
        public event EventHandler<int> OnSimConnectDataEventFrameRefreshed;
        public event EventHandler<string> OnActiveAircraftChanged;

        public SimConnectProvider()
        {
            _simConnector = new SimConnector();
            _simConnector.OnConnected += HandleSimConnected;
            _simConnector.OnDisconnected += HandleSimDisconnected;
            _simConnector.OnException += HandleSimException;
            _simConnector.OnReceiveSystemEvent += HandleReceiveSystemEvent;
            _simConnector.OnReceivedRequiredData += HandleRequiredDataReceived;
            _simConnector.OnReceivedHudBarData += (_, e) => OnSimConnectDataHudBarRefreshed?.Invoke(this, e); 
            _simConnector.OnReceivedDynamicLodData += (_, e) => OnSimConnectDataDynamicLodRefreshed?.Invoke(this, e);
            _simConnector.OnReceivedEventFrameData += (_, e) => OnSimConnectDataEventFrameRefreshed?.Invoke(this, e);
            _simConnector.OnActiveAircraftChanged += (_, e) => OnActiveAircraftChanged?.Invoke(this, e);

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
                OnDisconnected?.Invoke(this, EventArgs.Empty);
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
                    _simConnector.SetSimConnectHudBarDataDefinition(SimDataDefinitionType.Pmdg737HudBar);
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

        public void StartDynamicLod()
        {
            if (_dynamicLodRequestDataTimer.Enabled)
                return;
            
            // shut down data request and wait for the last request to be completed
            _dynamicLodRequestDataTimer.Stop();
            Thread.Sleep(MSFS_DYNAMICLOD_DATA_REFRESH_TIMEOUT);

            _simConnector.SetSimConnectDynamicLodDataDefinition();

            _dynamicLodRequestDataTimer.Start();

            _simConnector.StartReceiveFrameData();
        }

        public void StopDynamicLod()
        {
            _dynamicLodRequestDataTimer.Stop();
            _simConnector.StopReceiveFrameData();
        }


        public void TurnOnPower(bool isRequiredForColdStart)
        {
            if (!isRequiredForColdStart || _requiredSimData == null)
                return;

            // Wait for _simData.AtcOnParkingSpot to refresh
            Thread.Sleep(MSFS_DATA_REFRESH_TIMEOUT + 500);

            var planeInParkingSpot = Convert.ToBoolean(_requiredSimData.Find(d => d.PropertyName == PropName.PlaneInParkingSpot).Value);

            if (!planeInParkingSpot) 
                return;

            Debug.WriteLine("Turn On Battery Power...");

            _isPowerOnForPopOut = true;
            _simConnector.TransmitActionEvent(ActionEvent.MASTER_BATTERY_SET, 1);
        }

        public void TurnOffPower(bool isRequiredForColdStart)
        {
            if (!isRequiredForColdStart || _requiredSimData == null)
                return;

            if (!_isPowerOnForPopOut)
                return;

            Debug.WriteLine("Turn Off Battery Power...");

            _simConnector.TransmitActionEvent(ActionEvent.MASTER_BATTERY_SET, 0);
            _isPowerOnForPopOut = false;
        }

        public void TurnOnAvionics(bool isRequiredForColdStart)
        {
            if (!isRequiredForColdStart || _requiredSimData == null)
                return;

            var planeInParkingSpot = Convert.ToBoolean(_requiredSimData.Find(d => d.PropertyName == PropName.PlaneInParkingSpot).Value);
            
            if (!planeInParkingSpot)
                return;

            Debug.WriteLine("Turn On Avionics...");

            _isAvionicsOnForPopOut = true;
            _simConnector.TransmitActionEvent(ActionEvent.AVIONICS_MASTER_SET, 1);
        }

        public void TurnOffAvionics(bool isRequiredForColdStart)
        {
            if (!isRequiredForColdStart || _requiredSimData == null)
                return;

            if (!_isAvionicsOnForPopOut) 
                return;

            Debug.WriteLine("Turn Off Avionics...");

            _simConnector.TransmitActionEvent(ActionEvent.AVIONICS_MASTER_SET, 0);
            _isAvionicsOnForPopOut = false;
        }

        public void TurnOnTrackIR()
        {
            if (_requiredSimData == null)
                return;

            if (!_isTrackIRManaged)
                return;

            Debug.WriteLine("Turn On TrackIR...");
            SetTrackIREnable(true);
            _isTrackIRManaged = false;
        }

        public void TurnOffTrackIR()
        {
            if (_requiredSimData == null) 
                return;

            var trackIREnable = Convert.ToBoolean(_requiredSimData.Find(d => d.PropertyName == PropName.TrackIREnable).Value);

            if (!trackIREnable) 
                return;

            Debug.WriteLine("Turn Off TrackIR...");

            SetTrackIREnable(false);
            _isTrackIRManaged = true;
            Thread.Sleep(200);
        }

        public void TurnOnActivePause()
        {
            Debug.WriteLine("Active Pause On...");

            _simConnector.TransmitActionEvent(ActionEvent.PAUSE_SET, 1);
        }

        public void TurnOffActivePause()
        {
            Debug.WriteLine("Active Pause Off...");

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

        public void SetCameraState(CameraState cameraState)
        {
            _simConnector.SetDataObject(WritableVariableName.CameraState, Convert.ToDouble(cameraState));
        }

        public void SetCockpitCameraZoomLevel(int zoomLevel)
        {
            _simConnector.SetDataObject(WritableVariableName.CockpitCameraZoom, Convert.ToDouble(zoomLevel));
        }

        public void SetCameraRequestAction(int actionEnum)
        {
            _simConnector.SetDataObject(WritableVariableName.CameraRequestAction, Convert.ToDouble(actionEnum));
        }

        public void SetCameraViewTypeAndIndex0(int actionEnum)
        {
            _simConnector.SetDataObject(WritableVariableName.CameraViewTypeAndIndex0, Convert.ToDouble(actionEnum));
        }

        public void SetCameraViewTypeAndIndex1(int actionEnum)
        {
            _simConnector.SetDataObject(WritableVariableName.CameraViewTypeAndIndex1, Convert.ToDouble(actionEnum));
        }

        private void SetTrackIREnable(bool enable)
        {
            _simConnector.SetDataObject(WritableVariableName.TrackIREnable, enable ? Convert.ToDouble(1) : Convert.ToDouble(0));
        }

        private void HandleSimConnected(object source, EventArgs e)
        {
            // Setup required data request timer
            _requiredRequestDataTimer = new()
            {
                Interval = MSFS_DATA_REFRESH_TIMEOUT
            };
            _requiredRequestDataTimer.Start();
            _requiredRequestDataTimer.Elapsed += (_, _) =>
            {
                try
                {
                    _simConnector.RequestRequiredData(); 
                    _simConnector.ReceiveMessage();
                }
                catch
                {
                    // ignored
                }
            };

            // Setup hudbar data request timer
            _hudBarRequestDataTimer = new()
            {
                Interval = MSFS_HUDBAR_DATA_REFRESH_TIMEOUT,
            };
            _hudBarRequestDataTimer.Stop();
            _hudBarRequestDataTimer.Elapsed += (_, _) =>
            {
                try
                {
                    _simConnector.RequestHudBarData();
                }
                catch
                {
                    // ignored
                }
            };
            
            if (_isHudBarDataActive)
                SetHudBarConfig(_activeHudBarType);

            // Setup dynamic data request timer
            _dynamicLodRequestDataTimer = new()
            {
                Interval = MSFS_DYNAMICLOD_DATA_REFRESH_TIMEOUT,
            };
            _dynamicLodRequestDataTimer.Stop();
            _dynamicLodRequestDataTimer.Elapsed += (_, _) =>
            {
                try
                {
                    _simConnector.RequestDynamicLodData();
                }
                catch
                {
                    // ignored
                }
            };


            OnConnected?.Invoke(this, EventArgs.Empty);
        }

        private void HandleSimDisconnected(object source, EventArgs e)
        {
            _requiredRequestDataTimer.Stop();
            _hudBarRequestDataTimer.Stop();
            _dynamicLodRequestDataTimer.Stop();
            OnDisconnected?.Invoke(this, EventArgs.Empty);
            StopAndReconnect();
        }

        private void HandleSimException(object source, string e)
        {
            OnException?.Invoke(this, EventArgs.Empty);

            _requiredRequestDataTimer.Stop();
            _hudBarRequestDataTimer.Stop();
            _dynamicLodRequestDataTimer.Stop();

            if (!_isHandlingCriticalError)
            {
                _isHandlingCriticalError = true;     // Prevent restarting to occur in parallel
                StopAndReconnect();
                _isHandlingCriticalError = false;
            }
        }

        private void HandleRequiredDataReceived(object sender, List<SimDataItem> e)
        {
            _requiredSimData = e;
            DetectFlightStartedOrStopped(e);
            OnSimConnectDataRequiredRefreshed?.Invoke(this, e);
        }

        private CameraState _currentCameraState = CameraState.Unknown;

        private void DetectFlightStartedOrStopped(List<SimDataItem> simData)
        {
            // Determine is flight started or ended
            var cameraStateInt = Convert.ToInt32(simData.Find(d => d.PropertyName == PropName.CameraState).Value);

            var success = Enum.TryParse<CameraState>(cameraStateInt.ToString(), out var cameraState);
            if(!success)
                cameraState = CameraState.Unknown;

            if (_currentCameraState == cameraState)
                return;

            if (cameraState == CameraState.Cockpit)
                OnIsInCockpitChanged?.Invoke(this, true);


            switch (_currentCameraState)
            {
                case CameraState.HomeScreen:
                case CameraState.LoadScreen:
                    if (cameraState == CameraState.Cockpit)
                    {
                        _currentCameraState = cameraState;
                        OnFlightStarted?.Invoke(this, EventArgs.Empty);
                    }

                    break;
                case CameraState.Cockpit:
                    if (cameraState == CameraState.LoadScreen || cameraState == CameraState.HomeScreen)
                    {
                        _currentCameraState = cameraState;
                        OnFlightStopped?.Invoke(this, EventArgs.Empty);
                        OnIsInCockpitChanged?.Invoke(this, false);

                        _isHudBarDataActive = false;
                        _hudBarRequestDataTimer.Stop();

                        _dynamicLodRequestDataTimer.Stop();
                    }
                    break;
            }

            if (cameraState is CameraState.Cockpit or CameraState.HomeScreen or CameraState.LoadScreen)
                _currentCameraState = cameraState;
        }

        private void HandleReceiveSystemEvent(object sender, SimConnectEvent e)
        {
            // TBD
        }
    }
}
