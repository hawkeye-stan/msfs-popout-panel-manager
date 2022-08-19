using MSFSPopoutPanelManager.Shared;
using MSFSPopoutPanelManager.SimConnectAgent;
using System;

namespace MSFSPopoutPanelManager.Orchestration
{
    public class FlightSimOrchestrator : ObservableObject
    {
        private SimConnectProvider _simConnectProvider;

        public FlightSimOrchestrator()
        {
            _simConnectProvider = new SimConnectProvider();
        }

        internal ProfileData ProfileData { get; set; }

        internal AppSettingData AppSettingData { get; set; }

        internal FlightSimData FlightSimData { get; set; }

        internal SimConnectProvider SimConnectProvider { get { return _simConnectProvider; } }

        public event EventHandler OnSimulatorStarted;
        public event EventHandler OnSimulatorStopped;
        public event EventHandler OnFlightStarted;
        public event EventHandler OnFlightStopped;
        public event EventHandler OnFlightStartedForAutoPopOut;

        public void StartSimConnectServer()
        {
            _simConnectProvider.OnConnected += (sender, e) =>
            {
                FlightSimData.IsSimulatorStarted = true;
                OnSimulatorStarted?.Invoke(this, null);
            };
            _simConnectProvider.OnDisconnected += (sender, e) =>
            {
                FlightSimData.IsSimulatorStarted = false;
                FlightSimData.ClearData();

                OnSimulatorStopped?.Invoke(this, null);
            };
            _simConnectProvider.OnSimConnectDataRefreshed += (sender, e) =>
            {
                var aircraftName = Convert.ToString(e.Find(d => d.PropName == "AircraftName").Value);
                aircraftName = String.IsNullOrEmpty(aircraftName) ? null : aircraftName;
                var electricalMasterBattery = Convert.ToBoolean(e.Find(d => d.PropName == "ElectricalMasterBattery").Value);
                var liveryName = Convert.ToString(e.Find(d => d.PropName == "Title").Value);

                if (electricalMasterBattery != FlightSimData.ElectricalMasterBatteryStatus)
                    FlightSimData.ElectricalMasterBatteryStatus = electricalMasterBattery;

                if (liveryName != FlightSimData.CurrentMsfsLiveryTitle)
                {
                    FlightSimData.CurrentMsfsLiveryTitle = liveryName;
                    ProfileData.MigrateLiveryToAircraftBinding(liveryName, aircraftName);
                }

                // Automatic switching of active profile when SimConnect active aircraft change
                if (FlightSimData.CurrentMsfsAircraft != aircraftName)
                {
                    FlightSimData.CurrentMsfsAircraft = aircraftName;
                    ProfileData.AutoSwitchProfile();
                }
            };
            _simConnectProvider.OnFlightStarted += HandleOnFlightStarted;
            _simConnectProvider.OnFlightStopped += HandleOnFlightStopped;
            _simConnectProvider.OnIsInCockpitChanged += (sender, e) => FlightSimData.IsInCockpit = e;
            _simConnectProvider.Start();
        }

        public void EndSimConnectServer(bool appExit)
        {
            _simConnectProvider.Stop(appExit);
        }

        public void TurnOnTrackIR()
        {
            if (AppSettingData.AppSetting.AutoDisableTrackIR)
                _simConnectProvider.TurnOnTrackIR();
        }

        public void TurnOffTrackIR()
        {
            if (AppSettingData.AppSetting.AutoDisableTrackIR)
                _simConnectProvider.TurnOffTrackIR();
        }

        public void TurnOnPower()
        {
            if (ProfileData.ActiveProfile != null)
                _simConnectProvider.TurnOnPower(ProfileData.ActiveProfile.PowerOnRequiredForColdStart);
        }

        public void TurnOnAvionics()
        {
            if (ProfileData.ActiveProfile != null)
                _simConnectProvider.TurnOnAvionics(ProfileData.ActiveProfile.PowerOnRequiredForColdStart);
        }

        public void TurnOffPower()
        {
            if (ProfileData.ActiveProfile != null)
                _simConnectProvider.TurnOffpower(ProfileData.ActiveProfile.PowerOnRequiredForColdStart);
        }

        public void TurnOffAvionics()
        {
            if (ProfileData.ActiveProfile != null)
                _simConnectProvider.TurnOffAvionics(ProfileData.ActiveProfile.PowerOnRequiredForColdStart);
        }

        private void HandleOnFlightStarted(object sender, EventArgs e)
        {
            OnFlightStarted?.Invoke(this, null);

            if (AppSettingData.AppSetting.AutoPopOutPanels)
                OnFlightStartedForAutoPopOut?.Invoke(this, null);
        }

        private void HandleOnFlightStopped(object sender, EventArgs e)
        {
            OnFlightStopped?.Invoke(this, null);
        }
    }
}
