using System;
using System.Diagnostics;
using System.Threading;
using System.Windows.Forms;

namespace MSFSPopoutPanelManager
{
    static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            bool createNew;

            using var mutex = new Mutex(true, typeof(Program).Namespace, out createNew);

            if (createNew)
            {
                Application.SetHighDpiMode(HighDpiMode.SystemAware);
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new StartupForm());
            }
            else
            {
                var current = Process.GetCurrentProcess();

                foreach (var process in Process.GetProcessesByName(current.ProcessName))
                {
                    if (process.Id == current.Id) continue;
                    PInvoke.SetForegroundWindow(process.MainWindowHandle);
                    break;
                }
            }
        }
    }
}
