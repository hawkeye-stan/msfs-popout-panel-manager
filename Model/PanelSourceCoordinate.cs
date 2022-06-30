using CommunityToolkit.Mvvm.ComponentModel;
using Newtonsoft.Json;
using System;

namespace MSFSPopoutPanelManager.Model
{
    public class PanelSourceCoordinate : ObservableObject
    {
        public int PanelIndex { get; set; }

        public int X { get; set; }

        public int Y { get; set; }

        [JsonIgnore]
        public IntPtr PanelHandle { get; set; }
    }
}
