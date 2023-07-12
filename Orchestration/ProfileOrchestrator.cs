using MSFSPopoutPanelManager.DomainModel.Profile;
using System.Linq;

namespace MSFSPopoutPanelManager.Orchestration
{
    public class ProfileOrchestrator
    {
        private ProfileData _profileData;
        private FlightSimData _flightSimData;

        public ProfileOrchestrator(ProfileData profileData, FlightSimData flightSimData)
        {
            _profileData = profileData;
            _flightSimData = flightSimData;
        }

        public void AddProfile(string profileName, UserProfile copiedProfile)
        {
            // Reset current profile
            _profileData.ResetActiveProfile();

            if (copiedProfile == null)
                _profileData.AddProfile(profileName);
            else
                _profileData.AddProfile(profileName, copiedProfile);

            // Automatically bind aircraft
            var boundProfile = _profileData.Profiles.FirstOrDefault(p => p.AircraftBindings.Any(p => p == _flightSimData.AircraftName));
            if (boundProfile == null && _flightSimData.HasAircraftName)
            {
                _profileData.ActiveProfile.AircraftBindings.Add(_flightSimData.AircraftName);
                _profileData.WriteProfiles();
                _profileData.RefreshProfile();
            }
        }

        public void DeleteActiveProfile()
        {
            _profileData.DeleteActiveProfile();
        }

        public void MoveToNextProfile()
        {
            // Reset current profile
            _profileData.ResetActiveProfile();

            var newProfileIndex = _profileData.Profiles.IndexOf(_profileData.ActiveProfile) + 1;

            if (newProfileIndex >= _profileData.Profiles.Count)
                newProfileIndex = 0;

            _profileData.SetActiveProfile(newProfileIndex);
        }

        public void MoveToPreviousProfile()
        {
            // Reset current profile
            _profileData.ResetActiveProfile();

            var newProfileIndex = _profileData.Profiles.IndexOf(_profileData.ActiveProfile) - 1;

            if (newProfileIndex < 0)
                newProfileIndex = _profileData.Profiles.Count - 1;

            _profileData.SetActiveProfile(newProfileIndex);
        }

        public void ChangeProfile(UserProfile profile)
        {
            if (_profileData != null)
                _profileData.SetActiveProfile(profile.Id);
        }

        public void AddProfileBinding(string bindingAircraft)
        {
            if (_profileData.ActiveProfile != null && bindingAircraft != null)
                _profileData.AddProfileBinding(bindingAircraft);
        }

        public void DeleteProfileBinding(string bindingAircraft)
        {
            if (_profileData.ActiveProfile != null && bindingAircraft != null)
                _profileData.DeleteProfileBinding(bindingAircraft);
        }

        public void AddPanel()
        {
            var panelConfig = new PanelConfig();
            panelConfig.PanelType = PanelType.CustomPopout;
            panelConfig.PanelSource.Color = PanelConfigColors.GetNextAvailableColor(_profileData.ActiveProfile.PanelConfigs.ToList());
            _profileData.ActiveProfile.PanelConfigs.Add(panelConfig);
        }
    }
}
