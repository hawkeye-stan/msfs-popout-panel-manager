using MSFSPopoutPanelManager.Model;
using MSFSPopoutPanelManager.Shared;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace MSFSPopoutPanelManager.Provider
{
    public class UserProfileManager
    {
        private const string USER_PROFILE_DATA_FILENAME = "userprofiledata.json";

        public ObservableCollection<UserProfile> UserProfiles { get; set; }

        public int AddUserProfile(string newProfileName)
        {
            return AddProfile(new UserProfile(), newProfileName);
        }

        public int AddUserProfileByCopyingProfile(string newProfileName, int copyProfileId)
        {
            if (UserProfiles == null)
                throw new Exception("User Profiles is null.");

            var matchedProfile = UserProfiles.FirstOrDefault(p => p.ProfileId == copyProfileId);

            var copiedProfile = matchedProfile.Copy<UserProfile>();     // Using Shared/ObjectExtensions.cs extension method
            copiedProfile.IsDefaultProfile = false;
            copiedProfile.BindingPlaneTitle = null;

            return AddProfile(copiedProfile, newProfileName);
        }

        public bool DeleteUserProfile(int profileId)
        {
            if (UserProfiles == null)
                throw new Exception("User Profiles is null.");

            if (profileId == -1)
                return false;

            var profileToRemove = UserProfiles.First(x => x.ProfileId == profileId);
            UserProfiles.Remove(profileToRemove);
            WriteUserProfiles();

            Logger.LogStatus($"Profile '{profileToRemove.ProfileName}' has been deleted successfully.", StatusMessageType.Info);

            return true;
        }

        public bool SetDefaultUserProfile(int profileId)
        {
            if (UserProfiles == null)
                throw new Exception("User Profiles is null.");

            if (profileId == -1)
                return false;

            var profile = UserProfiles.First(x => x.ProfileId == profileId);

            profile.IsDefaultProfile = true;
            foreach (var p in UserProfiles)
            {
                if (p.ProfileId != profileId)
                    p.IsDefaultProfile = false;
            }

            WriteUserProfiles();

            Logger.LogStatus($"Profile '{profile.ProfileName}' has been set as default.", StatusMessageType.Info);

            return true;
        }

        public UserProfile GetDefaultProfile()
        {
            return UserProfiles.ToList().Find(x => x.IsDefaultProfile);
        }

        public void AddProfileBinding(string planeTitle, int activeProfileId)
        {
            var bindedProfile = UserProfiles.FirstOrDefault(p => p.BindingPlaneTitle == planeTitle);
            if (bindedProfile != null)
            {
                Logger.LogStatus($"Unable to add binding to the profile because '{planeTitle}' was already bound to profile '{bindedProfile.ProfileName}'.", StatusMessageType.Error);
                return;
            }

            UserProfiles.First(p => p.ProfileId == activeProfileId).BindingPlaneTitle = planeTitle;
            WriteUserProfiles();
            Logger.LogStatus($"Binding for the profile has been added successfully.", StatusMessageType.Info);
        }

        public void DeleteProfileBinding(int activeProfileId)
        {
            UserProfiles.First(p => p.ProfileId == activeProfileId).BindingPlaneTitle = null;
            WriteUserProfiles();
            Logger.LogStatus($"Binding for the profile has been deleted successfully.", StatusMessageType.Info);
        }

        public void ReadUserProfiles()
        {
            try
            {
                using (StreamReader reader = new StreamReader(Path.Combine(FileIo.GetUserDataFilePath(), USER_PROFILE_DATA_FILENAME)))
                {
                    UserProfiles = new ObservableCollection<UserProfile>(JsonConvert.DeserializeObject<List<UserProfile>>(reader.ReadToEnd()));
                }
            }
            catch
            {
                UserProfiles = new ObservableCollection<UserProfile>(new List<UserProfile>());
            }
        }

        public void WriteUserProfiles()
        {
            Debug.WriteLine("saving profile....");

            if (UserProfiles == null)
                throw new Exception("User Profiles is null.");

            try
            {
                var userProfilePath = FileIo.GetUserDataFilePath();

                if (!Directory.Exists(userProfilePath))
                    Directory.CreateDirectory(userProfilePath);

                using (StreamWriter file = File.CreateText(Path.Combine(userProfilePath, USER_PROFILE_DATA_FILENAME)))
                {
                    JsonSerializer serializer = new JsonSerializer();
                    serializer.Serialize(file, UserProfiles);
                }
            }
            catch
            {
                Logger.LogStatus($"Unable to write user data file: {USER_PROFILE_DATA_FILENAME}", StatusMessageType.Error);
            }
        }

        private int AddProfile(UserProfile userProfile, string newProfileName)
        {
            if (UserProfiles == null)
                throw new Exception("User Profiles is null.");

            var newPlaneProfile = userProfile;
            var newProfileId = UserProfiles.Count > 0 ? UserProfiles.Max(x => x.ProfileId) + 1 : 1;

            newPlaneProfile.ProfileName = newProfileName;
            newPlaneProfile.ProfileId = newProfileId;

            var tmpList = UserProfiles.ToList();
            tmpList.Add(newPlaneProfile);
            var index = tmpList.OrderBy(x => x.ProfileName).ToList().FindIndex(x => x.ProfileId == newProfileId);
            UserProfiles.Insert(index, newPlaneProfile);
            WriteUserProfiles();

            Logger.LogStatus($"Profile '{newPlaneProfile.ProfileName}' has been added successfully.", StatusMessageType.Info);

            return newProfileId;
        }
    }
}
