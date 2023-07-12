using MSFSPopoutPanelManager.DomainModel.DataFile;
using MSFSPopoutPanelManager.DomainModel.Legacy;
using MSFSPopoutPanelManager.DomainModel.Setting;
using MSFSPopoutPanelManager.Shared;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using System.IO;

namespace MSFSPopoutPanelManager.Orchestration
{
    public class AppSettingDataManager
    {
        private const string APP_SETTING_DATA_FILENAME = "appsettingdata.json";

        public static ApplicationSetting ReadAppSetting()
        {
            try
            {
                Debug.WriteLine("Reading application settings data file...");

                bool isNeededMigration = false;
                string fileContent;

                using (StreamReader reader = new StreamReader(Path.Combine(FileIo.GetUserDataFilePath(), APP_SETTING_DATA_FILENAME)))
                {
                    fileContent = reader.ReadToEnd();
                    var jTokens = JObject.Parse(fileContent);

                    isNeededMigration = jTokens["FileVersion"] == null;
                }

                if (isNeededMigration)
                {
                    var appSetting = MigrateData.MigrateAppSettingFile(fileContent) ?? new ApplicationSetting();
                    WriteAppSetting(appSetting);
                    return appSetting;
                }

                return JsonConvert.DeserializeObject<AppSetttingFile>(fileContent).ApplicationSetting;
            }
            catch
            {
                return null;
            }
        }

        public static void WriteAppSetting(ApplicationSetting appSetting)
        {
            try
            {
                Debug.WriteLine("Saving application settings data file...");

                var userProfilePath = FileIo.GetUserDataFilePath();
                if (!Directory.Exists(userProfilePath))
                    Directory.CreateDirectory(userProfilePath);

                using (StreamWriter file = File.CreateText(Path.Combine(userProfilePath, APP_SETTING_DATA_FILENAME)))
                {
                    JsonSerializer serializer = new JsonSerializer();
                    serializer.Serialize(file, new AppSetttingFile { ApplicationSetting = appSetting });
                }
            }
            catch
            {
                FileLogger.WriteLog($"Unable to write app setting data file: {APP_SETTING_DATA_FILENAME}", StatusMessageType.Error);
            }
        }

        public static LegacyAppSetting LegacyReadAppSetting()
        {
            try
            {
                Debug.WriteLine("Reading legacy (pre-v4) application settings file...");

                using (StreamReader reader = new StreamReader(Path.Combine(FileIo.GetUserDataFilePath(), APP_SETTING_DATA_FILENAME)))
                {
                    return JsonConvert.DeserializeObject<LegacyAppSetting>(reader.ReadToEnd());
                }
            }
            catch
            {
                return null;
            }
        }
    }
}
