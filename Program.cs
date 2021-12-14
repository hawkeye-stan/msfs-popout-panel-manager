using log4net;
using log4net.Config;
using MSFSPopoutPanelManager.Shared;
using MSFSPopoutPanelManager.UI;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;

namespace MSFSPopoutPanelManager
{
    static class Program
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            bool createNew;
            using var mutex = new Mutex(true, typeof(Program).Namespace, out createNew);

            var logRepository = LogManager.GetRepository(Assembly.GetEntryAssembly());
            XmlConfigurator.Configure(logRepository, new FileInfo("log4net.config"));

            if (createNew)
            {
                Application.SetHighDpiMode(HighDpiMode.SystemAware);
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.ThreadException += new ThreadExceptionEventHandler(HandleThreadException);
                Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
                AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(UnhandledDomainException);
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

            void HandleThreadException(object sender, ThreadExceptionEventArgs e)
            {
                Log.Error(e.Exception.Message, e.Exception);
                ShowExceptionForm();
            }

            void UnhandledDomainException(object sender, UnhandledExceptionEventArgs e)
            {
                var exception = (Exception)e.ExceptionObject;
                Log.Error(exception.Message, exception);

                ShowExceptionForm();
            }

            void ShowExceptionForm()
            {
                var title = "Critical Error";
                var message = "Application has encountered a critical error and will be closed. Please see the file error.log for information.";

                using (var form = new ConfirmDialogForm(title, message, false) { StartPosition = FormStartPosition.CenterParent })
                {
                    var dialogResult = form.ShowDialog();

                    if (dialogResult == DialogResult.OK)
                    {
                        Application.Exit();
                    }
                }
            }
        }
    }
}
