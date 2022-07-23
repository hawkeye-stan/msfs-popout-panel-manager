using MSFSPopoutPanelManager.Shared;
using Newtonsoft.Json;
using System.IO;

namespace MSFSPopoutPanelManager.UserDataAgent
{
    public class AppSettingManager
    {
        private const string APP_SETTING_DATA_FILENAME = "appsettingdata.json";

        public static AppSetting ReadAppSetting()
        {
            try
            {
                using (StreamReader reader = new StreamReader(Path.Combine(FileIo.GetUserDataFilePath(), APP_SETTING_DATA_FILENAME)))
                {
                    return JsonConvert.DeserializeObject<AppSetting>(reader.ReadToEnd());
                }
            }
            catch
            {
                return null;
            }
        }

        public static void WriteAppSetting(AppSetting appSetting)
        {
            try
            {
                var userProfilePath = FileIo.GetUserDataFilePath();
                if (!Directory.Exists(userProfilePath))
                    Directory.CreateDirectory(userProfilePath);

                using (StreamWriter file = File.CreateText(Path.Combine(userProfilePath, APP_SETTING_DATA_FILENAME)))
                {
                    JsonSerializer serializer = new JsonSerializer();
                    serializer.Serialize(file, appSetting);
                }
            }
            catch
            {
                FileLogger.WriteLog($"Unable to write app setting data file: {APP_SETTING_DATA_FILENAME}", StatusMessageType.Error);
            }
        }
    }
}
