using MSFSPopoutPanelManager.DomainModel.SimConnect;
using MSFSPopoutPanelManager.Shared;
using System;
using System.ComponentModel;
using MSFSPopoutPanelManager.SimConnectAgent;

namespace MSFSPopoutPanelManager.Orchestration
{
    public class FlightSimData : ObservableObject
    {
        public FlightSimData()
        {
            Setup();
            InitializeChildPropertyChangeBinding();
            IsSimConnectActive = false;
        }

        public event EventHandler OnAltAboveGroundChanged;

        public bool IsSimConnectActive { get; set; }

        public string AircraftName { get; set; }

        public bool HasAircraftName => !String.IsNullOrEmpty(AircraftName);

        public bool ElectricalMasterBatteryStatus { get; set; }

        public bool AvionicsMasterSwitchStatus { get; set; }

        public bool TrackIRStatus { get; set; }

        public CameraState CameraState { get; set; }

        public int CockpitCameraZoom { get; set; }

        public int CameraViewTypeAndIndex0 { get; set; }

        public int CameraViewTypeAndIndex1 { get; set; }

        public int CameraViewTypeAndIndex1Max { get; set; }

        public int CameraViewTypeAndIndex2Max { get; set; }

        public bool PlaneInParkingSpot { get; set; }

        public int PlaneAltAboveGround { get; set; }

        public bool IsSimulatorStarted { get; set; }

        public bool IsSimConnectDataReceived { get; set; }

        public bool IsInCockpit { get; set; }

        public bool IsFlightStarted { get; set; }

        public IHudBarData HudBarData { get; set; }

        [IgnorePropertyChanged]
        internal ProfileData ProfileDataRef { get; set; }

        public new void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(this, new PropertyChangedEventArgs(e.PropertyName));

            // Automatic switching of active profile when SimConnect active aircraft change
            if (e.PropertyName == "AircraftName")
                ProfileDataRef.AutoSwitchProfile();

            if(e.PropertyName == "PlaneAltAboveGround")
                OnAltAboveGroundChanged?.Invoke(this, EventArgs.Empty);
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
            CameraState = CameraState.Unknown;
            IsSimulatorStarted = false;
            PlaneAltAboveGround = 0;
            CameraViewTypeAndIndex1Max = 0;
            CameraViewTypeAndIndex2Max = 0;
        }
    }
}
