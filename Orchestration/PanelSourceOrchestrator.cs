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
        private ProfileData _profileData;
        private AppSettingData _appSettingData;
        private FlightSimData _flightSimData;
        private int _prePanelConfigurationCockpitZoomLevel = 50;

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
        public event EventHandler<PanelConfig> OnOverlayRemoved;
        public event EventHandler OnPanelSourceSelectionStarted;
        public event EventHandler OnPanelSourceSelectionCompleted;

        public void StartPanelSelectionEvent()
        {
            OnPanelSourceSelectionStarted?.Invoke(this, null);
        }

        public void StartPanelSelection(PanelConfig panelConfig)
        {
            _profileData.ActiveProfile.IsEditingPanelSource = true;

            InputHookManager.OnLeftClick += (sender, e) => HandleOnPanelSelectionAdded(panelConfig, e);
            InputHookManager.StartMouseHook();
        }

        public async Task StartEditPanelSources()
        {
            await Task.Run(() =>
            {
                // Set Windowed Display Mode window's configuration if needed
                if (_appSettingData.ApplicationSetting.WindowedModeSetting.AutoResizeMsfsGameWindow)
                    WindowActionManager.SetMsfsGameWindowLocation(ActiveProfile.MsfsGameWindowConfig);

                if (AppSetting.PopOutSetting.AutoPanning.IsEnabled)
                {
                    _prePanelConfigurationCockpitZoomLevel = _flightSimData.CockpitCameraZoom;
                    LoadCustomView(AppSetting.PopOutSetting.AutoPanning.KeyBinding);
                    SetCockpitZoomLevel(50);
                    WindowActionManager.BringWindowToForeground(ApplicationHandle);
                }
            });

            foreach (var panel in _profileData.ActiveProfile.PanelConfigs)
            {
                if (panel.HasPanelSource)
                    OnOverlayShowed?.Invoke(this, panel);
            }

            // Turn off TrackIR if TrackIR is started
            FlightSimOrchestrator.TurnOffTrackIR();
        }

        public async Task EndEditPanelSources()
        {
            foreach (var panel in _profileData.ActiveProfile.PanelConfigs)
            {
                OnOverlayRemoved?.Invoke(this, panel);
            }

            // Save last auto panning camera angle
            if (AppSetting.PopOutSetting.AutoPanning.IsEnabled)
            {
                // !!! Fix MSFS bug that without setting zoom, everything will be off by few pixels
                FlightSimOrchestrator.SetCockpitCameraZoomLevel(_flightSimData.CockpitCameraZoom);

                // If using windows mode, save MSFS game window configuration
                if (_appSettingData.ApplicationSetting.WindowedModeSetting.AutoResizeMsfsGameWindow)
                    _profileData.SaveMsfsGameWindowConfig();

                InputEmulationManager.SaveCustomView(AppSetting.PopOutSetting.AutoPanning.KeyBinding);
            }

            await Task.Run(() =>
            {
                Thread.Sleep(500);  // wait for custom view save to be completed

                // Recenter game or return to after pop out camera view
                if (!AppSetting.PopOutSetting.AfterPopOutCameraView.IsEnabled)
                {
                    FlightSimOrchestrator.ResetCameraView();
                    SetCockpitZoomLevel(_prePanelConfigurationCockpitZoomLevel);
                }
                else
                {
                    switch (AppSetting.PopOutSetting.AfterPopOutCameraView.CameraView)
                    {
                        case AfterPopOutCameraViewType.CockpitCenterView:
                            FlightSimOrchestrator.ResetCameraView();
                            SetCockpitZoomLevel(_prePanelConfigurationCockpitZoomLevel);
                            break;
                        case AfterPopOutCameraViewType.CustomCameraView:
                            LoadCustomView(AppSetting.PopOutSetting.AfterPopOutCameraView.KeyBinding);
                            SetCockpitZoomLevel(_prePanelConfigurationCockpitZoomLevel);
                            break;
                    }
                }

                WindowActionManager.BringWindowToForeground(ApplicationHandle);

                // Turn TrackIR back on
                FlightSimOrchestrator.TurnOnTrackIR();
            });
        }

        public void ShowPanelSourceNonEdit(PanelConfig panel)
        {
            if (panel.HasPanelSource)
                OnOverlayShowed?.Invoke(this, panel);
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
            int retry = 20;
            for (var i = 0; i < retry; i++)
            {
                InputEmulationManager.LoadCustomView(keybinding);
                Thread.Sleep(750);  // wait for flightsimdata to be updated
                if (_flightSimData.CameraViewTypeAndIndex1 == 7)    // 7 = custom camera view enum
                    break;
            }
        }

        private void SetCockpitZoomLevel(int zoom)
        {
            int retry = 10;
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
