using System;
using System.Windows;

namespace MSFSPopoutPanelManager.Orchestration
{
    public class SharedStorage
    {
        public AppSettingData AppSettingData { get; set; } = new AppSettingData();

        public ProfileData ProfileData { get; set; } = new ProfileData();

        public FlightSimData FlightSimData { get; set; } = new FlightSimData();

        public IntPtr ApplicationHandle { get; set; }

        public Window ApplicationWindow { get; set; }
    }
}
