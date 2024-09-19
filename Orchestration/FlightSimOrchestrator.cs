using MSFSPopoutPanelManager.DomainModel.Profile;
using MSFSPopoutPanelManager.DomainModel.SimConnect;
using MSFSPopoutPanelManager.Shared;
using MSFSPopoutPanelManager.SimConnectAgent;
using MSFSPopoutPanelManager.WindowsAgent;
using System;
using System.Collections.Generic;
using System.Threading;

namespace MSFSPopoutPanelManager.Orchestration
{
    public class FlightSimOrchestrator : BaseOrchestrator
    {
        private const int MSFS_GAME_EXIT_DETECTION_INTERVAL = 3000;
        private System.Timers.Timer _msfsGameExitDetectionTimer;
        private SimConnectProvider _simConnectProvider;

        private readonly DynamicLodOrchestrator _dynamicLodOrchestrator;
        private bool _isTurnedOnPower;
        private bool _isTurnedOnAvionics;

        public FlightSimOrchestrator(SharedStorage sharedStorage, DynamicLodOrchestrator dynamicLodOrchestrator) : base(sharedStorage)
        {
            _dynamicLodOrchestrator = dynamicLodOrchestrator;
            _simConnectProvider = new SimConnectProvider();
        }

        public event EventHandler OnSimulatorExited;

        public event EventHandler OnFlightStarted;

        public event EventHandler OnFlightStopped;

        public void StartSimConnectServer()
        {
            _simConnectProvider ??= new SimConnectProvider();

            _simConnectProvider.OnConnected += (_, _) =>
            {
                FlightSimData.IsSimConnectActive = true;
                FlightSimData.IsSimulatorStarted = true;
                WindowProcessManager.GetSimulatorProcess();     // refresh simulator process
                DetectMsfsExit();

                StartDynamicLod();
            };

            _simConnectProvider.OnDisconnected += (_, _) =>
            {
                FlightSimData.IsSimConnectDataReceived = false;
                FlightSimData.IsSimConnectActive = false;
                WindowProcessManager.GetSimulatorProcess();     // refresh simulator process
                FlightSimData.Reset();
            };

            _simConnectProvider.OnException += (_, _) =>
            {
                FlightSimData.IsSimConnectDataReceived = false;
                FlightSimData.IsSimConnectActive = false;
            };

            _simConnectProvider.OnSimConnectDataRequiredRefreshed += (_, e) =>
            {
                MapRequiredSimConnectData(e);
            };

            _simConnectProvider.OnSimConnectDataHudBarRefreshed += (_, e) =>
            {
                if (!ProfileData.ActiveProfile.ProfileSetting.HudBarConfig.IsEnabled || !FlightSimData.IsFlightStarted) 
                    return;

                MapHudBarSimConnectData(e);
            };

            var _lastDynamicLodPause = DateTime.Now;
            var _isDynamicLodPausePrevously = true;
            var _lastDyanmicLodUpdatedTime = DateTime.Now;

            _simConnectProvider.OnSimConnectDataDynamicLodRefreshed += (_, e) =>
            {
                if (!AppSettingData.ApplicationSetting.DynamicLodSetting.IsEnabled || !FlightSimData.IsFlightStarted)
                    return;

                var isPaused = (AppSettingData.ApplicationSetting.DynamicLodSetting.PauseWhenMsfsLoseFocus && !WindowActionManager.IsMsfsInFocus()) ||
                               (AppSettingData.ApplicationSetting.DynamicLodSetting.PauseOutsideCockpitView && FlightSimData.CameraState != CameraState.Cockpit);

                if (_isDynamicLodPausePrevously && !isPaused)
                {
                    if (DateTime.Now - _lastDynamicLodPause <= TimeSpan.FromSeconds(3))
                        return;
                    
                    _lastDynamicLodPause = DateTime.Now;
                    _isDynamicLodPausePrevously = false;
                }
                else if (isPaused)
                {
                    _isDynamicLodPausePrevously = true;
                    return;
                }
                
                if (DateTime.Now - _lastDyanmicLodUpdatedTime <= TimeSpan.FromSeconds(0.4))     // take FPS sample every 0.4 seconds
                    return;
                
                _lastDyanmicLodUpdatedTime = DateTime.Now;
            
                MapDynamicLodSimConnectData(e);
                _dynamicLodOrchestrator.UpdateLod();
            };

            _simConnectProvider.OnSimConnectDataEventFrameRefreshed += (_, e) =>
            {
                if (!AppSettingData.ApplicationSetting.DynamicLodSetting.IsEnabled || !FlightSimData.IsFlightStarted)
                    return;

                MapEventFrameData(e);
            };

            _simConnectProvider.OnActiveAircraftChanged += (_, e) =>
            {
                var aircraftName = string.IsNullOrEmpty(e) ? null : e;
                if (FlightSimData.AircraftName != aircraftName)
                {
                    FlightSimData.AircraftName = aircraftName;
                    ProfileData.RefreshProfile();
                }
            };

            _simConnectProvider.OnFlightStarted += HandleOnFlightStarted;
            _simConnectProvider.OnFlightStopped += HandleOnFlightStopped;
            _simConnectProvider.OnIsInCockpitChanged += (_, e) =>
            {
                FlightSimData.IsInCockpit = e;
                if (e)
                    FlightSimData.IsFlightStarted = true;
            };
            _simConnectProvider.Start();
        }

