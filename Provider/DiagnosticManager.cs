using MSFSPopoutPanelManager.Shared;
using System;
using System.Diagnostics;
using System.Timers;

namespace MSFSPopoutPanelManager.Provider
{
    public class DiagnosticManager
    {
        public static event EventHandler<EventArgs<bool>> OnPollMsfsConnectionResult;

        public static string GetApplicationVersion()
        {
            var systemAssemblyVersion = System.Reflection.Assembly.GetEntryAssembly().GetName().Version;
            var appVersion = $"{systemAssemblyVersion.Major}.{systemAssemblyVersion.Minor}.{systemAssemblyVersion.Build}";
            if (systemAssemblyVersion.Revision > 0)
                appVersion += "." + systemAssemblyVersion.Revision;

            return appVersion;
        }

        public static WindowProcess GetSimulatorProcess()
        {
            return GetProcess("FlightSimulator");
        }

        public static WindowProcess GetApplicationProcess()
        {
            return GetProcess("MSFSPopoutPanelManager");
        }

        public static void StartPollingMsfsConnection()
        {
            Timer timer = new Timer();
            timer.Interval = 2000;
            timer.Elapsed += (sender, e) =>
            {
                OnPollMsfsConnectionResult?.Invoke(null, new EventArgs<bool>(GetSimulatorProcess() != null));
            };
            timer.Enabled = true;

        }

        public static void OpenOnlineUserGuide()
        {
            Process.Start(new ProcessStartInfo("https://github.com/hawkeye-stan/msfs-popout-panel-manager#msfs-pop-out-panel-manager") { UseShellExecute = true });
        }

        public static void OpenOnlineLatestDownload()
        {
            Process.Start(new ProcessStartInfo("https://github.com/hawkeye-stan/msfs-popout-panel-manager/releases") { UseShellExecute = true });
        }

        private static WindowProcess GetProcess(string processName)
        {
            foreach (var process in Process.GetProcesses())
            {
                if (process.ProcessName == processName)
                {
                    return new WindowProcess()
                    {
                        ProcessId = process.Id,
                        ProcessName = process.ProcessName,
                        Handle = process.MainWindowHandle
                    };
                }
            }

            return null;
        }
    }
}
