using MSFSPopoutPanelManager.Shared;
using System;
using System.ComponentModel;

namespace MSFSPopoutPanelManager.Orchestration
{
    public class FlightSimData : ObservableObject
    {
        public event PropertyChangedEventHandler CurrentMsfsPlaneTitleChanged;

        public string CurrentMsfsPlaneTitle { get; set; }

        public bool HasCurrentMsfsPlaneTitle
        {
            get { return !String.IsNullOrEmpty(CurrentMsfsPlaneTitle); }
        }

        public bool ElectricalMasterBatteryStatus { get; set; }

        public bool IsSimulatorStarted { get; set; }

        public bool IsEnteredFlight { get; set; }

        public new void OnPropertyChanged(string propertyName, object oldValue, object newValue)
        {
            if (oldValue != newValue)
            {
                if (propertyName == "CurrentMsfsPlaneTitle")
                    CurrentMsfsPlaneTitleChanged?.Invoke(this, null);

                base.OnPropertyChanged(propertyName, oldValue, newValue);
            }
        }

        public void ClearData()
        {
            CurrentMsfsPlaneTitle = null;
            ElectricalMasterBatteryStatus = false;
            IsEnteredFlight = false;
        }
    }
}
