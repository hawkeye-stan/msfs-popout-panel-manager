using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MSFSPopoutPanelManager.MainApp.AppWindow;
using MSFSPopoutPanelManager.MainApp.ViewModel;
using MSFSPopoutPanelManager.Orchestration;
using MSFSPopoutPanelManager.Shared;
using MSFSPopoutPanelManager.WindowsAgent;
using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using Application = System.Windows.Application;

namespace MSFSPopoutPanelManager.MainApp
{
    public partial class App
    {
        public SharedStorage SharedStorage;

        public static IHost AppHost { get; private set; }
        
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
                // Setup all unhandled exception handlers
                Dispatcher.UnhandledException += HandleDispatcherException;
                TaskScheduler.UnobservedTaskException += HandleTaskSchedulerUnobservedTaskException;
                AppDomain.CurrentDomain.UnhandledException += HandledDomainException;

                // Setup all data storage objects
                SharedStorage = new SharedStorage();
                SharedStorage.AppSettingData.ReadSettings();
                SharedStorage.ProfileData.AppSettingDataRef = SharedStorage.AppSettingData;
                SharedStorage.ProfileData.ReadProfiles();
                FileLogger.UseApplicationDataPath = SharedStorage.AppSettingData.ApplicationSetting.GeneralSetting.UseApplicationDataPath;

                // Setup dependency injections
                AppHost = Host.CreateDefaultBuilder()
                    .ConfigureServices((_, services) =>
                    {
                        services.AddSingleton<AppMainWindow>();

                        services.AddSingleton(s => new AppOrchestrator(SharedStorage, s.GetRequiredService<PanelConfigurationOrchestrator>(), s.GetRequiredService<FlightSimOrchestrator>(), s.GetRequiredService<HelpOrchestrator>(), s.GetRequiredService<KeyboardOrchestrator>()));
                        services.AddSingleton(_ => new ProfileOrchestrator(SharedStorage));
                        services.AddSingleton(_ => new DynamicLodOrchestrator(SharedStorage));
                        services.AddSingleton(s => new PanelSourceOrchestrator(SharedStorage, s.GetRequiredService<FlightSimOrchestrator>()));
                        services.AddSingleton(s => new PanelPopOutOrchestrator(SharedStorage, s.GetRequiredService<FlightSimOrchestrator>(), s.GetRequiredService<PanelSourceOrchestrator>(), s.GetRequiredService<PanelConfigurationOrchestrator>(), s.GetRequiredService<KeyboardOrchestrator>()));
                        services.AddSingleton(s => new PanelConfigurationOrchestrator(SharedStorage, s.GetRequiredService<FlightSimOrchestrator>(), s.GetRequiredService<KeyboardOrchestrator>()));
                        services.AddSingleton(s => new FlightSimOrchestrator(SharedStorage, s.GetRequiredService<DynamicLodOrchestrator>()));
                        services.AddSingleton(_ => new KeyboardOrchestrator(SharedStorage));
                        services.AddSingleton(_ => new HelpOrchestrator());

                        services.AddSingleton(s => new OrchestratorUiHelper(SharedStorage, s.GetRequiredService<PanelSourceOrchestrator>(), s.GetRequiredService<PanelPopOutOrchestrator>()));
                        services.AddSingleton(s => new ApplicationViewModel(SharedStorage, s.GetRequiredService<AppOrchestrator>()));
                        services.AddSingleton(s => new HelpViewModel(SharedStorage, s.GetRequiredService<HelpOrchestrator>()));
                        services.AddSingleton(s => new ProfileCardListViewModel(SharedStorage, s.GetRequiredService<ProfileOrchestrator>(), s.GetRequiredService<PanelSourceOrchestrator>()));
                        services.AddSingleton(s => new ProfileCardViewModel(SharedStorage, s.GetRequiredService<ProfileOrchestrator>(), s.GetRequiredService<PanelSourceOrchestrator>(), s.GetRequiredService<PanelConfigurationOrchestrator>(), s.GetRequiredService<PanelPopOutOrchestrator>()));
                        services.AddSingleton(s => new TrayIconViewModel(SharedStorage, s.GetRequiredService<AppOrchestrator>(), s.GetRequiredService<PanelPopOutOrchestrator>()));
                        services.AddSingleton(s => new PreferenceDrawerViewModel(SharedStorage, s.GetRequiredService<KeyboardOrchestrator>()));

                        services.AddTransient(s => new AddProfileViewModel(SharedStorage, s.GetRequiredService<ProfileOrchestrator>(), s.GetRequiredService<PanelSourceOrchestrator>()));
                        services.AddTransient(_ => new PopOutPanelListViewModel(SharedStorage));
                        services.AddTransient(s => new PopOutPanelConfigCardViewModel(SharedStorage, s.GetRequiredService<PanelSourceOrchestrator>(), s.GetRequiredService<PanelConfigurationOrchestrator>()));
                        services.AddTransient(s => new PopOutPanelSourceCardViewModel(SharedStorage, s.GetRequiredService<PanelSourceOrchestrator>(), s.GetRequiredService<PanelConfigurationOrchestrator>()));
                        services.AddTransient(s => new PopOutPanelSourceLegacyCardViewModel(SharedStorage, s.GetRequiredService<PanelSourceOrchestrator>(), s.GetRequiredService<PanelConfigurationOrchestrator>()));
                        services.AddTransient(s => new PanelConfigFieldViewModel(SharedStorage, s.GetRequiredService<PanelConfigurationOrchestrator>()));
                        services.AddTransient(_ => new PanelCoorOverlayViewModel(SharedStorage));

                        services.AddTransient(s => new MessageWindowViewModel(SharedStorage, s.GetRequiredService<PanelSourceOrchestrator>(), s.GetRequiredService<PanelPopOutOrchestrator>()));
                        services.AddTransient(s => new HudBarViewModel(SharedStorage, s.GetRequiredService<FlightSimOrchestrator>()));
                        services.AddTransient(_ => new NumPadViewModel(SharedStorage));
                        services.AddTransient(_ => new SwitchWindowViewModel(SharedStorage));

                    }).Build();

