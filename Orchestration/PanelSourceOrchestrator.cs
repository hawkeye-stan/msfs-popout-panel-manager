using MSFSPopoutPanelManager.DomainModel.Profile;
using MSFSPopoutPanelManager.DomainModel.Setting;
using MSFSPopoutPanelManager.Shared;
using MSFSPopoutPanelManager.WindowsAgent;
using System;
using System.Threading;
using System.Threading.Tasks;
using Point = System.Drawing.Point;

namespace MSFSPopoutPanelManager.Orchestration
{
    public class PanelSourceOrchestrator : ObservableObject
    {
        private const int CAMERA_VIEW_HOME_COCKPIT_MODE = 8;
        private const int CAMERA_VIEW_CUSTOM_CAMERA = 7;

        private ProfileData _profileData;
        private AppSettingData _appSettingData;
        private FlightSimData _flightSimData;
        private bool _isEditingPanelSourceLock = false;

        public PanelSourceOrchestrator(ProfileData profileData, AppSettingData appSettingData, FlightSimData flightSimData)
        {
            _profileData = profileData;
            _appSettingData = appSettingData;
            _flightSimData = flightSimData;

            _profileData.ActiveProfileChanged += (sender, e) => { CloseAllPanelSource(); };
        }

        internal FlightSimOrchestrator FlightSimOrchestrator { get; set; }

        internal IntPtr ApplicationHandle { get; set; }

        private UserProfile ActiveProfile { get { return _profileData == null ? null : _profileData.ActiveProfile; } }

        private ApplicationSetting AppSetting { get { return _appSettingData == null ? null : _appSettingData.ApplicationSetting; } }

        public event EventHandler<PanelConfig> OnOverlayShowed;
        public event EventHandler<PanelConfig> OnNonEditOverlayShowed;
        public event EventHandler<PanelConfig> OnOverlayRemoved;
        public event EventHandler OnPanelSourceSelectionStarted;
        public event EventHandler OnPanelSourceSelectionCompleted;

        public void StartPanelSelectionEvent()
        {
            OnPanelSourceSelectionStarted?.Invoke(this, null);
        }

        public void StartPanelSelection(PanelConfig panelConfig)
        {
            ActiveProfile.IsEditingPanelSource = true;

            InputHookManager.OnLeftClick += (sender, e) => HandleOnPanelSelectionAdded(panelConfig, e);
            InputHookManager.StartMouseHook();
        }

        public async Task StartEditPanelSources()
        {
            if (_isEditingPanelSourceLock)
                return;

            _isEditingPanelSourceLock = true;

            await Task.Run(() =>
            {
                // Set Windowed Display Mode window's configuration if needed
                if (_appSettingData.ApplicationSetting.WindowedModeSetting.AutoResizeMsfsGameWindow)
                    WindowActionManager.SetMsfsGameWindowLocation(ActiveProfile.MsfsGameWindowConfig);

                if (AppSetting.PopOutSetting.AutoPanning.IsEnabled)
                {
                    if(_flightSimData.CameraViewTypeAndIndex1 == CAMERA_VIEW_HOME_COCKPIT_MODE)
                    {
                        FlightSimOrchestrator.SetCockpitCameraZoomLevel(_profileData.ActiveProfile.PanelSourceCockpitZoomFactor);
                    }
                    else
                    {
                        LoadCustomView(AppSetting.PopOutSetting.AutoPanning.KeyBinding);
                        FlightSimOrchestrator.SetCockpitCameraZoomLevel(50);
                    }
                    
                    WindowActionManager.BringWindowToForeground(ApplicationHandle);
                }

                foreach (var panel in _profileData.ActiveProfile.PanelConfigs)
                {
                    if (panel.HasPanelSource)
                        OnOverlayShowed?.Invoke(this, panel);
                }

                // Turn off TrackIR if TrackIR is started
                FlightSimOrchestrator.TurnOffTrackIR();
            });
        }

