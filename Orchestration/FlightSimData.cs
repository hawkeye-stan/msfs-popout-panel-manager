using MSFSPopoutPanelManager.Shared;
using System;
using System.ComponentModel;

namespace MSFSPopoutPanelManager.Orchestration
{
    public class FlightSimData : ObservableObject
    {
        public event PropertyChangedEventHandler CurrentMsfsAircraftChanged;
        public event PropertyChangedEventHandler CurrentMsfsLiveryTitleChanged;

        public string CurrentMsfsAircraft { get; set; }

        public string CurrentMsfsLiveryTitle { get; set; }

        public bool HasCurrentMsfsAircraft
        {
            get { return !String.IsNullOrEmpty(CurrentMsfsAircraft); }
        }

        public bool ElectricalMasterBatteryStatus { get; set; }

        public bool IsSimulatorStarted { get; set; }

        public bool IsInCockpit { get; set; }

        public new void OnPropertyChanged(string propertyName, object oldValue, object newValue)
        {
            if (oldValue != newValue)
            {
                base.OnPropertyChanged(propertyName, oldValue, newValue);

                if (propertyName == "CurrentMsfsAircraft")
                    CurrentMsfsAircraftChanged?.Invoke(this, null);

                if (propertyName == "CurrentMsfsLiveryTitle")
                    CurrentMsfsLiveryTitleChanged?.Invoke(this, null);
            }
        }

        public void ClearData()
        {
            CurrentMsfsAircraft = null;
            CurrentMsfsLiveryTitle = null;
            ElectricalMasterBatteryStatus = false;
        }
    }
}
