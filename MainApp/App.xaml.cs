using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MSFSPopoutPanelManager.MainApp.ViewModel;
using MSFSPopoutPanelManager.Orchestration;
using MSFSPopoutPanelManager.Shared;
using MSFSPopoutPanelManager.WindowsAgent;
using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace MSFSPopoutPanelManager.MainApp
{
    public partial class App : Application
    {
        public static IHost AppHost { get; private set; }

        public App()
        {
        }

        protected override async void OnStartup(StartupEventArgs e)
        {
            DpiAwareness.Enable();

            // Must run this first
            if(IsRunning())
            {
                //app is already running! Exiting the application  
                Application.Current.Shutdown();
            }
            else
            {
                // Setup all unhandle exception handlers
                Dispatcher.UnhandledException += HandleDispatcherException;
                TaskScheduler.UnobservedTaskException += HandleTaskSchedulerUnobservedTaskException;
                AppDomain.CurrentDomain.UnhandledException += HandledDomainException;

                // Setup dependency injections
                AppHost = Host.CreateDefaultBuilder()
                    .ConfigureServices((hostContext, services) =>
                    {
                        services.AddSingleton<AppWindow>();

                        services.AddSingleton<MainOrchestrator>();
                        services.AddSingleton<OrchestratorUIHelper>(s => new OrchestratorUIHelper(s.GetRequiredService<MainOrchestrator>()));

                        services.AddSingleton<ApplicationViewModel>(s => new ApplicationViewModel(s.GetRequiredService<MainOrchestrator>()));
                        services.AddSingleton<HelpViewModel>(s => new HelpViewModel(s.GetRequiredService<MainOrchestrator>()));
                        services.AddSingleton<ProfileCardListViewModel>(s => new ProfileCardListViewModel(s.GetRequiredService<MainOrchestrator>()));
                        services.AddSingleton<ProfileCardViewModel>(s => new ProfileCardViewModel(s.GetRequiredService<MainOrchestrator>()));
                        services.AddSingleton<TrayIconViewModel>(s => new TrayIconViewModel(s.GetRequiredService<MainOrchestrator>()));

                        services.AddTransient<AddProfileViewModel>(s => new AddProfileViewModel(s.GetRequiredService<MainOrchestrator>()));
                        services.AddTransient<PopOutPanelListViewModel>(s => new PopOutPanelListViewModel(s.GetRequiredService<MainOrchestrator>()));
                        services.AddTransient<PopOutPanelCardViewModel>(s => new PopOutPanelCardViewModel(s.GetRequiredService<MainOrchestrator>()));
                        services.AddTransient<PanelConfigFieldViewModel>(s => new PanelConfigFieldViewModel(s.GetRequiredService<MainOrchestrator>()));
                        services.AddTransient<PanelCoorOverlayViewModel>(s => new PanelCoorOverlayViewModel(s.GetRequiredService<MainOrchestrator>()));

                        services.AddTransient<MessageWindowViewModel>(s => new MessageWindowViewModel(s.GetRequiredService<MainOrchestrator>()));
                        services.AddTransient<HudBarViewModel>(s => new HudBarViewModel(s.GetRequiredService<MainOrchestrator>()));

                    }).Build();

                await AppHost!.StartAsync();

                // Startup window (must come after DPI setup above)
                MainWindow = AppHost.Services.GetRequiredService<AppWindow>();
                MainWindow.Show();

                // Setup orchestration UI handler
                var orchestrationUIHelper = App.AppHost.Services.GetRequiredService<OrchestratorUIHelper>();

                // Setup message window dialog
                var messageWindow = new MessageWindow();
                messageWindow.Show();

                base.OnStartup(e);
            }
        }

        private bool IsRunning()
        {
            return Process.GetProcesses().Count(p => p.ProcessName.Contains(Assembly.GetEntryAssembly().GetName().Name)) > 1;
        }

        private void HandleTaskSchedulerUnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
        {
            FileLogger.WriteException(e.Exception.Message, e.Exception);
        }

        private void HandleDispatcherException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            e.Handled = true;
            if (e.Exception.Message != "E_INVALIDARG")      // Ignore this error
            {
                FileLogger.WriteException(e.Exception.Message, e.Exception);
            }
        }

        private void HandledDomainException(object sender, UnhandledExceptionEventArgs e)
        {
            var exception = (Exception)e.ExceptionObject;
            FileLogger.WriteException(exception.Message, exception);
        }
    }

    public enum PROCESS_DPI_AWARENESS
    {
        Process_DPI_Unaware = 0,
        Process_System_DPI_Aware = 1,
        Process_Per_Monitor_DPI_Aware = 2
    }

    public enum DPI_AWARENESS_CONTEXT
    {
        DPI_AWARENESS_CONTEXT_UNAWARE = 16,
        DPI_AWARENESS_CONTEXT_SYSTEM_AWARE = 17,
        DPI_AWARENESS_CONTEXT_PER_MONITOR_AWARE = 18,
        DPI_AWARENESS_CONTEXT_PER_MONITOR_AWARE_V2 = 34
    }

    public static class DpiAwareness
    {
        public static void Enable()
        {
            // Windows 8.1 added support for per monitor DPI
            if (Environment.OSVersion.Version >= new Version(6, 3, 0))
            {
                // Windows 10 creators update added support for per monitor v2
                if (Environment.OSVersion.Version >= new Version(10, 0, 15063))
                {
                    SetProcessDpiAwarenessContext(DPI_AWARENESS_CONTEXT.DPI_AWARENESS_CONTEXT_PER_MONITOR_AWARE_V2);
                }
                else
                {
                    SetProcessDpiAwareness(PROCESS_DPI_AWARENESS.Process_Per_Monitor_DPI_Aware);
                }
            }
            else
            {
                SetProcessDPIAware();
            }

            var process = WindowProcessManager.GetApplicationProcess();
            PROCESS_DPI_AWARENESS outValue;
            GetProcessDpiAwareness(process.Handle, out outValue);
        }

        [DllImport("User32.dll")]
        internal static extern bool SetProcessDpiAwarenessContext(DPI_AWARENESS_CONTEXT dpiFlag);

        [DllImport("SHCore.dll")]
        internal static extern bool SetProcessDpiAwareness(PROCESS_DPI_AWARENESS awareness);

        [DllImport("User32.dll")]
        internal static extern bool SetProcessDPIAware();

        [DllImport("SHCore.dll", SetLastError = true)]
        internal static extern void GetProcessDpiAwareness(IntPtr hprocess, out PROCESS_DPI_AWARENESS awareness);
    }
}