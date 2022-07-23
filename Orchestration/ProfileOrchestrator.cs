using MSFSPopoutPanelManager.Shared;

namespace MSFSPopoutPanelManager.Orchestration
{
    public class ProfileOrchestrator : ObservableObject
    {
        internal ProfileData ProfileData { get; set; }

        public void AddProfile(string profileName, int copyProfileId)
        {
            if (copyProfileId == -1)
                ProfileData.AddProfile(profileName);
            else
                ProfileData.AddProfile(profileName, copyProfileId);
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

        public void AddProfileBinding(string bindingLiveryName)
        {
            if (ProfileData.ActiveProfile != null)
                ProfileData.AddProfileBinding(bindingLiveryName, ProfileData.ActiveProfile.ProfileId);
        }

        public void DeleteProfileBinding(string bindingLiveryName)
        {
            if (ProfileData.ActiveProfile != null)
                ProfileData.DeleteProfileBinding(bindingLiveryName, ProfileData.ActiveProfile.ProfileId);
        }
    }
}
