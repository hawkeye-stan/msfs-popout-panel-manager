using Newtonsoft.Json;
using System;
using System.ComponentModel;

namespace MSFSPopoutPanelManager.Model
{
    public class PanelConfig : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

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
        public bool IsCustomPopout { get { return PanelType == PanelType.CustomPopout; } }

        [JsonIgnore]
        public IntPtr PanelHandle { get; set; }
    }
}
