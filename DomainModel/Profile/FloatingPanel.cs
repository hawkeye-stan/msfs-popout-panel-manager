using MSFSPopoutPanelManager.Shared;
using Newtonsoft.Json;

namespace MSFSPopoutPanelManager.DomainModel.Profile
{
    public class FloatingPanel : ObservableObject
    {
        public bool IsEnabled { get; set; }

        public string Binding { get; set; }

        [JsonIgnore]
        public bool IsDetectingKeystroke { get; set; }
    }
}
