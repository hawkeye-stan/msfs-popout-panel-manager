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

        public static IList<UserProfile> ReadProfiles()
        {
            try
            {
                Debug.WriteLine("Reading user profile data file...");

                using var reader = new StreamReader(Path.Combine(FileIo.GetUserDataFilePath(), USER_PROFILE_DATA_FILENAME));
                var fileContent = reader.ReadToEnd();

                return JsonConvert.DeserializeObject<UserProfileFile>(fileContent).Profiles;
            }
            catch
            {
                return new List<UserProfile>();
            }
        }

        public static void WriteProfiles(IList<UserProfile> profiles)
        {
            if (profiles == null)
            {
                FileLogger.WriteLog("User Profiles is null.", StatusMessageType.Error);
                throw new Exception("User Profiles is null.");
            }

            try
            {
                var dataFilePath = FileIo.GetUserDataFilePath();

                if (string.IsNullOrEmpty(dataFilePath))
                    throw new Exception("Unable to get user profile dat file path.");

                if (!Directory.Exists(dataFilePath))
                    Directory.CreateDirectory(dataFilePath);

                using var file = File.CreateText(Path.Combine(dataFilePath, USER_PROFILE_DATA_FILENAME));
                var serializer = new JsonSerializer();
                serializer.Serialize(file, new UserProfileFile { Profiles = profiles });
            }
            catch
            {
                FileLogger.WriteLog($"Unable to write user data file: {USER_PROFILE_DATA_FILENAME}", StatusMessageType.Error);
            }
        }
    }
}
