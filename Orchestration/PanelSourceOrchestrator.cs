using MSFSPopoutPanelManager.DomainModel.Profile;
using MSFSPopoutPanelManager.DomainModel.Setting;
using MSFSPopoutPanelManager.Shared;
using MSFSPopoutPanelManager.WindowsAgent;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using Point = System.Drawing.Point;

namespace MSFSPopoutPanelManager.Orchestration
{
    public class PanelSourceOrchestrator : BaseOrchestrator
    {
        private const int CAMERA_VIEW_HOME_COCKPIT_MODE = 8;
        private const int CAMERA_VIEW_CUSTOM_CAMERA = 7;

        private readonly FlightSimOrchestrator _flightSimOrchestrator;
        private bool _isEditingPanelSourceLock;

        public event EventHandler OnStatusMessageStarted;
        public event EventHandler OnStatusMessageEnded;

        public PanelSourceOrchestrator(SharedStorage sharedStorage, FlightSimOrchestrator flightSimOrchestrator) : base(sharedStorage)
        {
            _flightSimOrchestrator = flightSimOrchestrator;

            ProfileData.OnActiveProfileChanged += (_, _) => { CloseAllPanelSource(); };

            flightSimOrchestrator.OnFlightStopped += (_, _) => { CloseAllPanelSource(); };
        }

        internal IntPtr ApplicationHandle { get; set; }

        private UserProfile ActiveProfile => ProfileData?.ActiveProfile;

        private ApplicationSetting AppSetting => AppSettingData?.ApplicationSetting;

        public event EventHandler<PanelConfig> OnOverlayShowed;
        public event EventHandler<PanelConfig> OnNonEditOverlayShowed;
        public event EventHandler<PanelConfig> OnOverlayRemoved;

        public void StartPanelSelectionEvent()
        {
            if (ActiveProfile.IsSelectingPanelSource)
                return;

            ActiveProfile.IsSelectingPanelSource = true;
        }

        public void StartPanelSelection(PanelConfig panelConfig)
        {
            // ReSharper disable once EventUnsubscriptionViaAnonymousDelegate
            InputHookManager.OnLeftClick -= (_, e) => HandleOnPanelSelectionAdded(panelConfig, e);
            InputHookManager.OnLeftClick += (_, e) => HandleOnPanelSelectionAdded(panelConfig, e);
            InputHookManager.StartMouseHook();
        }

        public async Task StartEditPanelSources()
        {
            if (_isEditingPanelSourceLock)
                return;

            _isEditingPanelSourceLock = true;

            // Turn off TrackIR if TrackIR is started
            _flightSimOrchestrator.TurnOffTrackIR();

            if (ActiveProfile.IsUsedLegacyCameraSystem)
                await StartEditPanelSourcesLegacyCamera();
            else
                await StartEditPanelSourcesFixedCamera();
        }

        public async Task EndEditPanelSources()
        {
            if (!_isEditingPanelSourceLock)
                return;
            
            if (ActiveProfile.IsUsedLegacyCameraSystem)
                EndEditPanelSourcesLegacyCamera();
            else
                EndEditPanelSourcesFixedCamera();

            _isEditingPanelSourceLock = false;

            foreach (var panelConfig in ProfileData.ActiveProfile.PanelConfigs)
            {
                panelConfig.IsEditingPanel = false;
                panelConfig.IsSelectedPanelSource = false;
                OnOverlayRemoved?.Invoke(this, panelConfig);
            }

            ActiveProfile.IsSelectingPanelSource = false;

            await Task.Run(() =>
            {
                WindowActionManager.BringWindowToForeground(ApplicationHandle);

                // Turn TrackIR back on
                _flightSimOrchestrator.TurnOnTrackIR();
            });

            // End all mouse hook if active
            InputHookManager.EndMouseHook();
        }