        public void EndSimConnectServer(bool appExit)
        {
            _simConnectProvider.Stop(appExit);
            _simConnectProvider = null;
        }

        public void TurnOnTrackIR()
        {
            if (_simConnectProvider == null)
                return;

            if (!AppSettingData.ApplicationSetting.TrackIRSetting.AutoDisableTrackIR)
                return;

            WorkflowStepWithMessage.Execute("Turning on TrackIR", () =>
            {
                var count = 0;
                do
                {
                    _simConnectProvider.TurnOnTrackIR();
                    Thread.Sleep(500);
                    count++;
                }
                while (!FlightSimData.TrackIRStatus && count < 5);

                return FlightSimData.TrackIRStatus;
            });
        }

        public void TurnOffTrackIR()
        {
            if (_simConnectProvider == null)
                return;

            if (!AppSettingData.ApplicationSetting.TrackIRSetting.AutoDisableTrackIR)
                return;

            WorkflowStepWithMessage.Execute("Turning off TrackIR", () =>
            {
                var count = 0;
                do
                {
                    _simConnectProvider.TurnOffTrackIR();
                    Thread.Sleep(500);
                    count++;
                }
                while (FlightSimData.TrackIRStatus && count < 5);

                return !FlightSimData.TrackIRStatus;
            });
        }

        public void TurnOnPower()
        {
            if (_simConnectProvider == null)
                return;

            if (ProfileData.ActiveProfile == null || FlightSimData.ElectricalMasterBatteryStatus)
                return;

            _isTurnedOnPower = true;

            WorkflowStepWithMessage.Execute("Turning on battery", () =>
            {
                var count = 0;
                do
                {
                    _simConnectProvider.TurnOnPower(ProfileData.ActiveProfile.ProfileSetting.PowerOnRequiredForColdStart);
                    Thread.Sleep(500);
                    count++;
                }
                while (!FlightSimData.ElectricalMasterBatteryStatus && count < 10);

                return FlightSimData.ElectricalMasterBatteryStatus;
            });
        }

        public void TurnOffPower()
        {
            if (_simConnectProvider == null)
                return;

            if (ProfileData.ActiveProfile == null || !_isTurnedOnPower)
                return;

            WorkflowStepWithMessage.Execute("Turning off battery", () =>
            {
                var count = 0;
                do
                {
                    _simConnectProvider.TurnOffPower(ProfileData.ActiveProfile.ProfileSetting.PowerOnRequiredForColdStart);
                    Thread.Sleep(500);
                    count++;
                }
                while (FlightSimData.ElectricalMasterBatteryStatus && count < 10);

                return !FlightSimData.ElectricalMasterBatteryStatus;
            });

            _isTurnedOnPower = false;
        }

        public void TurnOnAvionics()
        {
            if (_simConnectProvider == null)
                return;

            if (ProfileData.ActiveProfile == null || FlightSimData.AvionicsMasterSwitchStatus)
                return;

            _isTurnedOnAvionics = true;

            WorkflowStepWithMessage.Execute("Turning on avionics", () =>
            {
                var count = 0;
                do
                {
                    _simConnectProvider.TurnOnAvionics(ProfileData.ActiveProfile.ProfileSetting.PowerOnRequiredForColdStart);
                    Thread.Sleep(500);
                    count++;
                }
                while (!FlightSimData.AvionicsMasterSwitchStatus && count < 10);

                return FlightSimData.AvionicsMasterSwitchStatus;
            });
        }

