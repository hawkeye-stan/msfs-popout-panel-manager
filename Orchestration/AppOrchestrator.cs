using AutoUpdaterDotNET;
using MSFSPopoutPanelManager.Shared;
using MSFSPopoutPanelManager.WindowsAgent;
using System;
using System.IO;
using System.Threading.Tasks;

namespace MSFSPopoutPanelManager.Orchestration
{
    public class AppOrchestrator : BaseOrchestrator
    {
        private readonly PanelConfigurationOrchestrator _panelConfigurationOrchestrator;
        private readonly FlightSimOrchestrator _flightSimOrchestrator;
        private readonly KeyboardOrchestrator _keyboardOrchestrator;

        public AppOrchestrator(SharedStorage sharedStorage, PanelConfigurationOrchestrator panelConfigurationOrchestrator, FlightSimOrchestrator flightSimOrchestrator, HelpOrchestrator helpOrchestrator, KeyboardOrchestrator keyboardOrchestrator) : base(sharedStorage)
        {
            _panelConfigurationOrchestrator = panelConfigurationOrchestrator;
            _flightSimOrchestrator = flightSimOrchestrator;
            _keyboardOrchestrator = keyboardOrchestrator;

            ProfileData.FlightSimDataRef = FlightSimData;
            ProfileData.AppSettingDataRef = AppSettingData;
            FlightSimData.ProfileDataRef = ProfileData;

            _flightSimOrchestrator.OnSimulatorExited += (_, _) => { ApplicationClose(); Environment.Exit(0); };

            // Delete all existing cache version of app
            helpOrchestrator.DeleteAppCache();
        }

        public ProfileOrchestrator Profile { get; set; }

        public void Initialize()
        {
            // Add default dynamic LOD configs
            if (!AppSettingData.ApplicationSetting.DynamicLodSetting.IsEnabled &&
                AppSettingData.ApplicationSetting.DynamicLodSetting.TlodConfigs.Count == 0 &&
                AppSettingData.ApplicationSetting.DynamicLodSetting.OlodConfigs.Count == 0)
            {
                AppSettingData.ApplicationSetting.DynamicLodSetting.AddDefaultTLodConfigs();
                AppSettingData.ApplicationSetting.DynamicLodSetting.AddDefaultOLodConfigs();
            }

            if (AppSettingData.ApplicationSetting.GeneralSetting.CheckForUpdate)
                CheckForAutoUpdate();

            ProfileData.SetActiveProfile(AppSettingData.ApplicationSetting.SystemSetting.LastUsedProfileId);     // Load last used profile

            Task.Run(() => _flightSimOrchestrator.StartSimConnectServer());                                      // Start the SimConnect server

            _keyboardOrchestrator.Initialize();
        }

        public void ApplicationClose()
        {
            // Force unhook all win events 
            _panelConfigurationOrchestrator.EndConfiguration();
            _panelConfigurationOrchestrator.EndTouchHook();

            InputHookManager.EndKeyboardHookForced();
            _flightSimOrchestrator.EndSimConnectServer(true);
        }

        private void CheckForAutoUpdate()
        {
            var jsonPath = Path.Combine(Path.Combine(FileIo.GetUserDataFilePath(), "autoupdate.json"));
            AutoUpdater.PersistenceProvider = new JsonFilePersistenceProvider(jsonPath);
            AutoUpdater.Synchronous = true;
            AutoUpdater.AppTitle = "MSFS Pop Out Panel Manager";
            AutoUpdater.RunUpdateAsAdmin = false;
            AutoUpdater.UpdateFormSize = new System.Drawing.Size(1024, 660);
            AutoUpdater.Start(AppSettingData.ApplicationSetting.SystemSetting.AutoUpdaterUrl);
        }
    }
}
