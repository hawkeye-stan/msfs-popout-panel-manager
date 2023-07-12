using MSFSPopoutPanelManager.DomainModel.SimConnect;
using MSFSPopoutPanelManager.Shared;
using System;
using System.ComponentModel;

namespace MSFSPopoutPanelManager.Orchestration
{
    public class FlightSimData : ObservableObject
    {
        public FlightSimData()
        {
            Setup();
            InitializeChildPropertyChangeBinding();
        }

        public string AircraftName { get; set; }

        public bool HasAircraftName
        {
            get { return !String.IsNullOrEmpty(AircraftName); }
        }

        public bool ElectricalMasterBatteryStatus { get; set; }

        public bool AvionicsMasterSwitchStatus { get; set; }

        public bool TrackIRStatus { get; set; }

        public int CameraState { get; set; }

        public bool PlaneInParkingSpot { get; set; }

        public bool IsSimulatorStarted { get; set; }

        public bool IsInCockpit { get; set; }

        public IHudBarData HudBarData { get; set; }

        [IgnorePropertyChanged]
        internal ProfileData ProfileDataRef { get; set; }

        public new void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(this, new PropertyChangedEventArgs(e.PropertyName));

            // Automatic switching of active profile when SimConnect active aircraft change
            if (e.PropertyName == "AircraftName")
                ProfileDataRef.AutoSwitchProfile();
        }

        public void Reset()
        {
            Setup();
        }

        private void Setup()
        {
            AircraftName = null;
            ElectricalMasterBatteryStatus = false;
            AvionicsMasterSwitchStatus = false;
            TrackIRStatus = false;
            IsInCockpit = false;
            PlaneInParkingSpot = false;
            CameraState = -1;
            IsSimulatorStarted = false;
        }
    }
}
