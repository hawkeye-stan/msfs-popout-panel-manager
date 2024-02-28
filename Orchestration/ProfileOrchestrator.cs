using MSFSPopoutPanelManager.DomainModel.Profile;
using System.Linq;

namespace MSFSPopoutPanelManager.Orchestration
{
    public class ProfileOrchestrator : BaseOrchestrator
    {
        public ProfileOrchestrator(SharedStorage sharedStorage) : base(sharedStorage)
        {
        }

        public void AddProfile(string profileName, UserProfile copiedProfile)
        {
            // Reset current profile
            ProfileData.ResetActiveProfile();

            if (copiedProfile == null)
                ProfileData.AddProfile(profileName);
            else
                ProfileData.AddProfile(profileName, copiedProfile);

            // Automatically bind aircraft
            var boundProfile = ProfileData.Profiles.FirstOrDefault(p => p.AircraftBindings.Any(a => a == FlightSimData.AircraftName));
            if (boundProfile == null && FlightSimData.HasAircraftName)
            {
                ProfileData.ActiveProfile.AircraftBindings.Add(FlightSimData.AircraftName);
                ProfileData.WriteProfiles();
                ProfileData.RefreshProfile();
            }
        }

        public void DeleteActiveProfile()
        {
            ProfileData.DeleteActiveProfile();
        }

        public void MoveToNextProfile()
        {
            // Reset current profile
            ProfileData.ResetActiveProfile();

            var newProfileIndex = ProfileData.Profiles.IndexOf(ProfileData.ActiveProfile) + 1;

            if (newProfileIndex >= ProfileData.Profiles.Count)
                newProfileIndex = 0;

            ProfileData.SetActiveProfile(newProfileIndex);
        }

        public void MoveToPreviousProfile()
        {
            // Reset current profile
            ProfileData.ResetActiveProfile();

            var newProfileIndex = ProfileData.Profiles.IndexOf(ProfileData.ActiveProfile) - 1;

            if (newProfileIndex < 0)
                newProfileIndex = ProfileData.Profiles.Count - 1;

            ProfileData.SetActiveProfile(newProfileIndex);
        }

        public void ChangeProfile(UserProfile profile)
        {
            if (ProfileData != null)
                ProfileData.SetActiveProfile(profile.Id);
        }

        public void AddProfileBinding(string bindingAircraft)
        {
            if (ProfileData.ActiveProfile != null && bindingAircraft != null)
                ProfileData.AddProfileBinding(bindingAircraft);
        }

        public void DeleteProfileBinding(string bindingAircraft)
        {
            if (ProfileData.ActiveProfile != null && bindingAircraft != null)
                ProfileData.DeleteProfileBinding(bindingAircraft);
        }

        public void AddPanel()
        {
            var panelConfig = new PanelConfig
            {
                PanelType = PanelType.CustomPopout,
                PanelSource =
                {
                    Color = PanelConfigColors.GetNextAvailableColor(ProfileData.ActiveProfile.PanelConfigs.ToList())
                }
            };
            ProfileData.ActiveProfile.PanelConfigs.Add(panelConfig);
        }
    }
}
