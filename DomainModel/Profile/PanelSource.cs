using MSFSPopoutPanelManager.Shared;
using Newtonsoft.Json;
using System;

namespace MSFSPopoutPanelManager.DomainModel.Profile
{
    public class PanelSource : ObservableObject
    {
        public int? X { get; set; }

        public int? Y { get; set; }

        public string Color { get; set; }

        [JsonIgnore]
        public IntPtr PanelHandle { get; set; }
    }
}
