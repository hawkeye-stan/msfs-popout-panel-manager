using System;
using MSFSPopoutPanelManager.DomainModel.DataFile;
using MSFSPopoutPanelManager.DomainModel.Setting;
using MSFSPopoutPanelManager.Shared;
using Newtonsoft.Json;
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

                using var reader = new StreamReader(Path.Combine(FileIo.GetUserDataFilePath(), APP_SETTING_DATA_FILENAME));
                var fileContent = reader.ReadToEnd();

                return JsonConvert.DeserializeObject<AppSettingFile>(fileContent).ApplicationSetting;
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

                var dataFilePath = FileIo.GetUserDataFilePath();

                if (string.IsNullOrEmpty(dataFilePath))
                    throw new Exception("Unable to get app setting data file path.");

                if (!Directory.Exists(dataFilePath))
                    Directory.CreateDirectory(dataFilePath);

                using var file = File.CreateText(Path.Combine(dataFilePath, APP_SETTING_DATA_FILENAME));
                var serializer = new JsonSerializer();
                serializer.Serialize(file, new AppSettingFile { ApplicationSetting = appSetting });
            }
            catch
            {
                FileLogger.WriteLog($"Unable to write app setting data file: {APP_SETTING_DATA_FILENAME}", StatusMessageType.Error);
            }
        }
    }
}
