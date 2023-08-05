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
    public class FlightSimOrchestrator : ObservableObject
    {
        private const int MSFS_GAME_EXIT_DETECTION_INTERVAL = 3000;
        private System.Timers.Timer _msfsGameExitDetectionTimer;
        private SimConnectProvider _simConnectProvider;

        private ProfileData _profileData;
        private AppSettingData _appSettingData;
        private FlightSimData _flightSimData;
        private bool _isTurnedOnPower;
        private bool _isTurnedOnAvionics;

        public FlightSimOrchestrator(ProfileData profileData, AppSettingData appSettingData, FlightSimData flightSimData)
        {
            _profileData = profileData;
            _appSettingData = appSettingData;
            _flightSimData = flightSimData;

            _simConnectProvider = new SimConnectProvider();
        }

        internal PanelPopOutOrchestrator PanelPopOutOrchestrator { get; set; }

        internal PanelConfigurationOrchestrator PanelConfigurationOrchestrator { get; set; }

        internal SimConnectProvider SimConnectProvider { get { return _simConnectProvider; } }

        public event EventHandler OnSimulatorExited;

        public void StartSimConnectServer()
        {
            if (_simConnectProvider == null)
                _simConnectProvider = new SimConnectProvider();

            _simConnectProvider.OnConnected += (sender, e) =>
            {
                _flightSimData.IsSimConnectActive = true;
                _flightSimData.IsSimulatorStarted = true;
                WindowProcessManager.GetSimulatorProcess();     // refresh simulator process
                DetectMsfsExit();
            };

            _simConnectProvider.OnDisconnected += (sender, e) =>
            {
                _flightSimData.IsSimConnectActive = false;
                WindowProcessManager.GetSimulatorProcess();     // refresh simulator process
                _flightSimData.Reset();
            };

            _simConnectProvider.OnException += (sender, e) =>
            {
                _flightSimData.IsSimConnectActive = false;
            };

            _simConnectProvider.OnSimConnectDataRequiredRefreshed += (sender, e) =>
            {
                var electricalMasterBattery = Convert.ToBoolean(e.Find(d => d.PropertyName == SimDataDefinitions.PropName.ElectricalMasterBattery).Value);
                if (electricalMasterBattery != _flightSimData.ElectricalMasterBatteryStatus)
                    _flightSimData.ElectricalMasterBatteryStatus = electricalMasterBattery;

                var avionicsMasterSwitch = Convert.ToBoolean(e.Find(d => d.PropertyName == SimDataDefinitions.PropName.AvionicsMasterSwitch).Value);
                if (avionicsMasterSwitch != _flightSimData.AvionicsMasterSwitchStatus)
                    _flightSimData.AvionicsMasterSwitchStatus = avionicsMasterSwitch;

                var trackIR = Convert.ToBoolean(e.Find(d => d.PropertyName == SimDataDefinitions.PropName.TrackIREnable).Value);
                if (trackIR != _flightSimData.TrackIRStatus)
                    _flightSimData.TrackIRStatus = trackIR;

                var cockpitCameraZoom = Convert.ToInt32(e.Find(d => d.PropertyName == SimDataDefinitions.PropName.CockpitCameraZoom).Value);
                if (cockpitCameraZoom != _flightSimData.CockpitCameraZoom)
                    _flightSimData.CockpitCameraZoom = cockpitCameraZoom;
            };

            _simConnectProvider.OnSimConnectDataHudBarRefreshed += (sender, e) =>
            {
                if (_profileData.ActiveProfile.ProfileSetting.HudBarConfig.IsEnabled)
                    MapHudBarSimConnectData(e);
            };

            _simConnectProvider.OnActiveAircraftChanged += (sender, e) =>
            {
                var aircraftName = String.IsNullOrEmpty(e) ? null : e;
                if (_flightSimData.AircraftName != aircraftName)
                {
                    _flightSimData.AircraftName = aircraftName;
                    _profileData.RefreshProfile();
                }
            };

            _simConnectProvider.OnFlightStarted += HandleOnFlightStarted;
            _simConnectProvider.OnFlightStopped += HandleOnFlightStopped;
            _simConnectProvider.OnIsInCockpitChanged += (sender, e) => _flightSimData.IsInCockpit = e;
            _simConnectProvider.Start();
        }

        public void EndSimConnectServer(bool appExit)
        {
            _simConnectProvider.Stop(appExit);
            _simConnectProvider = null;
        }

        public void TurnOnTrackIR(bool writeMessage = true)
        {
            if (_simConnectProvider == null)
                return;

            if (!_appSettingData.ApplicationSetting.TrackIRSetting.AutoDisableTrackIR)
                return;

            StatusMessageWriter.WriteMessage("Turning on TrackIR", StatusMessageType.Info);

            int count = 0;
            do
            {
                _simConnectProvider.TurnOnTrackIR();
                Thread.Sleep(500);
                count++;
            }
            while (!_flightSimData.TrackIRStatus && count < 5);

            if (_flightSimData.TrackIRStatus)
                StatusMessageWriter.WriteOkStatusMessage();
            else
                StatusMessageWriter.WriteFailureStatusMessage();
        }

        public void TurnOffTrackIR(bool writeMessage = true)
        {
            if (_simConnectProvider == null)
                return;

            if (!_appSettingData.ApplicationSetting.TrackIRSetting.AutoDisableTrackIR)
                return;

            StatusMessageWriter.WriteMessage("Turning off TrackIR", StatusMessageType.Info);

            int count = 0;
            do
            {
                _simConnectProvider.TurnOffTrackIR();
                Thread.Sleep(500);
                count++;
            }
            while (_flightSimData.TrackIRStatus && count < 5);

            if (!writeMessage)
                return;

            if (!_flightSimData.TrackIRStatus)
                StatusMessageWriter.WriteOkStatusMessage();
            else
                StatusMessageWriter.WriteFailureStatusMessage();
        }

        public void TurnOnPower()
        {
            if (_simConnectProvider == null)
                return;

            if (_profileData.ActiveProfile == null || _flightSimData.ElectricalMasterBatteryStatus)
                return;

            _isTurnedOnPower = true;
            StatusMessageWriter.WriteMessage("Turning on battery", StatusMessageType.Info);

            int count = 0;
            do
            {
                _simConnectProvider.TurnOnPower(_profileData.ActiveProfile.ProfileSetting.PowerOnRequiredForColdStart);
                Thread.Sleep(500);
                count++;
            }
            while (!_flightSimData.ElectricalMasterBatteryStatus && count < 10);

            if (_flightSimData.ElectricalMasterBatteryStatus)
                StatusMessageWriter.WriteOkStatusMessage();
            else
                StatusMessageWriter.WriteFailureStatusMessage();
        }

        public void TurnOffPower()
        {
            if (_simConnectProvider == null)
                return;

            if (_profileData.ActiveProfile == null || !_isTurnedOnPower)
                return;

            StatusMessageWriter.WriteMessage("Turning off battery", StatusMessageType.Info);

            int count = 0;
            do
            {
                _simConnectProvider.TurnOffPower(_profileData.ActiveProfile.ProfileSetting.PowerOnRequiredForColdStart);
                Thread.Sleep(500);
                count++;
            }
            while (_flightSimData.ElectricalMasterBatteryStatus && count < 10);

            if (!_flightSimData.ElectricalMasterBatteryStatus)
                StatusMessageWriter.WriteOkStatusMessage();
            else
                StatusMessageWriter.WriteFailureStatusMessage();

            _isTurnedOnPower = false;
        }

        public void TurnOnAvionics()
        {
            if (_simConnectProvider == null)
                return;

            if (_profileData.ActiveProfile == null || _flightSimData.AvionicsMasterSwitchStatus)
                return;

            _isTurnedOnAvionics = true;

            StatusMessageWriter.WriteMessage("Turning on avionics", StatusMessageType.Info);

            int count = 0;
            do
            {
                _simConnectProvider.TurnOnAvionics(_profileData.ActiveProfile.ProfileSetting.PowerOnRequiredForColdStart);
                Thread.Sleep(500);
                count++;
            }
            while (!_flightSimData.AvionicsMasterSwitchStatus && count < 10);

            if (_flightSimData.AvionicsMasterSwitchStatus)
                StatusMessageWriter.WriteOkStatusMessage();
            else
                StatusMessageWriter.WriteFailureStatusMessage();
        }

        public void TurnOffAvionics()
        {
            if (_simConnectProvider == null)
                return;

            if (_profileData.ActiveProfile == null || !_isTurnedOnAvionics)
                return;

            StatusMessageWriter.WriteMessage("Turning off avionics", StatusMessageType.Info);

            int count = 0;
            do
            {
                _simConnectProvider.TurnOffAvionics(_profileData.ActiveProfile.ProfileSetting.PowerOnRequiredForColdStart);
                Thread.Sleep(500);
                count++;
            }
            while (_flightSimData.AvionicsMasterSwitchStatus && count < 10);

            if (!_flightSimData.AvionicsMasterSwitchStatus)
                StatusMessageWriter.WriteOkStatusMessage();
            else
                StatusMessageWriter.WriteFailureStatusMessage();

            _isTurnedOnAvionics = false;
        }

        public void TurnOnActivePause()
        {
            if (_simConnectProvider == null)
                return;

            if (!_appSettingData.ApplicationSetting.PopOutSetting.AutoActivePause)
                return;

            StatusMessageWriter.WriteMessage("Turning on active pause", StatusMessageType.Info);
            _simConnectProvider.TurnOnActivePause();
            StatusMessageWriter.WriteOkStatusMessage();
        }

        public void TurnOffActivePause()
        {
            if (_simConnectProvider == null)
                return;

            if (!_appSettingData.ApplicationSetting.PopOutSetting.AutoActivePause)
                return;

            StatusMessageWriter.WriteMessage("Turning off active pause", StatusMessageType.Info);
            _simConnectProvider.TurnOffActivePause();
            StatusMessageWriter.WriteOkStatusMessage();

        }

        public void IncreaseSimRate()
        {
            if (_simConnectProvider == null)
                return;

            if (_flightSimData.HudBarData.SimRate == 16)
                return;

            _simConnectProvider.IncreaseSimRate();
        }

        public void DecreaseSimRate()
        {
            if (_simConnectProvider == null)
                return;

            if (_flightSimData.HudBarData.SimRate == 0.25)
                return;

            _simConnectProvider.DecreaseSimRate();
        }

        public void SetCockpitCameraZoomLevel(int zoomLevel)
        {
            _simConnectProvider.SetCockpitCameraZoomLevel(zoomLevel);
        }

        public void SetHudBarConfig()
        {
            if (_simConnectProvider == null)
                return;

            var hudBarType = _profileData.ActiveProfile.ProfileSetting.HudBarConfig.HudBarType;
            switch (hudBarType)
            {
                case HudBarType.PMDG_737:
                    _flightSimData.HudBarData = new HudBarData737();
                    break;
                case HudBarType.Generic_Aircraft:
                default:
                    _flightSimData.HudBarData = new HudBarDataGeneric();
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
            if (_appSettingData.ApplicationSetting.AutoPopOutSetting.IsEnabled)
                PanelPopOutOrchestrator.AutoPopOut();
        }

        private void HandleOnFlightStopped(object sender, EventArgs e)
        {
            _profileData.ResetActiveProfile();
            PanelConfigurationOrchestrator.EndConfiguration();
            PanelConfigurationOrchestrator.EndTouchHook();

            WindowActionManager.CloseAllPopOuts();

            if (_flightSimData.HudBarData != null)
                _flightSimData.HudBarData.Clear();
        }

        private void DetectMsfsExit()
        {
            _msfsGameExitDetectionTimer = new System.Timers.Timer();
            _msfsGameExitDetectionTimer.Interval = MSFS_GAME_EXIT_DETECTION_INTERVAL;
            _msfsGameExitDetectionTimer.Enabled = true;
            _msfsGameExitDetectionTimer.Elapsed += (source, e) =>
            {
                WindowProcessManager.GetSimulatorProcess();
                if (WindowProcessManager.SimulatorProcess == null)
                {
                    if (_appSettingData.ApplicationSetting.GeneralSetting.AutoClose)
                        OnSimulatorExited?.Invoke(this, null);
                    else
                    {
                        _flightSimData.Reset();
                        _simConnectProvider.StopAndReconnect();
                    }
                }
            };
        }

        private void MapHudBarSimConnectData(List<SimDataItem> simData)
        {
            double newValue;

            if (CompareSimConnectData(simData, SimDataDefinitions.PropName.ElevatorTrim, _flightSimData.HudBarData.ElevatorTrim, out newValue))
                _flightSimData.HudBarData.ElevatorTrim = newValue;

            if (CompareSimConnectData(simData, SimDataDefinitions.PropName.AileronTrim, _flightSimData.HudBarData.AileronTrim, out newValue))
                _flightSimData.HudBarData.AileronTrim = newValue;

            if (CompareSimConnectData(simData, SimDataDefinitions.PropName.RudderTrim, _flightSimData.HudBarData.RudderTrim, out newValue))
                _flightSimData.HudBarData.RudderTrim = newValue;

            if (CompareSimConnectData(simData, SimDataDefinitions.PropName.ParkingBrake, _flightSimData.HudBarData.ParkingBrake, out newValue))
                _flightSimData.HudBarData.ParkingBrake = newValue;

            if (CompareSimConnectData(simData, SimDataDefinitions.PropName.Flap, _flightSimData.HudBarData.Flap, out newValue))
                _flightSimData.HudBarData.Flap = newValue;

            if (CompareSimConnectData(simData, SimDataDefinitions.PropName.GearLeft, _flightSimData.HudBarData.GearLeft, out newValue))
                _flightSimData.HudBarData.GearLeft = newValue;

            if (CompareSimConnectData(simData, SimDataDefinitions.PropName.GearCenter, _flightSimData.HudBarData.GearCenter, out newValue))
                _flightSimData.HudBarData.GearCenter = newValue;

            if (CompareSimConnectData(simData, SimDataDefinitions.PropName.GearRight, _flightSimData.HudBarData.GearRight, out newValue))
                _flightSimData.HudBarData.GearRight = newValue;

            if (CompareSimConnectData(simData, SimDataDefinitions.PropName.SimRate, _flightSimData.HudBarData.SimRate, out newValue))
                _flightSimData.HudBarData.SimRate = newValue;
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
            if (value != source)
            {
                newValue = value;
                return true;
            }

            newValue = 0;
            return false;
        }
    }
}
