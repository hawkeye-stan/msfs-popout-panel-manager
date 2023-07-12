using MSFSPopoutPanelManager.DomainModel.DataFile;
using MSFSPopoutPanelManager.DomainModel.Legacy;
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

                bool isNeededMigration = false;
                string fileContent;

                using (StreamReader reader = new StreamReader(Path.Combine(FileIo.GetUserDataFilePath(), USER_PROFILE_DATA_FILENAME)))
                {
                    fileContent = reader.ReadToEnd();
                    isNeededMigration = fileContent.Substring(0, 1) == "["; // still using pre version 4.0 format
                }

                if (isNeededMigration)
                {
                    var userProfiles = MigrateData.MigrateUserProfileFile(fileContent) ?? new List<UserProfile>();
                    WriteProfiles(userProfiles);
                    return userProfiles;
                }

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
                FileLogger.WriteLog($"User Profiles is null.", StatusMessageType.Error);
                throw new Exception("User Profiles is null.");
            }

            try
            {
                //Debug.WriteLine("Writing user profile data file...");

                var userProfilePath = FileIo.GetUserDataFilePath();

                if (!Directory.Exists(userProfilePath))
                    Directory.CreateDirectory(userProfilePath);

                using (StreamWriter file = File.CreateText(Path.Combine(userProfilePath, USER_PROFILE_DATA_FILENAME)))
                {
                    JsonSerializer serializer = new JsonSerializer();
                    serializer.Serialize(file, new UserProfileFile { Profiles = profiles });
                }
            }
            catch
            {
                FileLogger.WriteLog($"Unable to write user data file: {USER_PROFILE_DATA_FILENAME}", StatusMessageType.Error);
            }
        }

        public static List<LegacyProfile> LegacyReadProfiles()
        {
            try
            {
                using (StreamReader reader = new StreamReader(Path.Combine(FileIo.GetUserDataFilePath(), USER_PROFILE_DATA_FILENAME)))
                {
                    return new List<LegacyProfile>(JsonConvert.DeserializeObject<List<LegacyProfile>>(reader.ReadToEnd()));
                }
            }
            catch
            {
                return new List<LegacyProfile>();
            }
        }
    }
}