        public async Task EndEditPanelSources()
        {
            if (!_isEditingPanelSourceLock)
                return;

            // Save last auto panning camera angle
            if (AppSetting.PopOutSetting.AutoPanning.IsEnabled)
            {
                // If using windows mode, save MSFS game window configuration
                if (_appSettingData.ApplicationSetting.WindowedModeSetting.AutoResizeMsfsGameWindow)
                    _profileData.SaveMsfsGameWindowConfig();

                if(_flightSimData.CameraViewTypeAndIndex1 == CAMERA_VIEW_HOME_COCKPIT_MODE)
                {
                    _profileData.ActiveProfile.PanelSourceCockpitZoomFactor = _flightSimData.CockpitCameraZoom;
                }
                else
                {
                    // !!! Fix MSFS bug that without setting zoom, everything will be off by few pixels at a time
                    SetCockpitZoomLevel(_flightSimData.CockpitCameraZoom);
                    InputEmulationManager.SaveCustomView(AppSetting.PopOutSetting.AutoPanning.KeyBinding);
                }
            }

            _isEditingPanelSourceLock = false;

            foreach (var panel in _profileData.ActiveProfile.PanelConfigs)
            {
                OnOverlayRemoved?.Invoke(this, panel);
            }

            await Task.Run(() =>
            {
                Thread.Sleep(500);  // wait for custom view save to be completed

                FlightSimOrchestrator.ResetCameraView();
                WindowActionManager.BringWindowToForeground(ApplicationHandle);

                // Turn TrackIR back on
                FlightSimOrchestrator.TurnOnTrackIR();
            });
        }

        public void ShowPanelSourceNonEdit(PanelConfig panel)
        {
            if (panel.HasPanelSource)
                OnNonEditOverlayShowed?.Invoke(this, panel);
        }

        public void ClosePanelSourceNonEdit(PanelConfig panel)
        {
            OnOverlayRemoved?.Invoke(this, panel);
        }

        public void CloseAllPanelSource()
        {
            if (ActiveProfile != null)
            {
                ActiveProfile.IsEditingPanelSource = false;
                _isEditingPanelSourceLock = false;

                foreach (var panelConfig in ActiveProfile.PanelConfigs)
                    OnOverlayRemoved?.Invoke(this, panelConfig);
            }
        }

        public void HandleOnPanelSelectionAdded(PanelConfig panelConfig, Point e)
        {
            OnPanelSourceSelectionCompleted?.Invoke(this, null);

            InputHookManager.EndMouseHook();

            if (ActiveProfile == null)
                return;

            panelConfig.PanelSource.X = e.X;
            panelConfig.PanelSource.Y = e.Y;

            _profileData.WriteProfiles();

            // Show source circle on screen
            OnOverlayShowed?.Invoke(this, panelConfig);

            // If using windows mode, save MSFS game window configuration
            if (_appSettingData.ApplicationSetting.WindowedModeSetting.AutoResizeMsfsGameWindow)
                _profileData.SaveMsfsGameWindowConfig();

            panelConfig.IsSelectedPanelSource = false;
        }

        public void RemovePanelSource(PanelConfig panelConfig)
        {
            // Disable hooks if active
            InputHookManager.EndMouseHook();

            _profileData.ActiveProfile.CurrentMoveResizePanelId = Guid.Empty;

            OnOverlayRemoved?.Invoke(this, panelConfig);

            _profileData.ActiveProfile.PanelConfigs.Remove(panelConfig);
        }

        private void LoadCustomView(string keybinding)
        {
            int retry = 3;
            for (var i = 0; i < retry; i++)
            {
                InputEmulationManager.LoadCustomView(keybinding);
                Thread.Sleep(750);  // wait for flightsimdata to be updated
                if (_flightSimData.CameraViewTypeAndIndex1 == CAMERA_VIEW_CUSTOM_CAMERA)    // custom camera view enum
                    break;
            }
        }

        private void SetCockpitZoomLevel(int zoom)
        {
            int retry = 3;
            for (var i = 0; i < retry; i++)
            {
                FlightSimOrchestrator.SetCockpitCameraZoomLevel(zoom);
                Thread.Sleep(750);  // wait for flightsimdata to be updated

                if (_flightSimData.CockpitCameraZoom == zoom)
                    break;
            }
        }
    }
}