        public void TurnOffAvionics()
        {
            if (_simConnectProvider == null)
                return;

            if (ProfileData.ActiveProfile == null || !_isTurnedOnAvionics)
                return;

            WorkflowStepWithMessage.Execute("Turning off avionics", () =>
            {
                var count = 0;
                do
                {
                    _simConnectProvider.TurnOffAvionics(ProfileData.ActiveProfile.ProfileSetting.PowerOnRequiredForColdStart);
                    Thread.Sleep(500);
                    count++;
                }
                while (FlightSimData.AvionicsMasterSwitchStatus && count < 10);

                return !FlightSimData.AvionicsMasterSwitchStatus;
            });

            _isTurnedOnAvionics = false;
        }

        public void TurnOnActivePause()
        {
            if (_simConnectProvider == null)
                return;

            if (!AppSettingData.ApplicationSetting.PopOutSetting.AutoActivePause)
                return;

            WorkflowStepWithMessage.Execute("Turning on active pause", () =>
            {
                _simConnectProvider.TurnOnActivePause();
            });
        }

        public void TurnOffActivePause()
        {
            if (_simConnectProvider == null)
                return;

            if (!AppSettingData.ApplicationSetting.PopOutSetting.AutoActivePause)
                return;

            WorkflowStepWithMessage.Execute("Turning off active pause", () =>
            {
                _simConnectProvider.TurnOffActivePause();
            });
        }

        public void IncreaseSimRate()
        {
            if (_simConnectProvider == null)
                return;

            if (Convert.ToInt32(FlightSimData.HudBarData.SimRate) == 16)
                return;

            _simConnectProvider.IncreaseSimRate();
        }

        public void DecreaseSimRate()
        {
            if (_simConnectProvider == null)
                return;

            if (FlightSimData.HudBarData.SimRate.ToString("N2") == "0.25")
                return;

            _simConnectProvider.DecreaseSimRate();
        }

        public void SetCockpitCameraZoomLevel(int zoomLevel)
        {
            _simConnectProvider.SetCockpitCameraZoomLevel(zoomLevel);
        }

        public void ResetCameraView()
        {
            _simConnectProvider.SetCameraRequestAction(1);
        }

        public void SetFixedCamera(CameraType cameraType, int index)
        {
            if (FlightSimData.CameraState != CameraState.Cockpit)
            {
                _simConnectProvider.SetCameraState(CameraState.Cockpit);
                Thread.Sleep(250);
                ResetCameraView();
                Thread.Sleep(250);
            }

            _simConnectProvider.SetCameraViewTypeAndIndex0(Convert.ToInt32(cameraType));
            Thread.Sleep(250);
            _simConnectProvider.SetCameraViewTypeAndIndex1(index);
        }

        public void SetHudBarConfig()
        {
            if (_simConnectProvider == null)
                return;

            var hudBarType = ProfileData.ActiveProfile.ProfileSetting.HudBarConfig.HudBarType;
            switch (hudBarType)
            {
                case HudBarType.PMDG_737:
                    FlightSimData.HudBarData = new HudBarData737();
                    break;
                case HudBarType.Generic_Aircraft:
                default:
                    FlightSimData.HudBarData = new HudBarDataGeneric();
                    break;
            }

            _simConnectProvider.SetHudBarConfig(hudBarType);
        }

        public void StopHudBar()
        {
            _simConnectProvider.StopHudBar();
        }

        private void HandleOnFlightStarted(object sender, EventArgs e)
        {
            OnFlightStarted?.Invoke(this, EventArgs.Empty);

            FlightSimData.IsFlightStarted = true;

            StartDynamicLod();
        }

        private void HandleOnFlightStopped(object sender, EventArgs e)
        {
            ProfileData.ResetActiveProfile();

            OnFlightStopped?.Invoke(this, EventArgs.Empty);

            CloseAllPopOuts();
            
            FlightSimData.HudBarData?.Clear();

            FlightSimData.IsFlightStarted = false;

            StopDynamicLod();
            FpsCalc.Reset();
            FlightSimData.DynamicLodSimData.Clear();
        }

        private void DetectMsfsExit()
        {
            _msfsGameExitDetectionTimer = new System.Timers.Timer();
            _msfsGameExitDetectionTimer.Interval = MSFS_GAME_EXIT_DETECTION_INTERVAL;
            _msfsGameExitDetectionTimer.Enabled = true;
            _msfsGameExitDetectionTimer.Elapsed += (_, _) =>
            {
                WindowProcessManager.GetSimulatorProcess();
                if (WindowProcessManager.SimulatorProcess != null) 
                    return;

                if (AppSettingData.ApplicationSetting.GeneralSetting.AutoClose)
                {
                    OnSimulatorExited?.Invoke(this, EventArgs.Empty);
                    return;
                }
                
                FlightSimData.Reset();
                _simConnectProvider.StopAndReconnect();
            };
        }

