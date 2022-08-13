using MSFSPopoutPanelManager.Shared;
using System.Linq;

namespace MSFSPopoutPanelManager.Orchestration
{
    public class ProfileOrchestrator : ObservableObject
    {
        internal ProfileData ProfileData { get; set; }

        internal FlightSimData FlightSimData { get; set; }

        public void AddProfile(string profileName, int copyProfileId)
        {
            if (copyProfileId == -1)
                ProfileData.AddProfile(profileName);
            else
                ProfileData.AddProfile(profileName, copyProfileId);

            // Automatically bind aircraft
            var boundProfile = ProfileData.Profiles.FirstOrDefault(p => p.BindingAircrafts.Any(p => p == FlightSimData.CurrentMsfsAircraft));
            if (boundProfile == null && FlightSimData.HasCurrentMsfsAircraft)
            {
                ProfileData.ActiveProfile.BindingAircrafts.Add(FlightSimData.CurrentMsfsAircraft);
                ProfileData.WriteProfiles();
                ProfileData.RefreshProfile();
            }
        }

        public void DeleteActiveProfile()
        {
            if (ProfileData.ActiveProfile != null)
                ProfileData.DeleteProfile(ProfileData.ActiveProfile.ProfileId);
        }

        public void ChangeProfile(int profileId)
        {
            if (ProfileData != null)
                ProfileData.UpdateActiveProfile(profileId);
        }

        public void AddProfileBinding(string bindingAircraft)
        {
            if (ProfileData.ActiveProfile != null && bindingAircraft != null)
                ProfileData.AddProfileBinding(bindingAircraft, ProfileData.ActiveProfile.ProfileId);
        }

        public void DeleteProfileBinding(string bindingAircraft)
        {
            if (ProfileData.ActiveProfile != null && bindingAircraft != null)
                ProfileData.DeleteProfileBinding(bindingAircraft, ProfileData.ActiveProfile.ProfileId);
        }
    }
}
