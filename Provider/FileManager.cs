using MSFSPopoutPanelManager.Shared;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

namespace MSFSPopoutPanelManager.Provider
{
    public class FileManager
    {
        private static string UserProfileDataPath;
        private const string APP_SETTING_DATA_FILENAME = "appsettingdata.json";
        private const string USER_PROFILE_DATA_FILENAME = "userprofiledata.json";

        static FileManager()
        {
            FileManager.UserProfileDataPath = Path.Combine(Application.StartupPath, "userdata");
        }

        public static List<UserProfileData> ReadUserProfileData()
        {
            try
            {
                using (StreamReader reader = new StreamReader(Path.Combine(UserProfileDataPath, USER_PROFILE_DATA_FILENAME)))
                {
                    return JsonConvert.DeserializeObject<List<UserProfileData>>(reader.ReadToEnd());
                }
            }
            catch
            {
                return new List<UserProfileData>();
            }
        }

        public static void WriteUserProfileData(List<UserProfileData> userProfileData)
        {
            try
            {
                if (!Directory.Exists(UserProfileDataPath))
                    Directory.CreateDirectory(UserProfileDataPath);

                using (StreamWriter file = File.CreateText(Path.Combine(UserProfileDataPath, USER_PROFILE_DATA_FILENAME)))
                {
                    JsonSerializer serializer = new JsonSerializer();
                    serializer.Serialize(file, userProfileData);
                }
            }
            catch
            {
                Logger.BackgroundStatus($"Unable to write user data file: {USER_PROFILE_DATA_FILENAME}", StatusMessageType.Error);
            }
        }

        public static AppSettingData ReadAppSettingData()
        {
            try
            {
                using (StreamReader reader = new StreamReader(Path.Combine(UserProfileDataPath, APP_SETTING_DATA_FILENAME)))
                {
                    return JsonConvert.DeserializeObject<AppSettingData>(reader.ReadToEnd());
                }
            }
            catch (Exception ex)
            {
                return new AppSettingData();
            }
        }

        public static void WriteAppSettingData(AppSettingData appSettingData)
        {
            try
            {
                if (!Directory.Exists(UserProfileDataPath))
                    Directory.CreateDirectory(UserProfileDataPath);

                using (StreamWriter file = File.CreateText(Path.Combine(UserProfileDataPath, APP_SETTING_DATA_FILENAME)))
                {
                    JsonSerializer serializer = new JsonSerializer();
                    serializer.Serialize(file, appSettingData);
                }
            }
            catch
            {
                Logger.BackgroundStatus($"Unable to write app setting data file: {USER_PROFILE_DATA_FILENAME}", StatusMessageType.Error);
            }
        }
    }
}