                await AppHost!.StartAsync();

                // Startup window (must come after DPI setup above)
                MainWindow = AppHost.Services.GetRequiredService<AppMainWindow>();
                MainWindow?.Show();

                // Setup orchestration UI handler
                App.AppHost.Services.GetRequiredService<OrchestratorUiHelper>();

                // Setup message window dialog
                var messageWindow = new MessageWindow();
                messageWindow.Show();

                base.OnStartup(e);
            }
        }

        private bool IsRunning()
        {
            var assembly = Assembly.GetEntryAssembly();

            if (assembly == null)
                return false;

            var assemblyName = assembly.GetName().Name;
            if (string.IsNullOrEmpty(assemblyName))
                return false;

            return Process.GetProcesses().Count(p => p.ProcessName.Contains(assemblyName)) > 1;
        }

        private void HandleTaskSchedulerUnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
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

    public enum ProcessDpiAwareness
    {
        //PROCESS_DPI_UNAWARE = 0,
        //PROCESS_SYSTEM_DPI_AWARE = 1,
        PROCESS_PER_MONITOR_DPI_AWARE = 2
    }

    public enum DpiAwarenessContext
    {
        //DPI_AWARENESS_CONTEXT_UNAWARE = 16,
        //DPI_AWARENESS_CONTEXT_SYSTEM_AWARE = 17,
        //DPI_AWARENESS_CONTEXT_PER_MONITOR_AWARE = 18,
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
                    SetProcessDpiAwarenessContext(DpiAwarenessContext.DPI_AWARENESS_CONTEXT_PER_MONITOR_AWARE_V2);
                }
                else
                {
                    SetProcessDpiAwareness(ProcessDpiAwareness.PROCESS_PER_MONITOR_DPI_AWARE);
                }
            }
            else
            {
                SetProcessDPIAware();
            }

            var process = WindowProcessManager.GetApplicationProcess();
            GetProcessDpiAwareness(process.Handle, out _);
        }

        [DllImport("User32.dll")]
        internal static extern bool SetProcessDpiAwarenessContext(DpiAwarenessContext dpiFlag);

        [DllImport("SHCore.dll")]
        internal static extern bool SetProcessDpiAwareness(ProcessDpiAwareness awareness);

        [DllImport("User32.dll")]
        internal static extern bool SetProcessDPIAware();

        [DllImport("SHCore.dll", SetLastError = true)]
        internal static extern void GetProcessDpiAwareness(IntPtr hProcess, out ProcessDpiAwareness awareness);
    }
}