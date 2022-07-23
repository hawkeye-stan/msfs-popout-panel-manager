using MSFSPopoutPanelManager.Shared;
using System;
using System.IO;

namespace MSFSPopoutPanelManager.Orchestration
{
    internal class MigrateData
    {
        public static void MigrateUserDataFiles()
        {
            var packageFile = new FileInfo("MSFSPopoutPanelManager.exe");

            var newDataPath = Path.Combine(FileIo.GetUserDataFilePath());
            var oldDataPath = Path.Combine(packageFile.DirectoryName, "userdata");

            try
            {
                // Check if migration is necessary
                if (new DirectoryInfo(newDataPath).Exists)
                    return;

                // Check if upgrading from older version of app
                if (!new DirectoryInfo(oldDataPath).Exists)
                    return;

                const string APPSETTING_DATA_JSON = "appsettingdata.json";
                const string USERPROFILE_DATA_JSON = "userprofiledata.json";

                var installationPathDirInfo = packageFile.Directory;

                var oldAppSettingDataJsonPath = Path.Combine(oldDataPath, APPSETTING_DATA_JSON);
                var newAppSettingDataJsonPath = Path.Combine(newDataPath, APPSETTING_DATA_JSON);
                var oldUserProfileDataJsonPath = Path.Combine(oldDataPath, USERPROFILE_DATA_JSON);
                var newUserProfileDataJsonPath = Path.Combine(newDataPath, USERPROFILE_DATA_JSON);

                StatusMessageWriter.WriteMessage($"Performing user data migration to your Windows 'Documents' folder. Please wait.....", StatusMessageType.Info, true, 3, true);

                // Try to create user folder
                if (!new DirectoryInfo(newDataPath).Exists)
                {
                    var userDocumentFolder = Directory.CreateDirectory(newDataPath);

                    if (!new DirectoryInfo(newDataPath).Exists)
                    {
                        StatusMessageWriter.WriteMessage($"Unable to create folder '{userDocumentFolder}'. Application will close.", StatusMessageType.Error, true, 5, true);
                        Environment.Exit(0);
                    }
                }

                // Try move appsettingdata.json
                if (new FileInfo(oldAppSettingDataJsonPath).Exists)
                {
                    File.Copy(oldAppSettingDataJsonPath, newAppSettingDataJsonPath);

                    // Verify file has been copied
                    if (!new FileInfo(newAppSettingDataJsonPath).Exists)
                    {
                        StatusMessageWriter.WriteMessage("An error has occured when moving 'appsettingdata.json' to new folder location. Please try manually move this file and restart the app again.", StatusMessageType.Error, true, 5, true);
                        Environment.Exit(0);
                    }

                    File.Delete(oldAppSettingDataJsonPath);
                }

                // Try move userprofiledata.json
                if (new FileInfo(oldUserProfileDataJsonPath).Exists)
                {
                    File.Copy(oldUserProfileDataJsonPath, newUserProfileDataJsonPath);

                    // Verify file has been copied
                    if (!new FileInfo(newUserProfileDataJsonPath).Exists)
                    {
                        StatusMessageWriter.WriteMessage("An error has occured when moving 'userprofiledata.json' to new folder location. Please try manually move this file and restart the app again.", StatusMessageType.Error, true, 5, true);
                        Environment.Exit(0);
                    }

                    File.Delete(oldUserProfileDataJsonPath);
                }

                // Now remove all orphan files and folder
                CleanFolderRecursive(installationPathDirInfo);

                // Force an update of AppSetting file
                var appSettingData = new AppSettingData();
                appSettingData.ReadSettings();
                appSettingData.AppSetting.AlwaysOnTop = false;
                System.Threading.Thread.Sleep(500);
                appSettingData.AppSetting.AlwaysOnTop = true;

                StatusMessageWriter.WriteMessage(String.Empty, StatusMessageType.Info, false);
            }
            catch (Exception ex)
            {
                var msg = "An unknown user data migration error has occured. Application will close";
                FileLogger.WriteException(msg, ex);
                StatusMessageWriter.WriteMessage(msg, StatusMessageType.Error, true, 5, true);

                Environment.Exit(0);
            }
        }

        private static void CleanFolderRecursive(DirectoryInfo directory)
        {
            foreach (FileInfo filePath in directory.GetFiles())
            {
                var name = filePath.Name.ToLower();
                if (name != "msfspopoutpanelmanager.exe")
                {
                    try
                    {
                        filePath.Delete();
                    }
                    catch { }
                }
            }

            foreach (DirectoryInfo dir in directory.GetDirectories())
            {
                CleanFolderRecursive(new DirectoryInfo(dir.FullName));

                var name = dir.Name.ToLower();
                if (name == "resources" || name == "userdata")
                {
                    try
                    {
                        dir.Delete();
                    }
                    catch { }
                }
            }
        }
    }
}
