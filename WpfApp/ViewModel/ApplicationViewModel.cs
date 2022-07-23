using MSFSPopoutPanelManager.Orchestration;
using MSFSPopoutPanelManager.Shared;
using MSFSPopoutPanelManager.WindowsAgent;
using Prism.Commands;
using PropertyChanged;
using System;
using System.Threading.Tasks;
using System.Windows;

namespace MSFSPopoutPanelManager.WpfApp.ViewModel
{
    [SuppressPropertyChangedWarnings]
    public class ApplicationViewModel : ObservableObject
    {
        private MainOrchestrator _orchestrator;
        private OrchestratorHelper _windowHelper;

        public event EventHandler<StatusMessageEventArg> ShowContextMenuBalloonTip;

        public PanelSelectionViewModel PanelSelectionViewModel { get; private set; }

        public PanelConfigurationViewModel PanelConfigurationViewModel { get; private set; }

        public PreferencesViewModel PreferencesViewModel { get; private set; }

        public ProfileData ProfileData { get { return _orchestrator.ProfileData; } }

        public AppSettingData AppSettingData { get { return _orchestrator.AppSettingData; } }

        public FlightSimData FlightSimData { get { return _orchestrator.FlightSimData; } }

        public IntPtr ApplicationHandle
        {
            get { return _orchestrator.ApplicationHandle; }
            set { _orchestrator.ApplicationHandle = value; }
        }

        public Window ApplicationWindow { set { _windowHelper.ApplicationWindow = value; } }

        public string ApplicationVersion { get; private set; }

        public WindowState InitialWindowState { get; private set; }

        public string StatusMessage { get; set; }

        public StatusMessageType StatusMessageType { get; set; }

        public bool IsShownPanelConfigurationScreen { get; set; }

        public bool IsShownPanelSelectionScreen { get; set; }

        public DelegateCommand RestartCommand => new DelegateCommand(OnRestart, () => IsShownPanelConfigurationScreen);

        public DelegateCommand EditPreferencesCommand => new DelegateCommand(OnEditPreferences);

        public DelegateCommand ExitCommand => new DelegateCommand(async () => { await OnExit(); });

        public DelegateCommand UserGuideCommand => new DelegateCommand(OnOpenUserGuide);

        public DelegateCommand DownloadLatestReleaseCommand => new DelegateCommand(OnDownloadLastest);

        public DelegateCommand<object> ChangeProfileCommand => new DelegateCommand<object>(OnChangeProfile);

        public DelegateCommand StartPopOutCommand => new DelegateCommand(OnStartPopOut, () => ProfileData.ActiveProfile != null && ProfileData.ActiveProfile.PanelSourceCoordinates.Count > 0)
                                                                        .ObservesProperty(() => ProfileData.ActiveProfile)
                                                                        .ObservesProperty(() => ProfileData.ActiveProfile.PanelSourceCoordinates.Count);

        public ApplicationViewModel()
        {
            _orchestrator = new MainOrchestrator();
            _windowHelper = new OrchestratorHelper(_orchestrator);

            PanelSelectionViewModel = new PanelSelectionViewModel(_orchestrator);
            PanelConfigurationViewModel = new PanelConfigurationViewModel(_orchestrator);
            PreferencesViewModel = new PreferencesViewModel(_orchestrator);
        }

        public void Initialize()
        {
            StatusMessageWriter.OnStatusMessage += HandleOnStatusMessage;

            _orchestrator.Initialize();
            _orchestrator.AppSettingData.AlwaysOnTopChanged += (sender, e) => WindowActionManager.ApplyAlwaysOnTop(ApplicationHandle, PanelType.PopOutManager, e);
            _orchestrator.FlightSim.OnSimulatorStarted += (sender, e) => ShowPanelSelection(true);
            _orchestrator.FlightSim.OnSimulatorStopped += (sender, e) => OnRestart();
            _orchestrator.FlightSim.OnFlightStopped += (sender, e) => OnRestart();
            _orchestrator.PanelPopOut.OnPopOutStarted += (sender, e) => ShowPanelSelection(true);
            _orchestrator.PanelPopOut.OnPopOutCompleted += (sender, e) => ShowPanelSelection(false);

            // Set application version
            ApplicationVersion = WindowProcessManager.GetApplicationVersion();

            // Set window state
            if (_orchestrator.AppSettingData.AppSetting.StartMinimized)
                InitialWindowState = WindowState.Minimized;

            // Set Always on Top
            if (_orchestrator.AppSettingData.AppSetting.AlwaysOnTop)
                WindowActionManager.ApplyAlwaysOnTop(ApplicationHandle, PanelType.PopOutManager, _orchestrator.AppSettingData.AppSetting.AlwaysOnTop);

            ShowPanelSelection(true);
        }

        private void HandleOnStatusMessage(object sender, StatusMessageEventArg e)
        {
            StatusMessage = e.Message;
            StatusMessageType = e.StatusMessageType;

            if (StatusMessageType == StatusMessageType.Error)
                ShowContextMenuBalloonTip?.Invoke(this, e);
        }

        public async Task WindowClosing()
        {
            await _orchestrator.ApplicationClose();

            if (Application.Current != null)
                Environment.Exit(0);
        }

        private void ShowPanelSelection(bool show)
        {
            if (show)
            {
                IsShownPanelSelectionScreen = true;
                IsShownPanelConfigurationScreen = false;
            }
            else
            {
                IsShownPanelSelectionScreen = false;
                IsShownPanelConfigurationScreen = true;
            }
        }

        #region Menu bar and context menu commands

        private void OnRestart()
        {
            // End panel configuration
            _orchestrator.PanelConfiguration.EndConfiguration();

            // Clear log
            StatusMessageWriter.WriteMessage(string.Empty, StatusMessageType.Info, false);

            // Try to close all Custom Panel window
            WindowActionManager.CloseAllPopOuts();

            ShowPanelSelection(true);
        }

        private void OnEditPreferences()
        {
            DialogHelper.PreferencesDialog(PreferencesViewModel);
        }

        private async Task OnExit()
        {
            await WindowClosing();
        }

        private void OnOpenUserGuide()
        {
            _orchestrator.OnlineFeature.OpenUserGuide();
        }

        private void OnDownloadLastest()
        {
            _orchestrator.OnlineFeature.OpenLatestDownload();
        }

        private void OnChangeProfile(object commandParameter)
        {
            if (commandParameter == null)
                return;

            var profileId = Convert.ToInt32(commandParameter);

            ProfileData.UpdateActiveProfile(profileId);
        }

        private void OnStartPopOut()
        {
            ShowPanelSelection(true);
            PanelSelectionViewModel.StartPopOutCommand.Execute();
        }

        #endregion
    }
}