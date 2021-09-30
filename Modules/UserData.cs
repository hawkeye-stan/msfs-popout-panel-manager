using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace MSFSPopoutPanelManager
{
    public class UserData
    {
        public UserData()
        {
            Profiles = new List<UserPlaneProfile>();
            DefaultProfileId = 1;
        }

        public List<UserPlaneProfile> Profiles { get; set; }

        public int DefaultProfileId { get; set; }
    }

    public class UserPlaneProfile
    {
        public UserPlaneProfile()
        {
            PanelSourceCoordinates = new List<PanelSourceCoordinate>();
            PanelSettings = new PanelSettings();
        }

        public int ProfileId { get; set; }

        public PanelSettings PanelSettings { get; set; }

        public List<PanelSourceCoordinate> PanelSourceCoordinates;
    }

    public class PanelSourceCoordinate
    {
        public int PanelIndex { get; set; }

        public int X { get; set; }

        public int Y { get; set; }
    }

    public class PanelSettings
    {
        public PanelSettings()
        {
            PanelDestinationList = new List<PanelDestinationInfo>();
            AlwaysOnTop = false;
            HidePanelTitleBar = false;
        }

        public List<PanelDestinationInfo> PanelDestinationList { get; set; }

        public bool AlwaysOnTop { get; set; }

        public bool HidePanelTitleBar { get; set; }
    }

    public class PanelDestinationInfo
    {
        public string PanelName { get; set; }

        [JsonIgnore]
        public IntPtr PanelHandle { get; set; }

        public int Top { get; set; }

        public int Left { get; set; }

        public int Width { get; set; }

        public int Height { get; set; }
    }
}
