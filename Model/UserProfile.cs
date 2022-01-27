using Newtonsoft.Json;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace MSFSPopoutPanelManager.Model
{
    public class UserProfile : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public UserProfile()
        {
            PanelSourceCoordinates = new ObservableCollection<PanelSourceCoordinate>();
            PanelConfigs = new ObservableCollection<PanelConfig>();
            IsLocked = false;
        }

        public int ProfileId { get; set; }

        public string ProfileName { get; set; }

        public bool IsDefaultProfile { get; set; }

        public string BindingPlaneTitle { get; set; }

        public bool IsLocked { get; set; }

        public ObservableCollection<PanelSourceCoordinate> PanelSourceCoordinates;

        public ObservableCollection<PanelConfig> PanelConfigs { get; set; }

        public bool PowerOnRequiredForColdStart { get; set; }

        public void Reset()
        {
            PanelSourceCoordinates.Clear();
            PanelConfigs.Clear();
            IsLocked = false;
        }

        [JsonIgnore]
        public bool IsActive { get; set; }

        [JsonIgnore]
        public bool HasBindingPlaneTitle
        {
            get { return !string.IsNullOrEmpty(BindingPlaneTitle); }
        }
    }    
}
