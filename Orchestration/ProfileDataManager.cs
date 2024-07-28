using MSFSPopoutPanelManager.DomainModel.DataFile;
using MSFSPopoutPanelManager.DomainModel.Profile;
using MSFSPopoutPanelManager.Shared;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace MSFSPopoutPanelManager.Orchestration
{
    public class ProfileDataManager
    {
        private const string USER_PROFILE_DATA_FILENAME = "userprofiledata.json";

        public static IList<UserProfile> ReadProfiles(bool isRoamingPath)
        {
            try
            {
                Debug.WriteLine("Reading user profile data file...");

                using var reader = new StreamReader(Path.Combine(FileIo.GetUserDataFilePath(isRoamingPath), USER_PROFILE_DATA_FILENAME));
                var fileContent = reader.ReadToEnd();

                return JsonConvert.DeserializeObject<UserProfileFile>(fileContent).Profiles;
            }
            catch
            {
                return new List<UserProfile>();
            }
        }

        public static void WriteProfiles(IList<UserProfile> profiles, bool isRoamingPath)
        {
            if (profiles == null)
            {
                FileLogger.WriteLog("User Profiles is null.", StatusMessageType.Error);
                throw new Exception("User Profiles is null.");
            }

            try
            {
                CreateProfiles(profiles, isRoamingPath);
            }
            catch
            {
                FileLogger.WriteLog($"Unable to write user data file: {USER_PROFILE_DATA_FILENAME}", StatusMessageType.Error);
            }
        }

        public static void MoveProfiles(IList<UserProfile> profiles, bool isRoamingPath)
        {
            try
            {
                CreateProfiles(profiles, isRoamingPath);

                // Remove file in old path
                var oldPath = FileIo.GetUserDataFilePath(!isRoamingPath);
                if (File.Exists(oldPath))
                    File.Delete(oldPath);
            }
            catch
            {
                FileLogger.WriteLog($"Unable to move user data file: {USER_PROFILE_DATA_FILENAME}", StatusMessageType.Error);
            }
        }

        private static void CreateProfiles(IList<UserProfile> profiles, bool isRoamingPath)
        {
            var dataFilePath = FileIo.GetUserDataFilePath(isRoamingPath);

            if (string.IsNullOrEmpty(dataFilePath))
                throw new Exception("Unable to get user profile data file path.");

            if (!Directory.Exists(dataFilePath))
                Directory.CreateDirectory(dataFilePath);

            using var file = File.CreateText(Path.Combine(dataFilePath, USER_PROFILE_DATA_FILENAME));
            var serializer = new JsonSerializer();
            serializer.Serialize(file, new UserProfileFile { Profiles = profiles });
        }
    }
}
