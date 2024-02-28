using System;
using System.Diagnostics;
using System.Reflection;

namespace MSFSPopoutPanelManager.WindowsAgent
{
    public class WindowProcessManager
    {
        public static string GetApplicationVersion()
        {
            var assembly = Assembly.GetEntryAssembly();

            if (assembly != null)
            {
                var assemblyName = assembly.GetName();

                var systemAssemblyVersion = assemblyName.Version;

                if (systemAssemblyVersion == null)
                    throw new ApplicationException("Unable to get application version number.");

                var appVersion =
                    $"{systemAssemblyVersion.Major}.{systemAssemblyVersion.Minor}.{systemAssemblyVersion.Build}";
                if (systemAssemblyVersion.Revision > 0)
                    appVersion += "." + systemAssemblyVersion.Revision.ToString("D4");

                return appVersion;
            }

            throw new ApplicationException("Unable to get application version number.");
        }

        public static WindowProcess SimulatorProcess { get; private set; }

        public static void GetSimulatorProcess()
        {
            SimulatorProcess = GetWindowProcess("FlightSimulator");
        }

        public static WindowProcess GetApplicationProcess()
        {
            return GetWindowProcess("MSFSPopoutPanelManager");
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
                        Modules = process.Modules
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

        public ProcessModuleCollection Modules { get; set; }
    }
}
