using Newtonsoft.Json;
using System;
using System.ComponentModel;

namespace MSFSPopoutPanelManager.Model
{
    public class PanelSourceCoordinate : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public int PanelIndex { get; set; }

        public int X { get; set; }

        public int Y { get; set; }

        [JsonIgnore]
        public IntPtr PanelHandle { get; set; }
    }
}
