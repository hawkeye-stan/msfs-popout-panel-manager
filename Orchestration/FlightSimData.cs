using MSFSPopoutPanelManager.Shared;
using System;
using System.ComponentModel;

namespace MSFSPopoutPanelManager.Orchestration
{
    public class FlightSimData : ObservableObject
    {
        public event PropertyChangedEventHandler CurrentMsfsAircraftChanged;

        public string CurrentMsfsAircraft { get; set; }

        public string CurrentMsfsLiveryTitle { get; set; }

        public bool HasCurrentMsfsAircraft
        {
            get { return !String.IsNullOrEmpty(CurrentMsfsAircraft); }
        }

        public bool ElectricalMasterBatteryStatus { get; set; }

        public bool IsSimulatorStarted { get; set; }

        public bool IsEnteredFlight { get; set; }

        public new void OnPropertyChanged(string propertyName, object oldValue, object newValue)
        {
            if (oldValue != newValue)
            {
                if (propertyName == "CurrentMsfsAircraft")
                    CurrentMsfsAircraftChanged?.Invoke(this, null);

                base.OnPropertyChanged(propertyName, oldValue, newValue);
            }
        }

        public void ClearData()
        {
            CurrentMsfsAircraft = null;
            CurrentMsfsLiveryTitle = null;
            ElectricalMasterBatteryStatus = false;
            IsEnteredFlight = false;
        }
    }
}
