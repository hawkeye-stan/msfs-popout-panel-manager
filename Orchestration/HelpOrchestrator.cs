using AutoUpdaterDotNET;
using MSFSPopoutPanelManager.Shared;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace MSFSPopoutPanelManager.Orchestration
{
    public class HelpOrchestrator : ObservableObject
    {
        public void OpenGettingStarted()
        {
            Process.Start(new ProcessStartInfo("https://github.com/hawkeye-stan/msfs-popout-panel-manager/blob/master/GETTING_STARTED.md") { UseShellExecute = true });
        }

        public void OpenUserGuide()
        {
            Process.Start(new ProcessStartInfo("https://github.com/hawkeye-stan/msfs-popout-panel-manager#msfs-pop-out-panel-manager") { UseShellExecute = true });
        }

        public void OpenLatestDownloadGitHub()
        {
            Process.Start(new ProcessStartInfo("https://github.com/hawkeye-stan/msfs-popout-panel-manager/releases") { UseShellExecute = true });
        }

        public void OpenLatestDownloadFligthsimTo()
        {
            Process.Start(new ProcessStartInfo("https://flightsim.to/file/35759/msfs-pop-out-panel-manager") { UseShellExecute = true });
        }

        public void OpenLicense()
        {
            Process.Start("notepad.exe", "LICENSE");
        }

        public void OpenVersionInfo()
        {
            Process.Start("notepad.exe", "VERSION.md");
        }

        public void OpenDataFolder()
        {
            Process.Start("explorer.exe", Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "MSFS Pop Out Panel Manager"));
        }

        public bool HasOrphanAppCache()
        {
            var applocal = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var srcPath = Path.Combine(applocal, @"temp\.net\MSFSPopoutPanelManager");

            DirectoryInfo di = new DirectoryInfo(srcPath);

            if (di.Exists)
                return di.GetDirectories().Length > 1;

            return false;
        }

        public void DownloadVCCLibrary()
        {
            string target = "https://aka.ms/vs/17/release/vc_redist.x64.exe";
            try
            {
                var psi = new ProcessStartInfo();
                psi.UseShellExecute = true;
                psi.FileName = target;

                Process.Start(psi);
            }
            catch { }
        }

        public void DeleteAppCache()
        {
            var applocal = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var srcPath = Path.Combine(applocal, @"temp\.net\MSFSPopoutPanelManager");

            try
            {
                var currentAppPath = Directory.GetParent(Assembly.GetExecutingAssembly().Location);

                var dir = new DirectoryInfo(srcPath);
                var subDirs = dir.GetDirectories();

                foreach (var subDir in subDirs)
                {
                    if (subDir.FullName.ToLower().Trim() != currentAppPath.FullName.ToLower().Trim())
                    {
                        Directory.Delete(subDir.FullName, true);
                    }
                }
            }
            catch (Exception ex) 
            {
                FileLogger.WriteLog("Delete app cache exception: " + ex.Message, StatusMessageType.Error);
            }
        }

        public void RollBackUpdate()
        {
            var userProfileBackupPath = Path.Combine(FileIo.GetUserDataFilePath(), "Backup-previous-version", "userprofiledata.json");
            var appSettingBackupPath = Path.Combine(FileIo.GetUserDataFilePath(), "Backup-previous-version", "appsettingdata.json");

            var userProfilePath = Path.Combine(FileIo.GetUserDataFilePath(), "userprofiledata.json");
            var appSettingPath = Path.Combine(FileIo.GetUserDataFilePath(), "appsettingdata.json");

            if (File.Exists(userProfileBackupPath))
                File.Copy(userProfileBackupPath, userProfilePath, true);

            if (File.Exists(appSettingBackupPath))
                File.Copy(appSettingBackupPath, appSettingPath, true);

            AutoUpdater.InstalledVersion = new Version("1.0.0.0");
            AutoUpdater.Synchronous = true;
            AutoUpdater.AppTitle = "MSFS Pop Out Panel Manager";
            AutoUpdater.RunUpdateAsAdmin = false;
            AutoUpdater.UpdateFormSize = new System.Drawing.Size(1024, 660);
            AutoUpdater.UpdateMode = Mode.ForcedDownload;
            AutoUpdater.Start("https://raw.githubusercontent.com/hawkeye-stan/msfs-popout-panel-manager/master/rollback.xml");
        }

        public bool IsRollBackUpdateEnabled()
        {
            var appSettingBackupPath = Path.Combine(FileIo.GetUserDataFilePath(), "Backup-previous-version", "appsettingdata.json");

            return File.Exists(appSettingBackupPath);
        }
    }
}