        private void MapRequiredSimConnectData(List<SimDataItem> simData)
        {
            var electricalMasterBattery = Convert.ToBoolean(simData.Find(d => d.PropertyName == SimDataDefinitions.PropName.ElectricalMasterBattery).Value);
            if (electricalMasterBattery != FlightSimData.ElectricalMasterBatteryStatus)
                    FlightSimData.ElectricalMasterBatteryStatus = electricalMasterBattery;

            var avionicsMasterSwitch = Convert.ToBoolean(simData.Find(d => d.PropertyName == SimDataDefinitions.PropName.AvionicsMasterSwitch).Value);
            if (avionicsMasterSwitch != FlightSimData.AvionicsMasterSwitchStatus)
                FlightSimData.AvionicsMasterSwitchStatus = avionicsMasterSwitch;

            var trackIR = Convert.ToBoolean(simData.Find(d => d.PropertyName == SimDataDefinitions.PropName.TrackIREnable).Value);
            if (trackIR != FlightSimData.TrackIRStatus)
                FlightSimData.TrackIRStatus = trackIR;

            var cameraStateInt = Convert.ToInt32(simData.Find(d => d.PropertyName == SimDataDefinitions.PropName.CameraState).Value);
            var result = Enum.TryParse<CameraState>(cameraStateInt.ToString(), out var cameraState);
            if (!result)
                cameraState = CameraState.Unknown;
            if (cameraState != FlightSimData.CameraState)
                FlightSimData.CameraState = cameraState;
            
            var cockpitCameraZoom = Convert.ToInt32(simData.Find(d => d.PropertyName == SimDataDefinitions.PropName.CockpitCameraZoom).Value);
            if (cockpitCameraZoom != FlightSimData.CockpitCameraZoom)
                FlightSimData.CockpitCameraZoom = cockpitCameraZoom;

            var cameraViewTypeAndIndex0 = Convert.ToInt32(simData.Find(d => d.PropertyName == SimDataDefinitions.PropName.CameraViewTypeAndIndex0).Value);
            if (cameraViewTypeAndIndex0 != FlightSimData.CameraViewTypeAndIndex0)
                FlightSimData.CameraViewTypeAndIndex0 = cameraViewTypeAndIndex0;

            var cameraViewTypeAndIndex1 = Convert.ToInt32(simData.Find(d => d.PropertyName == SimDataDefinitions.PropName.CameraViewTypeAndIndex1).Value);
            if (cameraViewTypeAndIndex1 != FlightSimData.CameraViewTypeAndIndex1)
                FlightSimData.CameraViewTypeAndIndex1 = cameraViewTypeAndIndex1;

            var cameraViewTypeAndIndex1Max = Convert.ToInt32(simData.Find(d => d.PropertyName == SimDataDefinitions.PropName.CameraViewTypeAndIndex1Max).Value);
            if (cameraViewTypeAndIndex1Max != FlightSimData.CameraViewTypeAndIndex1Max)
                FlightSimData.CameraViewTypeAndIndex1Max = cameraViewTypeAndIndex1Max;

            var cameraViewTypeAndIndex2Max = Convert.ToInt32(simData.Find(d => d.PropertyName == SimDataDefinitions.PropName.CameraViewTypeAndIndex2Max).Value);
            if (cameraViewTypeAndIndex2Max != FlightSimData.CameraViewTypeAndIndex2Max)
                FlightSimData.CameraViewTypeAndIndex2Max = cameraViewTypeAndIndex2Max;

            FlightSimData.IsSimConnectDataReceived = true;
        }

        private void MapHudBarSimConnectData(List<SimDataItem> simData)
        {
            if (CompareSimConnectData(simData, SimDataDefinitions.PropName.ElevatorTrim, FlightSimData.HudBarData.ElevatorTrim, out var newValue))
                FlightSimData.HudBarData.ElevatorTrim = newValue;

            if (CompareSimConnectData(simData, SimDataDefinitions.PropName.AileronTrim, FlightSimData.HudBarData.AileronTrim, out newValue))
                FlightSimData.HudBarData.AileronTrim = newValue;

            if (CompareSimConnectData(simData, SimDataDefinitions.PropName.RudderTrim, FlightSimData.HudBarData.RudderTrim, out newValue))
                FlightSimData.HudBarData.RudderTrim = newValue;

            if (CompareSimConnectData(simData, SimDataDefinitions.PropName.ParkingBrake, FlightSimData.HudBarData.ParkingBrake, out newValue))
                FlightSimData.HudBarData.ParkingBrake = newValue;

            if (CompareSimConnectData(simData, SimDataDefinitions.PropName.Flap, FlightSimData.HudBarData.Flap, out newValue))
                FlightSimData.HudBarData.Flap = newValue;

            if (CompareSimConnectData(simData, SimDataDefinitions.PropName.GearLeft, FlightSimData.HudBarData.GearLeft, out newValue))
                FlightSimData.HudBarData.GearLeft = newValue;

            if (CompareSimConnectData(simData, SimDataDefinitions.PropName.GearCenter, FlightSimData.HudBarData.GearCenter, out newValue))
                FlightSimData.HudBarData.GearCenter = newValue;

            if (CompareSimConnectData(simData, SimDataDefinitions.PropName.GearRight, FlightSimData.HudBarData.GearRight, out newValue))
                FlightSimData.HudBarData.GearRight = newValue;

            if (CompareSimConnectData(simData, SimDataDefinitions.PropName.SimRate, FlightSimData.HudBarData.SimRate, out newValue))
                FlightSimData.HudBarData.SimRate = newValue;
        }

