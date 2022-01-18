using System.Linq;
using System.ComponentModel;

namespace MSFSPopoutPanelManager.Shared
{
    public class DataStore : INotifyPropertyChanged
    {
        private int _activeProfileId;

        public event PropertyChangedEventHandler PropertyChanged;

        public DataStore()
        {
            _activeProfileId = -1;
            ActiveUserProfile = null;
            ActiveProfilePanelCoordinates = new BindingList<PanelSourceCoordinate>();
            PanelConfigs = new BindingList<PanelConfig>();

        }

        public BindingList<UserProfileData> UserProfiles { get; set; }

        public BindingList<PanelSourceCoordinate> ActiveProfilePanelCoordinates { get; set; }

        public BindingList<PanelConfig> PanelConfigs { get; set; }

        public UserProfileData ActiveUserProfile { get; set; }

        public int ActiveUserProfileId
        {
            get
            {
                return _activeProfileId;
            }
            set
            {
                _activeProfileId = value;

                if(value == -1)
                {
                    ActiveUserProfile = null;
                    ActiveProfilePanelCoordinates.Clear();
                }
                else
                {
                    ActiveUserProfile = UserProfiles.ToList().Find(x => x.ProfileId == value);
                    ActiveProfilePanelCoordinates.Clear();
                    ActiveUserProfile.PanelSourceCoordinates.ForEach(c => ActiveProfilePanelCoordinates.Add(c));
                }
            }
        }
    }
}
