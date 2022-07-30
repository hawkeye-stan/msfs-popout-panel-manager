using System;
using System.Diagnostics;

namespace MSFSPopoutPanelManager.WindowsAgent
{
    public class WindowProcessManager
    {
        public static string GetApplicationVersion()
        {
            var systemAssemblyVersion = System.Reflection.Assembly.GetEntryAssembly().GetName().Version;
            var appVersion = $"{systemAssemblyVersion.Major}.{systemAssemblyVersion.Minor}.{systemAssemblyVersion.Build}";
            if (systemAssemblyVersion.Revision > 0)
                appVersion += "." + systemAssemblyVersion.Revision.ToString("D4"); ;

            return appVersion;
        }

        public static WindowProcess GetSimulatorProcess()
        {
            return GetWindowProcess("FlightSimulator");
        }

        public static WindowProcess GetApplicationProcess()
        {
            return GetWindowProcess("MSFSPopoutPanelManager");
        }

        public static void OpenOnlineUserGuide()
        {
            Process.Start(new ProcessStartInfo("https://github.com/hawkeye-stan/msfs-popout-panel-manager#msfs-pop-out-panel-manager") { UseShellExecute = true });
        }

        public static void OpenOnlineLatestDownload()
        {
            Process.Start(new ProcessStartInfo("https://github.com/hawkeye-stan/msfs-popout-panel-manager/releases") { UseShellExecute = true });
        }

        private static WindowProcess GetWindowProcess(string processName)
        {
            foreach (var process in Process.GetProcesses())
            {
                if (process.ProcessName == processName)
                {
                    return new WindowProcess()
                    {
                        ProcessId = process.Id,
                        ProcessName = process.ProcessName,
                        Handle = process.MainWindowHandle,
                        MainModule = process.MainModule
                    };
                }
            }

            return null;
        }
    }

    public class WindowProcess
    {
        public int ProcessId { get; set; }

        public string ProcessName { get; set; }

        public IntPtr Handle { get; set; }

        public ProcessModule MainModule { get; set; }
    }
}
