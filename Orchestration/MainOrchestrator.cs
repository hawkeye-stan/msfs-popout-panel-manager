using AutoUpdaterDotNET;
using MSFSPopoutPanelManager.Shared;
using MSFSPopoutPanelManager.WindowsAgent;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;

namespace MSFSPopoutPanelManager.Orchestration
{
    public class MainOrchestrator : ObservableObject
    {
        public MainOrchestrator()
        {
            AppSettingData = new AppSettingData();
            ProfileData = new ProfileData();
            FlightSimData = new FlightSimData();
            ProfileData.FlightSimDataRef = FlightSimData;
            ProfileData.AppSettingDataRef = AppSettingData;
            FlightSimData.ProfileDataRef = ProfileData;

            Profile = new ProfileOrchestrator(ProfileData, FlightSimData);
            PanelSource = new PanelSourceOrchestrator(ProfileData, AppSettingData, FlightSimData);
            PanelPopOut = new PanelPopOutOrchestrator(ProfileData, AppSettingData, FlightSimData);
            PanelConfiguration = new PanelConfigurationOrchestrator(ProfileData, AppSettingData, FlightSimData);
            FlightSim = new FlightSimOrchestrator(ProfileData, AppSettingData, FlightSimData);
            Help = new HelpOrchestrator();
            Keyboard = new KeyboardOrchestrator(AppSettingData, FlightSimData);

            PanelSource.FlightSimOrchestrator = FlightSim;

            PanelPopOut.FlightSimOrchestrator = FlightSim;
            PanelPopOut.PanelSourceOrchestrator = PanelSource;
            PanelPopOut.PanelConfigurationOrchestrator = PanelConfiguration;

            FlightSim.PanelPopOutOrchestrator = PanelPopOut;
            FlightSim.PanelConfigurationOrchestrator = PanelConfiguration;
            FlightSim.OnSimulatorExited += (sender, e) => { ApplicationClose(); Environment.Exit(0); };

            Keyboard.PanelPopOutOrchestrator = PanelPopOut;
        }

        public ProfileOrchestrator Profile { get; set; }

        public PanelSourceOrchestrator PanelSource { get; set; }

        public PanelPopOutOrchestrator PanelPopOut { get; set; }

        public PanelConfigurationOrchestrator PanelConfiguration { get; set; }

        public ProfileData ProfileData { get; set; }

        public AppSettingData AppSettingData { get; private set; }

        public FlightSimData FlightSimData { get; private set; }

        public FlightSimOrchestrator FlightSim { get; set; }

        public HelpOrchestrator Help { get; set; }

        public KeyboardOrchestrator Keyboard { get; set; }

        public IntPtr ApplicationHandle { get; set; }

        public Window ApplicationWindow { get; set; }

        public void Initialize()
        {
            AppSettingData.ReadSettings();
            ProfileData.ReadProfiles();

            PanelSource.ApplicationHandle = ApplicationHandle;

            if (AppSettingData.ApplicationSetting.GeneralSetting.CheckForUpdate)
                CheckForAutoUpdate();

            ProfileData.SetActiveProfile(AppSettingData.ApplicationSetting.SystemSetting.LastUsedProfileId);     // Load last used profile

            Task.Run(() => FlightSim.StartSimConnectServer());                                                   // Start the SimConnect server

            Keyboard.Initialize();
        }

        public void ApplicationClose()
        {
            // Force unhook all win events 
            PanelConfiguration.EndConfiguration();
            PanelConfiguration.EndTouchHook();

            InputHookManager.EndKeyboardHook();
            FlightSim.EndSimConnectServer(true);
        }

        private void CheckForAutoUpdate()
        {
            string jsonPath = Path.Combine(Path.Combine(FileIo.GetUserDataFilePath(), "autoupdate.json"));
            AutoUpdater.PersistenceProvider = new JsonFilePersistenceProvider(jsonPath);
            AutoUpdater.Synchronous = true;
            AutoUpdater.AppTitle = "MSFS Pop Out Panel Manager";
            AutoUpdater.RunUpdateAsAdmin = false;
            AutoUpdater.UpdateFormSize = new System.Drawing.Size(1024, 660);
            AutoUpdater.Start(AppSettingData.ApplicationSetting.SystemSetting.AutoUpdaterUrl);
        }
    }
}