        public void ShowPanelSourceForEdit(PanelConfig panel)
        {
            foreach (var panelConfig in ActiveProfile.PanelConfigs)
            {
                OnOverlayRemoved?.Invoke(this, panelConfig);
                panelConfig.IsEditingPanel = false;
            }

            panel.IsEditingPanel = true;

            if (panel.HasPanelSource)
                OnOverlayShowed?.Invoke(this, panel);
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

        public void SetCamera(PanelConfig panel)
        {
            if (!FlightSimData.IsInCockpit)
                return;

            if (panel.FixedCameraConfig == null)
                return;
            
            Task.Run(() =>
            {
                if (panel.FixedCameraConfig.CameraType == CameraType.Cockpit)
                {
                    _flightSimOrchestrator.ResetCameraView();
                    Thread.Sleep(250);
                }

                _flightSimOrchestrator.SetFixedCamera(panel.FixedCameraConfig.CameraType, panel.FixedCameraConfig.CameraIndex);
            });
            Thread.Sleep(250);
        }

        public void HandleOnPanelSelectionAdded(PanelConfig panelConfig, Point e)
        {
            if (WindowActionManager.IsPointInsideAppWindow(e))
                return;

            InputHookManager.EndMouseHook();

            if (ActiveProfile == null)
                return;

            panelConfig.PanelSource.X = e.X;
            panelConfig.PanelSource.Y = e.Y;

            ProfileData.WriteProfiles();

            // Show source circle on screen
            OnOverlayShowed?.Invoke(this, panelConfig);

            // If using windows mode, save MSFS game window configuration
            if (AppSettingData.ApplicationSetting.WindowedModeSetting.AutoResizeMsfsGameWindow)
                ProfileData.SaveMsfsGameWindowConfig();

            panelConfig.IsSelectedPanelSource = false;

            ActiveProfile.IsSelectingPanelSource = false;
        }

        public void RemovePanelSource(PanelConfig panelConfig)
        {
            // Disable hooks if active
            InputHookManager.EndMouseHook();

            ProfileData.ActiveProfile.CurrentMoveResizePanelId = Guid.Empty;

            OnOverlayRemoved?.Invoke(this, panelConfig);

            ProfileData.ActiveProfile.PanelConfigs.Remove(panelConfig);
        }

        private void LoadCustomView(string keyBinding)
        {
            int retry = 3;
            for (var i = 0; i < retry; i++)
            {
                InputEmulationManager.LoadCustomView(keyBinding);
                Thread.Sleep(750);  // wait for flightsimdata to be updated
                if (FlightSimData.CameraViewTypeAndIndex1 == CAMERA_VIEW_CUSTOM_CAMERA)    // custom camera view enum
                    break;
            }
        }

        private void SetCockpitZoomLevel(int zoom)
        {
            var retry = 3;
            for (var i = 0; i < retry; i++)
            {
                _flightSimOrchestrator.SetCockpitCameraZoomLevel(zoom);
                Thread.Sleep(750);  // wait for flightsimdata to be updated

                if (FlightSimData.CockpitCameraZoom == zoom)
                    break;
            }
        }

        private async Task StartEditPanelSourcesLegacyCamera()
        {
            await Task.Run(() =>
            {
                OnStatusMessageStarted?.Invoke(this, EventArgs.Empty);
                StatusMessageWriter.IsEnabled = true;
                StatusMessageWriter.ClearMessage();
                StatusMessageWriter.WriteMessage("Loading camera view. Please wait......", StatusMessageType.Info);
                
                // Set Windowed Display Mode window's configuration if needed
                if (AppSettingData.ApplicationSetting.WindowedModeSetting.AutoResizeMsfsGameWindow)
                    WindowActionManager.SetMsfsGameWindowLocation(ActiveProfile.MsfsGameWindowConfig);

                if (AppSetting.PopOutSetting.AutoPanning.IsEnabled)
                {
                    if (FlightSimData.CameraViewTypeAndIndex1 == CAMERA_VIEW_HOME_COCKPIT_MODE)
                    {
                        _flightSimOrchestrator.SetCockpitCameraZoomLevel(ProfileData.ActiveProfile.PanelSourceCockpitZoomFactor);
                    }
                    else
                    {
                        LoadCustomView(AppSetting.PopOutSetting.AutoPanning.KeyBinding);
                        _flightSimOrchestrator.SetCockpitCameraZoomLevel(50);
                    }

                    WindowActionManager.BringWindowToForeground(ApplicationHandle);
                }

                foreach (var panel in ProfileData.ActiveProfile.PanelConfigs)
                {
                    if (panel.HasPanelSource)
                        OnOverlayShowed?.Invoke(this, panel);
                }

                Thread.Sleep(500);
                StatusMessageWriter.IsEnabled = false;
                OnStatusMessageEnded?.Invoke(this, EventArgs.Empty);
            });
        }

        private Task StartEditPanelSourcesFixedCamera()
        {
            return Task.CompletedTask;
        }

        private void EndEditPanelSourcesLegacyCamera()
        {
            // Save last auto panning camera angle
            if (AppSetting.PopOutSetting.AutoPanning.IsEnabled)
            {
                // If using windows mode, save MSFS game window configuration
                if (AppSettingData.ApplicationSetting.WindowedModeSetting.AutoResizeMsfsGameWindow)
                    ProfileData.SaveMsfsGameWindowConfig();

                if (FlightSimData.CameraViewTypeAndIndex1 == CAMERA_VIEW_HOME_COCKPIT_MODE)
                {
                    ProfileData.ActiveProfile.PanelSourceCockpitZoomFactor = FlightSimData.CockpitCameraZoom;
                }
                else
                {
                    // !!! Fix MSFS issue that without setting zoom, everything will be off by few pixels at a time
                    SetCockpitZoomLevel(FlightSimData.CockpitCameraZoom);
                    InputEmulationManager.SaveCustomView(AppSetting.PopOutSetting.AutoPanning.KeyBinding);
                }
            }

            Thread.Sleep(500);  // wait for custom view save to be completed
        }

        private void EndEditPanelSourcesFixedCamera()
        {
            _flightSimOrchestrator.ResetCameraView();
        }

        public ObservableCollection<FixedCameraConfig> GetFixedCameraConfigs()
        {
            var configs = new List<FixedCameraConfig>
            {
                new() { Id = 0, Name = "Cockpit Pilot", CameraType = CameraType.Cockpit, CameraIndex = 1 },
                new() { Id = 1, Name = "Cockpit Copilot", CameraType = CameraType.Cockpit, CameraIndex = 5 }
            };

            for (var i = 0; i < FlightSimData.CameraViewTypeAndIndex2Max; i++)
            {
                var item = new FixedCameraConfig
                {
                    Id = i + 2,
                    Name = $"Instrument {i + 1}",
                    CameraType = CameraType.Instrument,
                    CameraIndex = i
                };
                configs.Add(item);
            }

            return new ObservableCollection<FixedCameraConfig>(configs);
        }
    }
}
