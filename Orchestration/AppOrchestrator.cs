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
            if (AppSettingData.ApplicationSetting.GeneralSetting.CheckForUpdate)
                CheckForAutoUpdate();

            ProfileData.SetActiveProfile(AppSettingData.ApplicationSetting.SystemSetting.LastUsedProfileId);     // Load last used profile

            Task.Run(() => _flightSimOrchestrator.StartSimConnectServer());                                      // Start the SimConnect server

            _keyboardOrchestrator.Initialize();

            AppSettingData.ApplicationSetting.GeneralSetting.OnApplicationDataPathUpdated += (_, e) =>
            {
                AppSettingDataManager.MoveAppSettings(AppSettingData.ApplicationSetting);
                ProfileDataManager.MoveProfiles(ProfileData.Profiles, e);

                FileLogger.UseApplicationDataPath = e;

                try
                {
                    FileLogger.CloseFileLogger();
                    if (Directory.Exists(FileIo.GetUserDataFilePath(!e)))
                        Directory.Delete(FileIo.GetUserDataFilePath(!e), true);
                }
                catch
                {
                    FileLogger.WriteLog($"Unable to remove old POPM data folder. {FileIo.GetUserDataFilePath(!e)}", StatusMessageType.Error);
                }
            };
        }

        public void ApplicationClose()
        {
            // Force unhook all win events 
            _panelConfigurationOrchestrator.EndConfiguration();
            _panelConfigurationOrchestrator.EndTouchHook();

            InputHookManager.EndKeyboardHook();
            _keyboardOrchestrator.EndGlobalKeyboardHookForced();
            _flightSimOrchestrator.EndSimConnectServer(true);
        }

        private void CheckForAutoUpdate()
        {
            var jsonPath = Path.Combine(Path.Combine(FileIo.GetUserDataFilePath(AppSettingData.ApplicationSetting.GeneralSetting.UseApplicationDataPath), "autoupdate.json"));
            AutoUpdater.PersistenceProvider = new JsonFilePersistenceProvider(jsonPath);
            AutoUpdater.Synchronous = true;
            AutoUpdater.AppTitle = "MSFS Pop Out Panel Manager";
            AutoUpdater.RunUpdateAsAdmin = false;
            AutoUpdater.UpdateFormSize = new System.Drawing.Size(1024, 660);
            AutoUpdater.Start(AppSettingData.ApplicationSetting.SystemSetting.AutoUpdaterUrl);
        }
    }
}
