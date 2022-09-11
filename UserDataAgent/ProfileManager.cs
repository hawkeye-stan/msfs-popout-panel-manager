using MSFSPopoutPanelManager.Shared;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

namespace MSFSPopoutPanelManager.UserDataAgent
{
    public class ProfileManager
    {
        private const string USER_PROFILE_DATA_FILENAME = "userprofiledata.json";

        public static int AddProfile(string newProfileName, IList<Profile> profiles)
        {
            if (profiles == null)
                return -1;

            return AddProfile(new Profile(), newProfileName, profiles);
        }

        public static int AddProfile(string newProfileName, int copyProfileId, IList<Profile> profiles)
        {
            if (profiles == null)
                return -1;

            var matchedProfile = profiles.FirstOrDefault(p => p.ProfileId == copyProfileId);

            var copiedProfile = matchedProfile.Copy<Profile>();     // Using Shared/ObjectExtensions.cs extension method
            copiedProfile.BindingAircrafts = new ObservableCollection<string>();
            copiedProfile.IsLocked = false;

            return AddProfile(copiedProfile, newProfileName, profiles);
        }

        public static bool DeleteProfile(int profileId, IList<Profile> profiles)
        {
            if (profiles == null || profileId == -1)
                return false;

            var profileToRemove = profiles.First(x => x.ProfileId == profileId);
            profiles.Remove(profileToRemove);
            WriteProfiles(profiles);

            StatusMessageWriter.WriteMessage($"Profile '{profileToRemove.ProfileName}' has been deleted successfully.", StatusMessageType.Info, false);
            return true;
        }

        public static void AddProfileBinding(string aircraft, int activeProfileId, IList<Profile> profiles)
        {
            var boundProfile = profiles.FirstOrDefault(p => p.BindingAircrafts.Any(p => p == aircraft));
            if (boundProfile != null)
            {
                StatusMessageWriter.WriteMessage($"Unable to add binding to the profile because '{aircraft}' was already bound to profile '{boundProfile.ProfileName}'.", StatusMessageType.Error, false);
                return;
            }

            profiles.First(p => p.ProfileId == activeProfileId).BindingAircrafts.Add(aircraft);
            WriteProfiles(profiles);

            StatusMessageWriter.WriteMessage($"Binding for the profile has been added successfully.", StatusMessageType.Info, false);
        }

        public static void DeleteProfileBinding(string aircraft, int activeProfileId, IList<Profile> profiles)
        {
            profiles.First(p => p.ProfileId == activeProfileId).BindingAircrafts.Remove(aircraft);
            WriteProfiles(profiles);

            StatusMessageWriter.WriteMessage($"Binding for the profile has been deleted successfully.", StatusMessageType.Info, false);
        }

        public static List<Profile> ReadProfiles()
        {
            try
            {
                using (StreamReader reader = new StreamReader(Path.Combine(FileIo.GetUserDataFilePath(), USER_PROFILE_DATA_FILENAME)))
                {
                    return new List<Profile>(JsonConvert.DeserializeObject<List<Profile>>(reader.ReadToEnd()));
                }
            }
            catch
            {
                return new List<Profile>();
            }
        }

        public static void WriteProfiles(IList<Profile> profiles)
        {
            if (profiles == null)
            {
                FileLogger.WriteLog($"User Profiles is null.", StatusMessageType.Error);
                throw new Exception("User Profiles is null.");
            }

            try
            {
                var userProfilePath = FileIo.GetUserDataFilePath();

                if (!Directory.Exists(userProfilePath))
                    Directory.CreateDirectory(userProfilePath);

                using (StreamWriter file = File.CreateText(Path.Combine(userProfilePath, USER_PROFILE_DATA_FILENAME)))
                {
                    JsonSerializer serializer = new JsonSerializer();
                    serializer.Serialize(file, profiles);
                }
            }
            catch
            {
                FileLogger.WriteLog($"Unable to write user data file: {USER_PROFILE_DATA_FILENAME}", StatusMessageType.Error);
            }
        }

        private static int AddProfile(Profile userProfile, string newProfileName, IList<Profile> profiles)
        {
            var newPlaneProfile = userProfile;
            var newProfileId = profiles.Count > 0 ? profiles.Max(x => x.ProfileId) + 1 : 1;

            newPlaneProfile.ProfileName = newProfileName;
            newPlaneProfile.ProfileId = newProfileId;

            var tmpList = profiles;
            var index = tmpList.OrderBy(x => x.ProfileName).ToList().FindIndex(x => x.ProfileId == newProfileId);

            if (index == -1)
                profiles.Add(newPlaneProfile);
            else
                profiles.Insert(index, newPlaneProfile);

            WriteProfiles(profiles);

            StatusMessageWriter.WriteMessage($"Profile '{newPlaneProfile.ProfileName}' has been added successfully.", StatusMessageType.Info, false);

            return newProfileId;
        }
    }
}
