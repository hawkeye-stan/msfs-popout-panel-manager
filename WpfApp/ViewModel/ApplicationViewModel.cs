using AutoUpdaterDotNET;
using CommunityToolkit.Mvvm.ComponentModel;
using MSFSPopoutPanelManager.Model;
using MSFSPopoutPanelManager.Provider;
using MSFSPopoutPanelManager.Shared;
using Newtonsoft.Json;
using System;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace MSFSPopoutPanelManager.WpfApp.ViewModel
{
    public class ApplicationViewModel : ObservableObject
    {
        private UserProfileManager _userProfileManager;
        private PanelPopOutManager _panelPopoutManager;
        private SimConnectManager _simConnectManager;

        public event EventHandler<EventArgs<StatusMessage>> ShowContextMenuBalloonTip;

        public PanelSelectionViewModel PanelSelectionViewModel { get; private set; }

        public PanelConfigurationViewModel PanelConfigurationViewModel { get; private set; }
        public PreferencesViewModel PreferencesViewModel { get; private set; }

        public TouchPanelManagementViewModel TouchPanelManagementViewModel { get; private set; }

        public DataStore DataStore { get; set; }

        public int ActiveUserProfileId { get; set; }

        public string ApplicationVersion { get; private set; }

        public WindowState InitialWindowState { get; private set; }

        public StatusMessage StatusMessage { get; set; }

        public bool IsShownPanelConfigurationScreen { get; set; }

        public bool IsShownPanelSelectionScreen { get; set; }

        public bool IsMinimizedAllPanels { get; set; }

        public IntPtr WindowHandle { get; set; }

        public DelegateCommand RestartCommand => new DelegateCommand(OnRestart, CanExecute);
        public DelegateCommand MinimizeAllPanelsCommand => new DelegateCommand(OnMinimizeAllPanels, CanExecute);
        public DelegateCommand ExitCommand => new DelegateCommand(OnExit, CanExecute);
        public DelegateCommand UserGuideCommand => new DelegateCommand((o) => { DiagnosticManager.OpenOnlineUserGuide(); }, CanExecute);
        public DelegateCommand DownloadLatestReleaseCommand => new DelegateCommand((o) => { DiagnosticManager.OpenOnlineLatestDownload(); }, CanExecute);
        public DelegateCommand UserProfileSelectCommand => new DelegateCommand(OnUserProfileSelected, CanExecute);
        public DelegateCommand ShowPanelCoorOverlayCommand => new DelegateCommand(OnShowPanelCoorOverlay, CanExecute);
        public DelegateCommand StartPopOutCommand => new DelegateCommand(OnStartPopOut, CanExecute);

        public ApplicationViewModel()
        {
            Logger.OnStatusLogged += (sender, e) => { OnStatusMessageLogged(e); };

            DataStore = new DataStore();

            _userProfileManager = new UserProfileManager();

            _simConnectManager = new SimConnectManager();
            _simConnectManager.OnSimConnectDataRefreshed += (sender, e) =>
            {
                // Automatic switching of active profile when SimConnect active aircraft livery changes
                if (DataStore.CurrentMsfsPlaneTitle != e.Value.Title)
                {
                    DataStore.CurrentMsfsPlaneTitle = e.Value.Title;
                    AutoSwitchProfile(e.Value.Title);
                }

                DataStore.ElectricalMasterBatteryStatus = e.Value.ElectricalMasterBattery;
            };
            _simConnectManager.OnConnected += (sender, e) => { DataStore.IsSimulatorStarted = true; };
            _simConnectManager.OnDisconnected += (sender, e) => { DataStore.IsSimulatorStarted = false; };

            _panelPopoutManager = new PanelPopOutManager(_userProfileManager, _simConnectManager);
            _panelPopoutManager.OnPopOutCompleted += (sender, e) =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    if (e.Value)
                    {
                        var messageDialog = new OnScreenMessageDialog("Panels have been popped out succesfully.", MessageIcon.Success);
                        messageDialog.ShowDialog();
                    }
                    else
                    {
                        var messageDialog = new OnScreenMessageDialog("Unable to pop out panels.\nPlease see error message in the app.", MessageIcon.Failed);
                        messageDialog.ShowDialog();
                    }
                });
            };
            _panelPopoutManager.OnTouchPanelOpened += (sender, e) =>
            {
                TouchPanelWebViewDialog window = new TouchPanelWebViewDialog(e.Value.PlaneId, e.Value.PanelId, e.Value.Caption, e.Value.Width, e.Value.Height);
                window.Show();
            };

            PanelSelectionViewModel = new PanelSelectionViewModel(DataStore, _userProfileManager, _panelPopoutManager, _simConnectManager);
            PanelSelectionViewModel.OnPopOutCompleted += (sender, e) => { ShowPanelSelection(false); PanelConfigurationViewModel.Initialize(); };
            PanelSelectionViewModel.OnUserProfileChanged += (sender, e) => { ReloadTouchPanelSimConnectDataDefinition(this, null); };
            PanelSelectionViewModel.TouchPanelBindingViewModel.OnBindingUpdated += ReloadTouchPanelSimConnectDataDefinition;
            PanelConfigurationViewModel = new PanelConfigurationViewModel(DataStore, _userProfileManager);

            PreferencesViewModel = new PreferencesViewModel(DataStore);
            TouchPanelManagementViewModel = new TouchPanelManagementViewModel(DataStore);

            InputHookManager.OnStartPopout += (source, e) => { OnStartPopOut(null); };
        }

        public void Initialize()
        {
            var appSetting = new AppSetting();
            appSetting.Load();

            DataStore.AppSetting = appSetting;
            DataStore.AppSetting.AlwaysOnTopChanged += OnAlwaysOnTopChanged;
            DataStore.AppSetting.AutoPopOutPanelsChanged += (sender, e) =>
            {
                if (e.Value)
                    ActivateAutoPanelPopOut();
                else
                    DeativateAutoPanelPopOut();
            };

            CheckForAutoUpdate();

            // Set application version
            ApplicationVersion = DiagnosticManager.GetApplicationVersion();

            // Set window state
            if (DataStore.AppSetting.StartMinimized)
                InitialWindowState = WindowState.Minimized;

            // Set Always on Top
            if (DataStore.AppSetting.AlwaysOnTop)
                OnAlwaysOnTopChanged(this, new EventArgs<bool>(DataStore.AppSetting.AlwaysOnTop));

            // Activate auto pop out panels
            _simConnectManager.OnFlightStopped += HandleOnFlightStopped;
            if (DataStore.AppSetting.AutoPopOutPanels)
                ActivateAutoPanelPopOut();

            // Start touch panel server is specified
            if (DataStore.AppSetting.TouchPanelSettings.EnableIntegration && DataStore.AppSetting.TouchPanelSettings.AutoStart)
                TouchPanelManagementViewModel.StartServerCommand.Execute(null);

            ShowPanelSelection(true);

            IsMinimizedAllPanels = false;

            InputHookManager.StartHook();
        }

        public async void Exit()
        {
            // This method gets call on Windows_Closing
            InputHookManager.EndHook();
            _simConnectManager.Stop();

            // Stop touch panel server if started
            await Task.Run(() =>
            {
                TouchPanelManagementViewModel.StopServer();
            });
        }

        private void OnRestart(object commandParameter)
        {
            // Un-minimize all panels if applicable
            if (IsMinimizedAllPanels)
            {
                DataStore.AllowEdit = true;
                IsMinimizedAllPanels = false;
                WindowManager.MinimizeAllPopoutPanels(false);
            }

            // Unhook all win events 
            PanelConfigurationViewModel.UnhookWinEvents();

            // Clear logger
            Logger.ClearStatus();

            // Try to close all Cutome Panel window
            DataStore.ActiveUserProfile.PanelConfigs.ToList().FindAll(p => p.PanelType == PanelType.CustomPopout || p.PanelType == PanelType.MSFSTouchPanel).ForEach(panel => WindowManager.CloseWindow(panel.PanelHandle));

            // Clear all panel windows handle for active profile
            DataStore.ActiveUserProfile.PanelConfigs.ToList().ForEach(p => p.PanelHandle = IntPtr.Zero);
            ShowPanelSelection(true);
        }

        private void OnExit(object commandParameter)
        {
            if (Application.Current != null)
                Application.Current.Dispatcher.Invoke(() => Application.Current.Shutdown());
        }

        private void ShutDownApplication()
        {
            if (Application.Current != null)
                Application.Current.Shutdown();
        }

        private void OnMinimizeAllPanels(object commandParameter)
        {
            IsMinimizedAllPanels = !IsMinimizedAllPanels;
            if (IsMinimizedAllPanels)
            {
                DataStore.AllowEdit = false;
                WindowManager.MinimizeAllPopoutPanels(true);
                Logger.LogStatus("All pop out panels have been minimized. Panel configuration has been disabled.", StatusMessageType.Info);
            }
            else
            {
                DataStore.AllowEdit = true;
                WindowManager.MinimizeAllPopoutPanels(false);
                Logger.ClearStatus();
            }
        }

        private void OnUserProfileSelected(object commandParameter)
        {
            var profileId = Convert.ToInt32(commandParameter);

            if (profileId != DataStore.ActiveUserProfileId)
                DataStore.ActiveUserProfileId = profileId;
        }

        private void OnShowPanelCoorOverlay(object commandParameter)
        {
            PanelSelectionViewModel.IsEditingPanelCoorOverlay = !PanelSelectionViewModel.IsEditingPanelCoorOverlay;
            PanelSelectionViewModel.EditPanelCoorOverlayCommand.Execute(null);
        }

        private void OnStartPopOut(object commandParameter)
        {
            ShowPanelSelection(true);
            PanelSelectionViewModel.StartPopOutCommand.Execute(null);
        }

        private void OnAlwaysOnTopChanged(object sender, EventArgs<bool> e)
        {
            WindowManager.ApplyAlwaysOnTop(DataStore.ApplicationHandle, e.Value);
        }

        private void ShowPanelSelection(bool show)
        {
            IsShownPanelSelectionScreen = show;
            IsShownPanelConfigurationScreen = !show;
        }

        private void OnStatusMessageLogged(EventArgs<StatusMessage> e)
        {
            StatusMessage = e.Value;
            ShowContextMenuBalloonTip?.Invoke(this, new EventArgs<StatusMessage>(e.Value));
        }

        private bool CanExecute(object commandParameter)
        {
            return true;
        }

        private void ActivateAutoPanelPopOut()
        {
            _simConnectManager.OnFlightStarted += HandleOnFlightStarted;
        }

        private void DeativateAutoPanelPopOut()
        {
            DataStore.IsEnteredFlight = false;
            _simConnectManager.OnFlightStarted -= HandleOnFlightStarted;
        }

        private void HandleOnFlightStarted(object sender, EventArgs e)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                AutoSwitchProfile(DataStore.CurrentMsfsPlaneTitle);

                DataStore.IsEnteredFlight = true;
                ShowPanelSelection(true);

                // find the profile with the matching binding plane title
                var profile = DataStore.UserProfiles.FirstOrDefault(p => p.BindingAircraftLiveries.ToList().Exists(p => p == DataStore.CurrentMsfsPlaneTitle));

                if (profile == null)
                    return;

                if (profile.TouchPanelBindings.Count == 0 && profile.PanelSourceCoordinates.Count == 0)
                    return;

                var messageDialog = new OnScreenMessageDialog($"Automatic pop out is starting for profile:\n{profile.ProfileName}", DataStore.AppSetting.AutoPopOutPanelsWaitDelay.ReadyToFlyButton);      // Wait for the ready to fly button
                messageDialog.ShowDialog();
                InputEmulationManager.LeftClickReadyToFly();

                Thread.Sleep(DataStore.AppSetting.AutoPopOutPanelsWaitDelay.InitialCockpitView * 1000);         // Wait for the initial cockpit view

                // Turn on power if required to pop out panels
                _simConnectManager.TurnOnPower(profile.PowerOnRequiredForColdStart);
                Thread.Sleep(DataStore.AppSetting.AutoPopOutPanelsWaitDelay.InstrumentationPowerOn * 1000);     // Wait for battery to be turned on

                DataStore.ActiveUserProfileId = profile.ProfileId;
                _panelPopoutManager.UserProfile = profile;
                _panelPopoutManager.AppSetting = DataStore.AppSetting;

                if (profile.PanelSourceCoordinates.Count > 0)
                    _panelPopoutManager.StartPopout();
                else if (profile.TouchPanelBindings.Count > 0)
                    _panelPopoutManager.StartOpenTouchPanel();

                // Turn off power if needed after pop out
                _simConnectManager.TurnOffpower();
            });
        }

        private void HandleOnFlightStopped(object sender, EventArgs e)
        {
            DataStore.IsEnteredFlight = false;
            OnRestart(null);
        }

        private async void ReloadTouchPanelSimConnectDataDefinition(object sender, EventArgs e)
        {
            // Communicate with Touch Panel API server to reload SimConnect data definitions
            if (DataStore.ActiveUserProfile != null && DataStore.ActiveUserProfile.TouchPanelBindings.Count > 0 && TouchPanelManagementViewModel.IsServerStarted)
            {
                var planeId = DataStore.ActiveUserProfile.TouchPanelBindings.Count > 0 ?
                              DataStore.ActiveUserProfile.TouchPanelBindings[0].PlaneId :
                              null;

                using (var client = new HttpClient())
                {
                    dynamic data = new ExpandoObject();
                    data.PlaneId = planeId;
                    var payload = JsonConvert.SerializeObject(data);

                    var url = "http://localhost:27011/posttouchpanelloaded";

                    try
                    {
                        var response = await client.PostAsync(url, new StringContent(payload, Encoding.UTF8, "application/json"));
                        var token = response.Content.ReadAsStringAsync().Result;
                    }
                    catch { }
                }
            }
        }

        private void CheckForAutoUpdate()
        {
            string jsonPath = Path.Combine(Path.Combine(FileIo.GetUserDataFilePath(), "autoupdate.json"));
            AutoUpdater.PersistenceProvider = new JsonFilePersistenceProvider(jsonPath);
            AutoUpdater.Synchronous = true;
            AutoUpdater.AppTitle = "MSFS Pop Out Panel Manager";
            AutoUpdater.RunUpdateAsAdmin = false;
            AutoUpdater.UpdateFormSize = new System.Drawing.Size(930, 675);
            AutoUpdater.Start(DataStore.AppSetting.AutoUpdaterUrl);
        }

        private void AutoSwitchProfile(string activeAircraftTitle)
        {
            // Automatic switching of active profile when SimConnect active aircraft livery changes
            if (DataStore.UserProfiles != null)
            {
                var matchedProfile = DataStore.UserProfiles.ToList().Find(p => p.BindingAircraftLiveries.ToList().Exists(t => t == activeAircraftTitle));
                if (matchedProfile != null)
                    DataStore.ActiveUserProfileId = matchedProfile.ProfileId;
            }

            // Load Touch Panel SimConnect InformApiServer
            ReloadTouchPanelSimConnectDataDefinition(this, null);
        }
    }
}
