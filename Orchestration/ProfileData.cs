using MSFSPopoutPanelManager.Shared;
using MSFSPopoutPanelManager.UserDataAgent;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

namespace MSFSPopoutPanelManager.Orchestration
{
    public class ProfileData : ObservableObject
    {
        public event PropertyChangedEventHandler ActiveProfileChanged;

        public ProfileData()
        {
            Profiles = new ObservableCollection<Profile>();
        }

        public ObservableCollection<Profile> Profiles { get; private set; }

        public FlightSimData FlightSimData { private get; set; }

        public AppSettingData AppSettingData { private get; set; }

        public int AddProfile(string profileName)
        {
            var newProfileId = ProfileManager.AddProfile(profileName, Profiles);
            UpdateActiveProfile(newProfileId);
            AppSettingData.AppSetting.LastUsedProfileId = newProfileId;
            return newProfileId;
        }

        public int AddProfile(string profileName, int copyFromProfileId)
        {
            var newProfileId = ProfileManager.AddProfile(profileName, copyFromProfileId, Profiles);
            UpdateActiveProfile(newProfileId);
            AppSettingData.AppSetting.LastUsedProfileId = newProfileId;
            return newProfileId;
        }

        public bool DeleteProfile(int profileId)
        {
            if (ActiveProfile == null)
                return false;

            return ProfileManager.DeleteProfile(profileId, Profiles);
        }

        public void AddProfileBinding(string planeTitle, int activeProfileId)
        {
            if (ActiveProfile == null)
                return;

            ProfileManager.AddProfileBinding(planeTitle, activeProfileId, Profiles);
            RefreshProfile();
        }

        public void DeleteProfileBinding(string planeTitle, int activeProfileId)
        {
            if (ActiveProfile == null)
                return;

            ProfileManager.DeleteProfileBinding(planeTitle, activeProfileId, Profiles);
            RefreshProfile();
        }

        public void ReadProfiles()
        {
            Profiles = new ObservableCollection<Profile>(ProfileManager.ReadProfiles());
        }

        public void WriteProfiles()
        {
            ProfileManager.WriteProfiles(Profiles);
        }

        public void UpdateActiveProfile(int profileId)
        {
            if (profileId == -1 && Profiles.Count > 0)
                ActiveProfile = Profiles.FirstOrDefault(p => p.ProfileId == Profiles[0].ProfileId);
            else if (profileId == -1 || Profiles.Count == 0)
                ActiveProfile = null;
            else
                ActiveProfile = Profiles.FirstOrDefault(p => p.ProfileId == profileId);

            // Set active profile flag, this is used only for MVVM binding
            Profiles.ToList().ForEach(p => p.IsActive = false);

            if (ActiveProfile != null)
            {
                ActiveProfile.IsActive = true;
                AppSettingData.AppSetting.LastUsedProfileId = ActiveProfile.ProfileId;
            }
            else
            {
                AppSettingData.AppSetting.LastUsedProfileId = -1;
            }

            ActiveProfileChanged?.Invoke(this, null);
        }

        public Profile ActiveProfile { get; private set; }

        public bool HasActiveProfile { get { return ActiveProfile != null; } }

        public bool IsAircraftBoundToProfile
        {
            get
            {
                if (ActiveProfile == null)
                    return false;

                return ActiveProfile.BindingAircraftLiveries.Any(p => p == FlightSimData.CurrentMsfsPlaneTitle);
            }
        }

        public bool IsAllowedDeleteAircraftBinding
        {
            get
            {
                if (ActiveProfile == null || !FlightSimData.HasCurrentMsfsPlaneTitle)
                    return false;

                var uProfile = Profiles.FirstOrDefault(u => u.BindingAircraftLiveries.Any(p => p == FlightSimData.CurrentMsfsPlaneTitle));
                if (uProfile != null && uProfile.ProfileId != ActiveProfile.ProfileId)
                    return false;

                return ActiveProfile.BindingAircraftLiveries.Any(p => p == FlightSimData.CurrentMsfsPlaneTitle);
            }
        }

        public bool IsAllowedAddAircraftBinding
        {
            get
            {
                if (ActiveProfile == null || !FlightSimData.HasCurrentMsfsPlaneTitle)
                    return false;

                var uProfile = Profiles.FirstOrDefault(u => u.BindingAircraftLiveries.Any(p => p == FlightSimData.CurrentMsfsPlaneTitle));
                if (uProfile != null && uProfile.ProfileId != ActiveProfile.ProfileId)
                    return false;

                return !ActiveProfile.BindingAircraftLiveries.Any(p => p == FlightSimData.CurrentMsfsPlaneTitle);
            }
        }

        public void RefreshProfile()
        {
            int currentProfileId;
            if (ActiveProfile == null)
                currentProfileId = -1;
            else
                currentProfileId = ActiveProfile.ProfileId;

            ActiveProfile = null;
            UpdateActiveProfile(currentProfileId);
        }

        public void AutoSwitchProfile(string activeAircraftTitle)
        {
            // Automatic switching of active profile when SimConnect active aircraft livery changes
            if (Profiles != null)
            {
                var matchedProfile = Profiles.FirstOrDefault(p => p.BindingAircraftLiveries.Any(t => t == activeAircraftTitle));
                if (matchedProfile != null)
                    UpdateActiveProfile(matchedProfile.ProfileId);
            }
        }
    }
}
