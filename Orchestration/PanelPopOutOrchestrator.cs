﻿using MSFSPopoutPanelManager.DomainModel.Profile;
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
        private const int READY_TO_FLY_BUTTON_APPEARANCE_DELAY = 2000;
        private const int CAMERA_VIEW_HOME_COCKPIT_MODE = 8;
        private const int CAMERA_VIEW_CUSTOM_CAMERA = 7;

        private ProfileData _profileData;
        private AppSettingData _appSettingData;
        private FlightSimData _flightSimData;

        public PanelPopOutOrchestrator(ProfileData profileData, AppSettingData appSettingData, FlightSimData flightSimData)
        {
            _profileData = profileData;
            _appSettingData = appSettingData;
            _flightSimData = flightSimData;
            IsDisabledStartPopOut = false;
        }

        internal FlightSimOrchestrator FlightSimOrchestrator { private get; set; }

        internal PanelSourceOrchestrator PanelSourceOrchestrator { private get; set; }

        internal PanelConfigurationOrchestrator PanelConfigurationOrchestrator { private get; set; }

        private UserProfile ActiveProfile { get { return _profileData == null ? null : _profileData.ActiveProfile; } }

        private ApplicationSetting AppSetting { get { return _appSettingData == null ? null : _appSettingData.ApplicationSetting; } }

        public bool IsDisabledStartPopOut { get; set; }

        public event EventHandler OnPopOutStarted;
        public event EventHandler OnPopOutCompleted;
        public event EventHandler<PanelConfig> OnHudBarOpened;

        public async void ManualPopOut()
        {
            if (IsDisabledStartPopOut || !_flightSimData.IsInCockpit)
                return;

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

                await StepReadyToFlyDelay(true);

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
            StatusMessageWriter.WriteMessageWithNewLine("Pop out in progress. Please refrain from moving your mouse.", StatusMessageType.Info);

            StepPopoutPrep();

            // *** THIS MUST BE DONE FIRST. Get the built-in panel list to be configured later
            List<IntPtr> builtInPanelHandles = WindowActionManager.GetWindowsByPanelType(new List<PanelType>() { PanelType.BuiltInPopout });

            await StepAddCustomPanels(builtInPanelHandles);

            StepAddBuiltInPanels(builtInPanelHandles);

            StepAddHudBar();

            SetupRefocusDisplay();

            StepApplyPanelConfig();

            await StepPostPopout();

            OnPopOutCompleted?.Invoke(this, null);

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

            if (AppSetting.AutoPopOutSetting.ReadyToFlyDelay == 0)
                return;

            await Task.Run(() =>
            {
                WorkflowStepWithMessage.Execute("Waiting on ready to fly button delay", () =>
                {
                    // Ready to fly button plugin default delay
                    Thread.Sleep(READY_TO_FLY_BUTTON_APPEARANCE_DELAY);

                    // Extra wait for cockpit view to appear and align
                    Thread.Sleep(AppSetting.AutoPopOutSetting.ReadyToFlyDelay * 1000);
                });
            });
        }


        private async Task StepAddCustomPanels(List<IntPtr> builtInPanelHandles)
        {
            if (!ActiveProfile.HasCustomPanels)
                return;

            if (!await StepPreCustomPanelPopOut())
            {
                ActiveProfile.PanelConfigs.Where(p => p.PanelType == PanelType.CustomPopout).ToList().ForEach(p => p.PanelHandle = IntPtr.Zero);
                return;
            }

            await StepCustomPanelPopOut(builtInPanelHandles);

            await StepPostCustomPanelPopOut();
        }

        private async Task<bool> StepPreCustomPanelPopOut()
        {
            return await Task.Run(() =>
            {
                // Set Windowed Display Mode window's configuration if needed
                if (_appSettingData.ApplicationSetting.WindowedModeSetting.AutoResizeMsfsGameWindow && WindowActionManager.IsMsfsGameInWindowedMode())
                {
                    WorkflowStepWithMessage.Execute("Moving and resizing MSFS game window", () =>
                    {
                        WindowActionManager.SetMsfsGameWindowLocation(ActiveProfile.MsfsGameWindowConfig);
                        Thread.Sleep(1000);
                    }); 
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
                    StatusMessageWriter.WriteMessageWithNewLine("Setting auto panning camera view", StatusMessageType.Info);
                   
                    if (_flightSimData.CameraViewTypeAndIndex1 == CAMERA_VIEW_HOME_COCKPIT_MODE)
                    {
                        SetCockpitZoomLevel(_profileData.ActiveProfile.PanelSourceCockpitZoomFactor, AppSetting.GeneralSetting.TurboMode);
                    }
                    else 
                    {
                        WorkflowStepWithMessage.Execute("Resetting camera view", () =>
                        {
                            ResetCockpitView(AppSetting.GeneralSetting.TurboMode);
                        }, true);


                        var success = WorkflowStepWithMessage.Execute("Loading custom camera view", () =>
                        {
                            return LoadCustomView(AppSetting.PopOutSetting.AutoPanning.KeyBinding, AppSetting.GeneralSetting.TurboMode, false);
                        }, true);

                        if (!success)
                            return false;

                        WorkflowStepWithMessage.Execute("Setting camera zoom level", () =>
                        {
                            SetCockpitZoomLevel(50, AppSetting.GeneralSetting.TurboMode);
                        }, true);
                    }
                }

                return true;
            });
        }

        private async Task StepCustomPanelPopOut(List<IntPtr> builtInPanelHandles)
        {
            await Task.Run(() =>
            {
                // Save current application location to restore it after pop out
                var appLocation = WindowActionManager.GetWindowRectangle(WindowProcessManager.GetApplicationProcess().Handle);

                if(ActiveProfile.PanelConfigs.Count > 0)
                    StatusMessageWriter.WriteMessageWithNewLine("Popping out user defined panels", StatusMessageType.Info);

                int index = 0;
                foreach (var panelConfig in ActiveProfile.PanelConfigs)
                {
                    if (panelConfig.PanelType == PanelType.CustomPopout)
                    {
                        WorkflowStepWithMessage.Execute(panelConfig.PanelName, () =>
                            {
                                panelConfig.IsSelectedPanelSource = true;

                                PanelSourceOrchestrator.ShowPanelSourceNonEdit(panelConfig);
                                InputEmulationManager.PrepareToPopOutPanel((int)panelConfig.PanelSource.X, (int)panelConfig.PanelSource.Y, AppSetting.GeneralSetting.TurboMode);
                                PanelSourceOrchestrator.ClosePanelSourceNonEdit(panelConfig);
                                ExecuteCustomPopout(panelConfig, builtInPanelHandles, index++);

                                ApplyPanelLocation(panelConfig);
                                panelConfig.IsSelectedPanelSource = false;

                                if (panelConfig.IsPopOutSuccess != null && !(bool)panelConfig.IsPopOutSuccess)
                                    return false;
                                else
                                    return true;
                            }, true);
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
                Thread.Sleep(500);

                // Turn on Active Pause
                FlightSimOrchestrator.TurnOffActivePause();
                Thread.Sleep(500);

                // Return to custom camera view if set
                ReturnToAfterPopOutCameraView();
            });
        }

        private void StepAddBuiltInPanels(List<IntPtr> builtInPanelHandles)
        {
            if (ActiveProfile.ProfileSetting.IncludeInGamePanels)
            {
                WorkflowStepWithMessage.Execute("Configuring built-in panel", () =>
                {
                    int count = 0;
                    while (builtInPanelHandles.Count == 0 && count < 5)
                    {
                        builtInPanelHandles = WindowActionManager.GetWindowsByPanelType(new List<PanelType>() { PanelType.BuiltInPopout });
                        count++;
                    }

                    var builtInPanels = new List<PanelConfig>();

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
                        return false;
                    else
                        return true;
                });
            }
        }

        private void StepAddHudBar()
        {
            if (!ActiveProfile.ProfileSetting.HudBarConfig.IsEnabled)
                return;

            WorkflowStepWithMessage.Execute("Opening HUD Bar", () =>
            {
                var panelConfig = ActiveProfile.PanelConfigs.FirstOrDefault(p => p.PanelType == PanelType.HudBarWindow);
                OnHudBarOpened?.Invoke(this, panelConfig);
            });
        }

        public void SetupRefocusDisplay()
        {
            if (!ActiveProfile.ProfileSetting.RefocusOnDisplay.IsEnabled)
                return;

            var panelConfigs = ActiveProfile.PanelConfigs.Where(p => p.PanelType == PanelType.RefocusDisplay);

            if (panelConfigs.Count() == 0)
                return;

            StatusMessageWriter.WriteMessageWithNewLine("Configurating panels for auto refocus on touch", StatusMessageType.Info);

            foreach (var panelConfig in panelConfigs)
            {
                if (panelConfig != null)
                {
                    WorkflowStepWithMessage.Execute(panelConfig.PanelName, () =>
                    {
                        panelConfig.PanelHandle = new IntPtr(1);
                    }, true);
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
                    StatusMessageWriter.WriteMessageWithNewLine("Pop out has been completed with error.", StatusMessageType.Info);
                else
                    StatusMessageWriter.WriteMessageWithNewLine("Pop out has been completed successfully.", StatusMessageType.Info);

                Thread.Sleep(1000);
            });
        }

        private void ExecuteCustomPopout(PanelConfig panel, List<IntPtr> builtInPanelHandles, int index)
        {
            // There should only be one handle that is not in both builtInPanelHandles vs latestAceAppWindowsWithCaptionHandles
            var handle = TryPopOutCustomPanel(panel.PanelSource, builtInPanelHandles, AppSetting.GeneralSetting.TurboMode);

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

        private IntPtr TryPopOutCustomPanel(PanelSource panelSource, List<IntPtr> builtInPanelHandles, bool isTurbo)
        {
            // Try to pop out 5 times before failure with 1/2 second wait in between
            int count = 0;
            do
            {
                InputEmulationManager.PopOutPanel((int)panelSource.X, (int)panelSource.Y, AppSetting.PopOutSetting.UseLeftRightControlToPopOut, isTurbo);
                
                var latestAceAppWindowsWithCaptionHandles = WindowActionManager.GetWindowsByPanelType(new List<PanelType>() { PanelType.BuiltInPopout });

                // There should only be one handle that is not in both builtInPanelHandles vs latestAceAppWindowsWithCaptionHandles
                var handle = latestAceAppWindowsWithCaptionHandles.Except(builtInPanelHandles).FirstOrDefault();

                if (handle != IntPtr.Zero)
                    return handle;

                Thread.Sleep(500);
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

            StatusMessageWriter.WriteMessageWithNewLine("Applying cockpit view after pop out", StatusMessageType.Info);

            if (_flightSimData.CameraViewTypeAndIndex1 == CAMERA_VIEW_HOME_COCKPIT_MODE)
            {
                FlightSimOrchestrator.ResetCameraView();
            }
            else
            {
                switch (AppSetting.PopOutSetting.AfterPopOutCameraView.CameraView)
                {
                    case AfterPopOutCameraViewType.CockpitCenterView:
                        WorkflowStepWithMessage.Execute("Resetting camera view", () =>
                        {
                            ResetCockpitView(AppSetting.GeneralSetting.TurboMode);
                        }, true);

                        break;
                    case AfterPopOutCameraViewType.CustomCameraView:
                        WorkflowStepWithMessage.Execute("Resetting camera view", () =>
                        {
                            ResetCockpitView(AppSetting.GeneralSetting.TurboMode);
                        }, true);

                        WorkflowStepWithMessage.Execute("Loading custom camera view", () =>
                        {
                            return LoadCustomView(AppSetting.PopOutSetting.AfterPopOutCameraView.KeyBinding, AppSetting.GeneralSetting.TurboMode, true);
                        }, true);

                        break;
                }
            }
        }

        private bool CheckForPopOutError()
        {
            return ActiveProfile.PanelConfigs.Count(p => p.IsPopOutSuccess != null && (bool)p.IsPopOutSuccess) != ActiveProfile.PanelConfigs.Count(p => p.IsPopOutSuccess != null);
        }

        private void ResetCockpitView(bool isTurboMode)
        {
            int retry = 10;
            for (var i = 0; i < retry; i++)
            {
                FlightSimOrchestrator.ResetCameraView();
                Thread.Sleep(isTurboMode ? 600 : 1000);  // wait for flightsimdata to be updated
                if (_flightSimData.CameraViewTypeAndIndex1 == 0)    // 0 = reset view
                    break;
            }
        }

        private bool LoadCustomView(string keybinding, bool isTurboMode, bool ignoreError)
        {
            int retry = 10;
            for(var i = 0; i < retry; i++) 
            {
                InputEmulationManager.LoadCustomView(keybinding);
                Thread.Sleep(isTurboMode ? 600 : 1000); // wait for flightsimdata to be updated
                if (_flightSimData.CameraViewTypeAndIndex1 == CAMERA_VIEW_CUSTOM_CAMERA)    // custom camera view enum
                    return true;
            }

            return ignoreError;
        }

        private void SetCockpitZoomLevel(int zoom, bool isTurboMode)
        {
            int retry = 10;
            for (var i = 0; i < retry; i++)
            {
                FlightSimOrchestrator.SetCockpitCameraZoomLevel(zoom);
                Thread.Sleep(isTurboMode ? 600 : 1000);  // wait for flightsimdata to be updated

                if (_flightSimData.CockpitCameraZoom == zoom) 
                    break;
            }
        }
    }
}

