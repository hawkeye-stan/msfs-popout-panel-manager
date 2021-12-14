using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace MSFSPopoutPanelManager.Shared
{
    public class UserProfileData
    {
        public UserProfileData()
        {
            PanelSourceCoordinates = new List<PanelSourceCoordinate>();
            PanelConfigs = new List<PanelConfig>();
        }

        public int ProfileId { get; set; }

        public string ProfileName { get; set; }

        public bool IsDefaultProfile { get; set; }

        public List<PanelSourceCoordinate> PanelSourceCoordinates;

        public List<PanelConfig> PanelConfigs { get; set; }
    }

    public class PanelSourceCoordinate
    {
        public int PanelIndex { get; set; }

        public int X { get; set; }

        public int Y { get; set; }
    }

    public class PanelConfig
    {
        public int PanelIndex { get; set; }

        public string PanelName { get; set; }

        public PanelType PanelType { get; set; }

        public int Top { get; set; }

        public int Left { get; set; }

        public int Width { get; set; }

        public int Height { get; set; }

        public bool AlwaysOnTop { get; set; }

        public bool HideTitlebar { get; set; }

        [JsonIgnore]
        public IntPtr PanelHandle { get; set; }
    }
}
