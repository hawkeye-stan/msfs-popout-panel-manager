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

        public bool HasOrphanAppCache()
        {
            var appLocal = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var srcPath = Path.Combine(appLocal, @"temp\.net\MSFSPopoutPanelManager");

            var di = new DirectoryInfo(srcPath);

            if (di.Exists)
                return di.GetDirectories().Length > 1;

            return false;
        }

        public void DownloadVccLibrary()
        {
            var target = "https://aka.ms/vs/17/release/vc_redist.x64.exe";
            try
            {
                var psi = new ProcessStartInfo
                {
                    UseShellExecute = true,
                    FileName = target
                };

                Process.Start(psi);
            }
            catch
            {
                // ignored
            }
        }

        public void DeleteAppCache()
        {
            var appLocal = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var srcPath = Path.Combine(appLocal, @"temp\.net\MSFSPopoutPanelManager");

            try
            {
                var currentAppPath = Directory.GetParent(Assembly.GetExecutingAssembly().Location);

                if (currentAppPath == null)
                    throw new ApplicationException("Unable to determine POPM application path.");

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
    }
}