        private void MapDynamicLodSimConnectData(List<SimDataItem> simData)
        {
            if (CompareSimConnectData(simData, SimDataDefinitions.PropName.PlaneAltAboveGround, FlightSimData.DynamicLodSimData.Agl, out var newValue))
                FlightSimData.DynamicLodSimData.Agl = newValue;

            if (CompareSimConnectData(simData, SimDataDefinitions.PropName.PlaneAltAboveGround, FlightSimData.DynamicLodSimData.AltAboveGround, out newValue))
                FlightSimData.DynamicLodSimData.AltAboveGround = newValue;

            if (CompareSimConnectData(simData, SimDataDefinitions.PropName.PlaneAltAboveGroundMinusCg, FlightSimData.DynamicLodSimData.AltAboveGroundMinusCg, out newValue))
                FlightSimData.DynamicLodSimData.AltAboveGroundMinusCg = newValue;

            if (CompareSimConnectData(simData, SimDataDefinitions.PropName.GroundVelocity, FlightSimData.DynamicLodSimData.GroundVelocity, out newValue))
                FlightSimData.DynamicLodSimData.GroundVelocity = newValue;

            if (CompareSimConnectData(simData, SimDataDefinitions.PropName.SimOnGround, 1.0f, out newValue))
                FlightSimData.DynamicLodSimData.PlaneOnGround = Convert.ToBoolean(newValue);

            var tlod = _dynamicLodOrchestrator.ReadTlod();
            if (FlightSimData.DynamicLodSimData.Tlod != tlod)
                FlightSimData.DynamicLodSimData.Tlod = tlod;

            var olod = _dynamicLodOrchestrator.ReadOlod();
            if (FlightSimData.DynamicLodSimData.Olod != olod)
                FlightSimData.DynamicLodSimData.Olod = olod;

            var cloudQuality = _dynamicLodOrchestrator.ReadCloudQuality();
            if (FlightSimData.DynamicLodSimData.CloudQuality != cloudQuality)
                FlightSimData.DynamicLodSimData.CloudQuality = cloudQuality;

            FlightSimData.DynamicLodSimData.Fps = FpsCalc.GetAverageFps(_dynamicLodOrchestrator.ReadIsFg() ? _currentFps * 2 : _currentFps);
        }

        private int _currentFps;
        private void MapEventFrameData(int fps)
        {
            if (!AppSettingData.ApplicationSetting.DynamicLodSetting.IsEnabled)
                return;
           
            _currentFps = fps;
        }

        private bool CompareSimConnectData(List<SimDataItem> simData, string propName, double source, out double newValue)
        {
            var propData = simData.Find(d => d.PropertyName == propName);

            if (propData == null)
            {
                newValue = 0;
                return false;
            }

            var value = Convert.ToDouble(simData.Find(d => d.PropertyName == propName).Value);
            if (Math.Abs(value - source) > 0.000000000000001)
            {
                newValue = value;
                return true;
            }

            newValue = 0;
            return false;
        }

        private void StartDynamicLod()
        {
            if (_simConnectProvider != null)
            {
                // Attach in memory override for Dynamic LOD
                if (AppSettingData != null && AppSettingData.ApplicationSetting.DynamicLodSetting.IsEnabled)
                    _dynamicLodOrchestrator.Attach();

                _simConnectProvider.StartDynamicLod();
            }
        }

        private void StopDynamicLod()
        {
            // Detach in memory override for Dynamic LOD
            if (AppSettingData != null && AppSettingData.ApplicationSetting.DynamicLodSetting.IsEnabled)
                _dynamicLodOrchestrator.Detach();

            _simConnectProvider.StopDynamicLod();
        }
    }
}
