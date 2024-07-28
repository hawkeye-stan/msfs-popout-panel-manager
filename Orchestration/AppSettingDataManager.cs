using MSFSPopoutPanelManager.DomainModel.DataFile;
using MSFSPopoutPanelManager.DomainModel.Setting;
using MSFSPopoutPanelManager.Shared;
using Newtonsoft.Json;
using System;
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

                // Try to read folder path from both locations
                var documentsFolderPath = Path.Combine(FileIo.GetUserDataFilePath(false), APP_SETTING_DATA_FILENAME);
                var applicationDataFolderPath = Path.Combine(FileIo.GetUserDataFilePath(true), APP_SETTING_DATA_FILENAME);
                string folderPath;

                if (File.Exists(applicationDataFolderPath))
                {
                    folderPath = applicationDataFolderPath;
                    FileLogger.UseApplicationDataPath = true;
                }
                else
                {
                    folderPath = documentsFolderPath;
                    FileLogger.UseApplicationDataPath = false;
                }

                using var reader = new StreamReader(folderPath);
                var fileContent = reader.ReadToEnd();

                var appSetting = JsonConvert.DeserializeObject<AppSettingFile>(fileContent).ApplicationSetting;

                appSetting.GeneralSetting.UseApplicationDataPath = folderPath.IndexOf("Roaming", StringComparison.Ordinal) > -1;

                return appSetting;
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
                CreateAppSettings(appSetting);
            }
            catch
            {
                FileLogger.WriteLog($"Unable to write app setting data file: {APP_SETTING_DATA_FILENAME}", StatusMessageType.Error);
            }
        }

        public static void MoveAppSettings(ApplicationSetting appSetting)
        {
            try
            {
                CreateAppSettings(appSetting);

                // Remove file in old path
                var oldPath = FileIo.GetUserDataFilePath(!appSetting.GeneralSetting.UseApplicationDataPath);
                if (File.Exists(oldPath))
                    File.Delete(oldPath);
            }
            catch
            {
                FileLogger.WriteLog($"Unable to remove old app setting data folder:", StatusMessageType.Error);
            }
        }

        private static void CreateAppSettings(ApplicationSetting appSetting)
        {
            var dataFilePath = FileIo.GetUserDataFilePath(appSetting.GeneralSetting.UseApplicationDataPath);

            if (string.IsNullOrEmpty(dataFilePath))
                throw new Exception("Unable to get app setting data file path.");

            if (!Directory.Exists(dataFilePath))
                Directory.CreateDirectory(dataFilePath);

            using var file = File.CreateText(Path.Combine(dataFilePath, APP_SETTING_DATA_FILENAME));
            var serializer = new JsonSerializer();
            serializer.Serialize(file, new AppSettingFile { ApplicationSetting = appSetting });
        }
    }
}
