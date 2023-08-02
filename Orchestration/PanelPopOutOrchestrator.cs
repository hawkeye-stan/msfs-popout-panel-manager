using MSFSPopoutPanelManager.DomainModel.Profile;
using MSFSPopoutPanelManager.DomainModel.Setting;
using MSFSPopoutPanelManager.Shared;
using MSFSPopoutPanelManager.WindowsAgent;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace MSFSPopoutPanelManager.Orchestration
{
    public class PanelPopOutOrchestrator : ObservableObject
    {
        // This will be replaced by a signal from Ready to Fly Skipper into webserver in version 4.0
        private const int READY_TO_FLY_BUTTON_APPEARANCE_DELAY = 2000;

        private ProfileData _profileData;
        private AppSettingData _appSettingData;
        private FlightSimData _flightSimData;
        private int _prePopOutCockpitZoomLevel = 50;

        public PanelPopOutOrchestrator(ProfileData profileData, AppSettingData appSettingData, FlightSimData flightSimData)
        {
            _profileData = profileData;
            _appSettingData = appSettingData;
            _flightSimData = flightSimData;
        }

        internal FlightSimOrchestrator FlightSimOrchestrator { private get; set; }

        internal PanelSourceOrchestrator PanelSourceOrchestrator { private get; set; }

        internal PanelConfigurationOrchestrator PanelConfigurationOrchestrator { private get; set; }

        private UserProfile ActiveProfile { get { return _profileData == null ? null : _profileData.ActiveProfile; } }

        private ApplicationSetting AppSetting { get { return _appSettingData == null ? null : _appSettingData.ApplicationSetting; } }

        public event EventHandler OnPopOutStarted;
        public event EventHandler OnPopOutCompleted;
        public event EventHandler<PanelConfig> OnHudBarOpened;

        public async void ManualPopOut()
        {
            await CoreSteps(false);
        }

        public async void AutoPopOut()
        {
            await Application.Current.Dispatcher.Invoke(async () =>
            {
                _profileData.AutoSwitchProfile();

                if (ActiveProfile == null)
                    return;

                // Do not do auto pop out if no profile matches the current aircraft
                if (!ActiveProfile.AircraftBindings.Any(p => p == _flightSimData.AircraftName))
                    return;

                // Do not do auto pop out if no panel configs defined
                if (ActiveProfile.PanelConfigs.Count == 0)
                    return;

                // Do not do auto pop out if not all custom panels have panel source defined
                if (ActiveProfile.PanelConfigs.Count(p => p.PanelType == PanelType.CustomPopout) > 0 &&
                    ActiveProfile.PanelConfigs.Count(p => p.PanelType == PanelType.CustomPopout && p.HasPanelSource) != ActiveProfile.PanelConfigs.Count(p => p.PanelType == PanelType.CustomPopout))
                    
                    return;

                await CoreSteps(true);
            });
        }

        private async Task CoreSteps(bool isAutoPopOut)
        {
            if (ActiveProfile == null || ActiveProfile.IsEditingPanelSource || ActiveProfile.HasUnidentifiedPanelSource)
                return;

            OnPopOutStarted?.Invoke(this, null);

            StatusMessageWriter.IsEnabled = true;
            StatusMessageWriter.ClearMessage();
            StatusMessageWriter.WriteMessageNewLine("Pop out in progress. Please refrain from moving your mouse.", StatusMessageType.Info);

            StepPopoutPrep();

            await StepReadyToFlyDelay(isAutoPopOut);

            // *** THIS MUST BE DONE FIRST. Get the built-in panel list to be configured later
            List<IntPtr> builtInPanelHandles = WindowActionManager.GetWindowsByPanelType(new List<PanelType>() { PanelType.BuiltInPopout });

            await StepAddCutomPanels(builtInPanelHandles);

            StepAddBuiltInPanels(builtInPanelHandles);

            StepAddHudBar();

            SetupRefocusDisplay();

            StepApplyPanelConfig();

            await StepPostPopout();

            StatusMessageWriter.IsEnabled = false;
        }

        private void StepPopoutPrep()
        {
            PanelConfigurationOrchestrator.EndConfiguration();

            // Set profile pop out status
            _profileData.ResetActiveProfile();

            // Close all existing custom pop out panels
            WindowActionManager.CloseAllPopOuts();

            // Close all panel source overlays
            PanelSourceOrchestrator.CloseAllPanelSource();
        }

        private async Task StepReadyToFlyDelay(bool isAutoPopOut)
        {
            if (!isAutoPopOut)
                return;

            await Task.Run(() =>
            {
                StatusMessageWriter.WriteMessage("Waiting on ready to fly button delay", StatusMessageType.Info);

                // Match the delay for Ready to Fly button to disappear
                Thread.Sleep(READY_TO_FLY_BUTTON_APPEARANCE_DELAY);

                // Extra wait for cockpit view to appear and align
                Thread.Sleep(AppSetting.AutoPopOutSetting.ReadyToFlyDelay * 1000);

                StatusMessageWriter.WriteOkStatusMessage();
            });
        }

        private async Task StepAddCutomPanels(List<IntPtr> builtInPanelHandles)
        {
            if (!ActiveProfile.HasCustomPanels)
                return;

            await StepPreCustomPanelPopOut();

            await StepCustomPanelPopOut(builtInPanelHandles);

            await StepPostCustomPanelPopOut();
        }

        private async Task StepPreCustomPanelPopOut()
        {
            await Task.Run(() =>
            {
                // Set Windowed Display Mode window's configuration if needed
                if (_appSettingData.ApplicationSetting.WindowedModeSetting.AutoResizeMsfsGameWindow && WindowActionManager.IsMsfsGameInWindowedMode())
                {
                    StatusMessageWriter.WriteMessage("Moving and resizing MSFS game window", StatusMessageType.Info);
                    WindowActionManager.SetMsfsGameWindowLocation(ActiveProfile.MsfsGameWindowConfig);
                    Thread.Sleep(500);
                    StatusMessageWriter.WriteOkStatusMessage();
                }

                // Turn on power and avionics if required to pop out panels at least one (fix Cessna 208b grand caravan mod bug where battery is reported as on)
                if (ActiveProfile.ProfileSetting.PowerOnRequiredForColdStart)
                {
                    FlightSimOrchestrator.TurnOnPower();
                    FlightSimOrchestrator.TurnOnAvionics();
                }

                // Turn off TrackIR if TrackIR is started
                FlightSimOrchestrator.TurnOffTrackIR();

                // Turn on Active Pause
                FlightSimOrchestrator.TurnOnActivePause();

                // Setting custom camera angle for auto panning
                if (AppSetting.PopOutSetting.AutoPanning.IsEnabled)
                {
                    StatusMessageWriter.WriteMessage("Setting auto panning camera angle", StatusMessageType.Info);
                    
                    // Remember current game's zoom level to be recall after pop out
                    _prePopOutCockpitZoomLevel = _flightSimData.CockpitCameraZoom;
                    InputEmulationManager.LoadCustomView(AppSetting.PopOutSetting.AutoPanning.KeyBinding);
                    FlightSimOrchestrator.SetCockpitCameraZoomLevel(50);
                    Thread.Sleep(1000);
                    StatusMessageWriter.WriteOkStatusMessage();
                }
            });
        }

        private async Task StepCustomPanelPopOut(List<IntPtr> builtInPanelHandles)
        {
            await Task.Run(() =>
            {
                // Save current application location to restore it after pop out
                var appLocation = WindowActionManager.GetWindowRectangle(WindowProcessManager.GetApplicationProcess().Handle);

                int index = 0;
                foreach (var panelConfig in ActiveProfile.PanelConfigs)
                {
                    if (panelConfig.PanelType == PanelType.CustomPopout)
                    {
                        StatusMessageWriter.WriteMessage($"Popping out panel '{panelConfig.PanelName}'", StatusMessageType.Info);

                        panelConfig.IsSelectedPanelSource = true;
                        //PanelSourceOrchestrator.ShowPanelSourceNonEdit(panelConfig);
                        //Thread.Sleep(500);
                        //PanelSourceOrchestrator.ClosePanelSourceNonEdit(panelConfig);
                        ExecuteCustomPopout(panelConfig, builtInPanelHandles, index++);
                        ApplyPanelLocation(panelConfig);
                        panelConfig.IsSelectedPanelSource = false;

                        if (panelConfig.IsPopOutSuccess != null && !(bool)panelConfig.IsPopOutSuccess)
                            StatusMessageWriter.WriteFailureStatusMessage();
                        else
                            StatusMessageWriter.WriteOkStatusMessage();
                    }
                }

                // Restore current application location
                WindowActionManager.MoveWindow(WindowProcessManager.GetApplicationProcess().Handle, appLocation);
            });
        }

        private async Task StepPostCustomPanelPopOut()
        {
            await Task.Run(() =>
            {
                if (ActiveProfile.ProfileSetting.PowerOnRequiredForColdStart)
                {
                    FlightSimOrchestrator.TurnOffAvionics();
                    FlightSimOrchestrator.TurnOffPower();
                }

                // Turn TrackIR back on
                FlightSimOrchestrator.TurnOnTrackIR();

                // Turn on Active Pause
                FlightSimOrchestrator.TurnOffActivePause();

                // Return to custom camera view if set
                var task = Task.Run(() => {
                    FlightSimOrchestrator.SetCockpitCameraZoomLevel(_prePopOutCockpitZoomLevel);
                    ReturnToAfterPopOutCameraView();
                });
            });
        }

        private void StepAddBuiltInPanels(List<IntPtr> builtInPanelHandles)
        {
            if (ActiveProfile.ProfileSetting.IncludeInGamePanels)
            {
                var builtInPanels = new List<PanelConfig>();

                StatusMessageWriter.WriteMessage("Configuring built-in panel", StatusMessageType.Info);

                foreach (var panelHandle in builtInPanelHandles)
                {
                    var panelCaption = WindowActionManager.GetWindowCaption(panelHandle);
                    var panelConfig = ActiveProfile.PanelConfigs.FirstOrDefault(p => p.PanelName == panelCaption);

                    if (panelConfig == null)
                    {
                        if (!ActiveProfile.IsLocked)
                        {
                            var rectangle = WindowActionManager.GetWindowRectangle(panelHandle);
                            panelConfig = new PanelConfig()
                            {
                                PanelHandle = panelHandle,
                                PanelType = PanelType.BuiltInPopout,
                                PanelName = panelCaption,
                                Top = rectangle.Top,
                                Left = rectangle.Left,
                                Width = rectangle.Width,
                                Height = rectangle.Height,
                                AutoGameRefocus = false
                            };

                            ActiveProfile.PanelConfigs.Add(panelConfig);
                        }
                    }
                    else
                    {
                        panelConfig.PanelHandle = panelHandle;

                        // Need to do it twice for MSFS to take this setting (MSFS bug)
                        ApplyPanelLocation(panelConfig);
                        ApplyPanelLocation(panelConfig);
                    }
                }

                // Set handles for missing built-in panels
                foreach (var panelConfig in ActiveProfile.PanelConfigs)
                {
                    if (panelConfig.PanelType == PanelType.BuiltInPopout && panelConfig.PanelHandle == IntPtr.MaxValue)
                        panelConfig.PanelHandle = IntPtr.Zero;
                }

                if (ActiveProfile.PanelConfigs.Any(p => p.PanelType == PanelType.BuiltInPopout && p.IsPopOutSuccess != null && !(bool)p.IsPopOutSuccess) ||
                    ActiveProfile.PanelConfigs.Count(p => p.PanelType == PanelType.BuiltInPopout) == 0)
                    StatusMessageWriter.WriteFailureStatusMessage();
                else
                    StatusMessageWriter.WriteOkStatusMessage();
            }
        }

        private void StepAddHudBar()
        {
            if (!ActiveProfile.ProfileSetting.HudBarConfig.IsEnabled)
                return;

            StatusMessageWriter.WriteMessage("Opening HUD Bar", StatusMessageType.Info);

            var panelConfig = ActiveProfile.PanelConfigs.FirstOrDefault(p => p.PanelType == PanelType.HudBarWindow);

            OnHudBarOpened?.Invoke(this, panelConfig);

            StatusMessageWriter.WriteOkStatusMessage();
        }

        public void SetupRefocusDisplay()
        {
            if (!ActiveProfile.ProfileSetting.RefocusOnDisplay.IsEnabled)
                return;

            foreach (var panelConfig in ActiveProfile.PanelConfigs.Where(p => p.PanelType == PanelType.RefocusDisplay))
            {
                if (panelConfig != null)
                {
                    StatusMessageWriter.WriteMessage($"Configurating {panelConfig.PanelName} for auto refocus on touch", StatusMessageType.Info);
                    panelConfig.PanelHandle = new IntPtr(1);
                    StatusMessageWriter.WriteOkStatusMessage();
                }
            }
        }

        private void StepApplyPanelConfig()
        {
            // Must apply other panel config after pop out since MSFS popping out action will overwrite panel config such as Always on Top
            foreach (var panelConfig in ActiveProfile.PanelConfigs)
            {
                ApplyPanelConfig(panelConfig);

                // Set title bar color
                if (_appSettingData.ApplicationSetting.PopOutSetting.PopOutTitleBarCustomization.IsEnabled && !panelConfig.FullScreen)
                {
                    WindowActionManager.SetWindowTitleBarColor(panelConfig.PanelHandle, _appSettingData.ApplicationSetting.PopOutSetting.PopOutTitleBarCustomization.HexColor);
                }
            }
        }

        private async Task StepPostPopout()
        {
            await Task.Run(() =>
            {
                // Set profile pop out status
                ActiveProfile.IsPoppedOut = true;

                // Must use application dispatcher to dispatch UI events (winEventHook)
                PanelConfigurationOrchestrator.StartConfiguration();

                // Start touch hook
                PanelConfigurationOrchestrator.StartTouchHook();

                if (CheckForPopOutError())
                    StatusMessageWriter.WriteMessageNewLine("Pop out has been completed with error.", StatusMessageType.Info, 10);
                else
                    StatusMessageWriter.WriteMessageNewLine("Pop out has been completed successfully.", StatusMessageType.Info, 10);

                Thread.Sleep(1000);
                OnPopOutCompleted?.Invoke(this, null);
            });
        }

        private void ExecuteCustomPopout(PanelConfig panel, List<IntPtr> builtInPanelHandles, int index)
        {
            if (panel.PanelType == PanelType.CustomPopout)
            {
                // There should only be one handle that is not in both builtInPanelHandles vs latestAceAppWindowsWithCaptionHandles
                var handle = TryPopOutCustomPanel(panel.PanelSource, builtInPanelHandles);

                if (handle == IntPtr.Zero)
                {
                    panel.PanelHandle = IntPtr.Zero;
                    return;
                }

                // Unable to pop out panel, the handle was previously popped out's handle
                if (_profileData.ActiveProfile.PanelConfigs.Any(p => p.PanelHandle.Equals(handle)) || handle.Equals(WindowProcessManager.SimulatorProcess.Handle) || handle == IntPtr.Zero)
                {
                    panel.PanelHandle = IntPtr.Zero;
                    return;
                }

                panel.PanelHandle = handle;
                WindowActionManager.SetWindowCaption(panel.PanelHandle, $"{panel.PanelName} (Custom)");

                // First time popping out
                if (panel.Width == 0 && panel.Height == 0)
                {
                    var rect = WindowActionManager.GetWindowRectangle(panel.PanelHandle);
                    panel.Top = 0 + index * 30;
                    panel.Left = 0 + index * 30;
                    panel.Width = rect.Width;
                    panel.Height = rect.Height;
                }
            }
        }

        private IntPtr TryPopOutCustomPanel(PanelSource panelSource, List<IntPtr> builtInPanelHandles)
        {
            // Try to pop out 5 times before failure with 1/4 second wait in between
            int count = 0;
            do
            {
                InputEmulationManager.PopOutPanel((int)panelSource.X, (int)panelSource.Y, AppSetting.PopOutSetting.UseLeftRightControlToPopOut);

                var latestAceAppWindowsWithCaptionHandles = WindowActionManager.GetWindowsByPanelType(new List<PanelType>() { PanelType.BuiltInPopout });

                // There should only be one handle that is not in both builtInPanelHandles vs latestAceAppWindowsWithCaptionHandles
                var handle = latestAceAppWindowsWithCaptionHandles.Except(builtInPanelHandles).FirstOrDefault();

                if (handle != IntPtr.Zero)
                    return handle;

                Thread.Sleep(250);
                count++;
            }
            while (count < 5);

            return IntPtr.Zero;
        }

        private void ApplyPanelLocation(PanelConfig panel)
        {
            if (panel.IsPopOutSuccess == null || !((bool)panel.IsPopOutSuccess) || panel.PanelHandle == IntPtr.Zero)
                return;

            // Apply top/left/width/height
            WindowActionManager.MoveWindow(panel.PanelHandle, panel.Left, panel.Top, panel.Width, panel.Height);
        }

        private void ApplyPanelConfig(PanelConfig panel)
        {
            if (panel.IsPopOutSuccess == null || !((bool)panel.IsPopOutSuccess) || panel.PanelHandle == IntPtr.Zero)
                return;

            if (!panel.FullScreen)
            {
                // Apply always on top
                if (panel.AlwaysOnTop)
                {
                    WindowActionManager.ApplyAlwaysOnTop(panel.PanelHandle, panel.PanelType, panel.AlwaysOnTop);
                    Thread.Sleep(250);
                }

                // Apply hide title bar
                if (panel.HideTitlebar)
                {
                    WindowActionManager.ApplyHidePanelTitleBar(panel.PanelHandle, true);
                    Thread.Sleep(250);
                }
            }

            if (panel.FullScreen && !panel.AlwaysOnTop && !panel.HideTitlebar)
            {
                Thread.Sleep(500);
                InputEmulationManager.ToggleFullScreenPanel(panel.PanelHandle);
                Thread.Sleep(250);
            }
        }

        private void ReturnToAfterPopOutCameraView()
        {
            if (!AppSetting.PopOutSetting.AfterPopOutCameraView.IsEnabled)
                return;

            switch (AppSetting.PopOutSetting.AfterPopOutCameraView.CameraView)
            {
                case AfterPopOutCameraViewType.CockpitCenterView:
                    InputEmulationManager.CenterView();
                    Thread.Sleep(500);
                    FlightSimOrchestrator.SetCockpitCameraZoomLevel(_prePopOutCockpitZoomLevel);
                    break;
                case AfterPopOutCameraViewType.CustomCameraView:
                    InputEmulationManager.LoadCustomView(AppSetting.PopOutSetting.AfterPopOutCameraView.KeyBinding);
                    FlightSimOrchestrator.SetCockpitCameraZoomLevel(_prePopOutCockpitZoomLevel);
                    break;
            }
        }

        private bool CheckForPopOutError()
        {
            return ActiveProfile.PanelConfigs.Count(p => p.IsPopOutSuccess != null && (bool)p.IsPopOutSuccess) != ActiveProfile.PanelConfigs.Count(p => p.IsPopOutSuccess != null);
        }
    }
}

