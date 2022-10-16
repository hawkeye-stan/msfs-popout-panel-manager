using MSFSPopoutPanelManager.Shared;
using MSFSPopoutPanelManager.UserDataAgent;
using MSFSPopoutPanelManager.WindowsAgent;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MSFSPopoutPanelManager.Orchestration
{
    public class PanelPopOutOrchestrator : ObservableObject
    {
        // This will be replaced by a signal from Ready to Fly Skipper into webserver in version 4.0
        private const int READY_TO_FLY_BUTTON_APPEARANCE_DELAY = 2000;
        private int _builtInPanelConfigDelay;

        internal ProfileData ProfileData { get; set; }

        internal AppSettingData AppSettingData { get; set; }

        internal FlightSimData FlightSimData { get; set; }

        internal FlightSimOrchestrator FlightSimOrchestrator { private get; set; }

        internal PanelSourceOrchestrator PanelSourceOrchestrator { private get; set; }

        internal TouchPanelOrchestrator TouchPanelOrchestrator { private get; set; }

        private Profile ActiveProfile { get { return ProfileData == null ? null : ProfileData.ActiveProfile; } }

        private AppSetting AppSetting { get { return AppSettingData == null ? null : AppSettingData.AppSetting; } }

        public event EventHandler OnPopOutStarted;
        public event EventHandler<bool> OnPopOutCompleted;
        public event EventHandler<TouchPanelOpenEventArg> OnTouchPanelOpened;
        public event EventHandler<PanelSourceCoordinate> OnPanelSourceOverlayFlashed;

        public void ManualPopOut()
        {
            if (ActiveProfile == null)
                return;

            InputHookManager.EndHook();

            if (ActiveProfile.PanelSourceCoordinates.Count > 0 || ActiveProfile.TouchPanelBindings.Count > 0 || ActiveProfile.IncludeInGamePanels)
            {
                StatusMessageWriter.WriteMessage($"Panels pop out in progress for profile:\n{ActiveProfile.ProfileName}", StatusMessageType.Info, true);
                _builtInPanelConfigDelay = 0;
                CorePopOutSteps();
            }
        }

        public void AutoPopOut()
        {
            if (ActiveProfile == null)
                return;

            ProfileData.AutoSwitchProfile();

            // find the profile with the matching binding aircraft
            var profile = ProfileData.Profiles.FirstOrDefault(p => p.BindingAircrafts.Any(p => p == FlightSimData.CurrentMsfsAircraft));

            // Do not do auto pop out if no profile matches the current aircraft
            if (profile == null)
                return;

            // Match the delay for Ready to Fly button to disappear
            Thread.Sleep(READY_TO_FLY_BUTTON_APPEARANCE_DELAY);

            if (ActiveProfile.PanelSourceCoordinates.Count > 0 || ActiveProfile.TouchPanelBindings.Count > 0 || ActiveProfile.IncludeInGamePanels)
            {
                StatusMessageWriter.WriteMessage($"Automatic pop out is starting for profile:\n{profile.ProfileName}", StatusMessageType.Info, true);

                // Extra wait for cockpit view to appear and align
                Thread.Sleep(2000);

                _builtInPanelConfigDelay = 4000;

                CorePopOutSteps();
            }
        }

        private void CorePopOutSteps()
        {
            // Set Windowed Display Mode window's configuration if needed
            if (AppSettingData.AppSetting.AutoResizeMsfsGameWindow)
                WindowActionManager.SetMsfsGameWindowLocation(ActiveProfile.MsfsGameWindowConfig);

            // Has custom pop out panels
            if (ActiveProfile.PanelSourceCoordinates.Count > 0)
            {
                // Turn off TrackIR if TrackIR is started
                FlightSimOrchestrator.TurnOffTrackIR();

                // Turn on power if required to pop out panels at least one (fix Cessna 208b grand caravan mod bug where battery is reported as on)
                if (ActiveProfile.PowerOnRequiredForColdStart)
                {
                    int count = 0;
                    do
                    {
                        FlightSimOrchestrator.TurnOnPower();
                        Thread.Sleep(500);
                        count++;
                    }
                    while (!FlightSimData.ElectricalMasterBatteryStatus && count < 10);
                }

                // Turn on avionics if required to pop out panels
                if (ActiveProfile.PowerOnRequiredForColdStart)
                    FlightSimOrchestrator.TurnOnAvionics();
            }

            StartPopout();

            // Has custom pop out panels
            if (ActiveProfile.PanelSourceCoordinates.Count > 0)
            {
                // Turn off avionics if needed after pop out
                FlightSimOrchestrator.TurnOffAvionics();

                // Turn off power if needed after pop out
                FlightSimOrchestrator.TurnOffPower();

                // Return to custom camera view
                ReturnToAfterPopOutCameraView();

                // Turn TrackIR back on
                FlightSimOrchestrator.TurnOnTrackIR();
            }
        }

        private void StartPopout()
        {
            List<PanelConfig> panelConfigs = new List<PanelConfig>();

            var simulatorProcess = WindowProcessManager.GetSimulatorProcess();

            if (simulatorProcess == null || simulatorProcess.Handle == IntPtr.Zero)
            {
                StatusMessageWriter.WriteMessage("MSFS/SimConnect has not been started. Please try again at a later time.", StatusMessageType.Error, false);
                return;
            }

            if (ActiveProfile == null)
            {
                StatusMessageWriter.WriteMessage("No profile has been selected. Please select a profile to continue.", StatusMessageType.Error, false);
                return;
            }

            if (ActiveProfile.PanelSourceCoordinates.Count == 0 && ActiveProfile.TouchPanelBindings.Count == 0 && !ActiveProfile.IncludeInGamePanels)
            {
                StatusMessageWriter.WriteMessage("No panel has been selected for the profile. Please select at least one panel to continue.", StatusMessageType.Error, false);
                return;
            }

            // Close all existing custom pop out panels
            WindowActionManager.CloseAllPopOuts();

            // Close all panel source overlays
            PanelSourceOrchestrator.CloseAllPanelSource();

            OnPopOutStarted?.Invoke(this, null);

            // Must close out all existing custom pop out panels
            if (WindowActionManager.GetWindowsCountByPanelType(new List<PanelType>() { PanelType.CustomPopout, PanelType.MSFSTouchPanel }) > 0)
            {
                StatusMessageWriter.WriteMessage("Please close all existing panel pop outs to continue.", StatusMessageType.Error, false);
                return;
            }

            // Try to pop out and separate custom panels
            if (ActiveProfile.PanelSourceCoordinates.Count > 0)
            {
                if (AppSetting.UseAutoPanning)
                    InputEmulationManager.LoadCustomView(AppSetting.AutoPanningKeyBinding);

                var panelResults = ExecutePopoutAndSeparation();

                if (panelResults == null)
                    return;

                panelConfigs.AddRange(panelResults);
            }

            // Add the MSFS Touch Panel (My other github project) windows to the panel list
            if (AppSetting.TouchPanelSettings.EnableTouchPanelIntegration)
            {
                var panelResults = AddMsfsTouchPanels(panelConfigs.Count + 100);     // add a panelIndex gap
                if (panelResults != null)
                    panelConfigs.AddRange(panelResults);
            }

            // Add the built-in panels from toolbar menu (ie. VFR Map, Check List, Weather, etc)
            if (ActiveProfile.IncludeInGamePanels)
            {
                // Allow delay to wait for in game built-in pop outs to appear
                Thread.Sleep(_builtInPanelConfigDelay);

                var panelResults = AddBuiltInPanels();
                if (panelResults != null)
                    panelConfigs.AddRange(panelResults);
            }

            if (panelConfigs.Count == 0)
            {
                StatusMessageWriter.WriteMessage("No panels have been found. Please select at least one in-game panel.", StatusMessageType.Error, true);
                return;
            }

            // Line up all the panels
            for (var i = panelConfigs.Count - 1; i >= 0; i--)
            {
                if (panelConfigs[i].PanelType == PanelType.CustomPopout)
                {
                    WindowActionManager.MoveWindow(panelConfigs[i].PanelHandle, panelConfigs[i].Top, panelConfigs[i].Left, panelConfigs[i].Width, panelConfigs[i].Height);
                    PInvoke.SetForegroundWindow(panelConfigs[i].PanelHandle);
                    Thread.Sleep(200);
                }
            }

            if (panelConfigs.Count > 0)
            {
                if (ActiveProfile.PanelConfigs.Count > 0)
                {
                    LoadAndApplyPanelConfigs(panelConfigs);
                    StatusMessageWriter.WriteMessage("Panels have been popped out succesfully and saved panel settings have been applied.", StatusMessageType.Info, true);
                    OnPopOutCompleted?.Invoke(this, false);
                }
                else
                {
                    LoadAndApplyPanelConfigs(panelConfigs);
                    StatusMessageWriter.WriteMessage("Panels have been popped out succesfully.", StatusMessageType.Info, true);
                    OnPopOutCompleted?.Invoke(this, true);
                }

                if (!ActiveProfile.IsLocked)
                    ProfileData.WriteProfiles();

                // For migrating existing profile, if using windows mode, save MSFS game window configuration
                if (AppSettingData.AppSetting.AutoResizeMsfsGameWindow && !ActiveProfile.MsfsGameWindowConfig.IsValid)
                    ProfileData.SaveMsfsGameWindowConfig();
            }
        }

        private List<PanelConfig> ExecutePopoutAndSeparation()
        {
            List<PanelConfig> panels = new List<PanelConfig>();

            // PanelIndex starts at 1
            for (var i = 1; i <= ActiveProfile.PanelSourceCoordinates.Count; i++)
            {
                var x = ActiveProfile.PanelSourceCoordinates[i - 1].X;
                var y = ActiveProfile.PanelSourceCoordinates[i - 1].Y;

                // show the panel source overlay for split second
                Task task = new Task(() => OnPanelSourceOverlayFlashed?.Invoke(this, ActiveProfile.PanelSourceCoordinates[i - 1]));
                task.RunSynchronously();

                InputEmulationManager.PopOutPanel(x, y, AppSetting.UseLeftRightControlToPopOut);

                // Get an AceApp window with empty title
                var handle = PInvoke.FindWindow("AceApp", String.Empty);

                // Need to move the window to upper left corner first. There is a possible bug in the game that panel pop out to full screen that prevents further clicking.
                if (handle != IntPtr.Zero)
                    WindowActionManager.MoveWindow(handle, 0, 0, 1000, 500);

                // The joined panel is always the first panel that got popped out
                if (i > 1)
                    SeparatePanel(panels[0].PanelHandle);

                handle = PInvoke.FindWindow("AceApp", String.Empty);

                if (handle == IntPtr.Zero && i == 1)
                {
                    StatusMessageWriter.WriteMessage("Unable to pop out the first panel. Please check the first panel's number circle is positioned inside the panel, check for panel obstruction, and check if panel can be popped out. Pop out process stopped.", StatusMessageType.Error, true);
                    return null;
                }
                else if (handle == IntPtr.Zero)
                {
                    StatusMessageWriter.WriteMessage($"Unable to pop out panel number {i}. Please check panel's number circle is positioned inside the panel, check for panel obstruction, and check if panel can be popped out. Pop out process stopped.", StatusMessageType.Error, true);
                    return null;
                }

                // Fix SU10+ bug where pop out window after separation is huge
                if (i > 1)
                    WindowActionManager.MoveWindow(handle, -8, 0, 800, 600);

                var panel = new PanelConfig();
                panel.PanelHandle = handle;
                panel.PanelType = PanelType.CustomPopout;
                panel.PanelIndex = i;
                panel.PanelName = $"Panel{i}";
                panel.Top = (i - 1) * 30;
                panel.Left = (i - 1) * 30;
                panel.Width = 800;
                panel.Height = 600;
                panels.Add(panel);

                PInvoke.SetWindowText(panel.PanelHandle, panel.PanelName + " (Custom)");
            }

            //Perform validation, make sure the number of pop out panels is equal to the number of selected panel
            if (WindowActionManager.GetWindowsCountByPanelType(new List<PanelType>() { PanelType.CustomPopout }) != ActiveProfile.PanelSourceCoordinates.Count)
            {
                StatusMessageWriter.WriteMessage("Unable to pop out all panels. Please align all panel number circles with in-game panel locations.", StatusMessageType.Error, false);
                return null;
            }

            return panels;
        }

        private void SeparatePanel(IntPtr hwnd)
        {
            // ToDo: Need to figure mouse click code to separate window
            PInvoke.SetForegroundWindow(hwnd);
            Thread.Sleep(500);

            // Find the magnifying glass coordinate    
            var point = PanelAnalyzer.GetMagnifyingGlassIconCoordinate(hwnd);

            InputEmulationManager.LeftClick(point.X, point.Y);
        }

        private List<PanelConfig> AddBuiltInPanels()
        {
            List<PanelConfig> builtinPanels = new List<PanelConfig>();

            var panelHandles = WindowActionManager.GetWindowsByPanelType(new List<PanelType>() { PanelType.BuiltInPopout });

            foreach (var panelHandle in panelHandles)
            {
                var rectangle = WindowActionManager.GetWindowRect(panelHandle);
                var clientRectangle = WindowActionManager.GetClientRect(panelHandle);

                builtinPanels.Add(new PanelConfig()
                {
                    PanelIndex = -1,
                    PanelHandle = panelHandle,
                    PanelType = PanelType.BuiltInPopout,
                    PanelName = WindowActionManager.GetWindowCaption(panelHandle),
                    Top = rectangle.Top,
                    Left = rectangle.Left,
                    Width = clientRectangle.Width,
                    Height = clientRectangle.Height
                });
            }

            return builtinPanels.Count == 0 ? null : builtinPanels;
        }

        private List<PanelConfig> AddMsfsTouchPanels(int panelIndex)
        {
            List<PanelConfig> touchPanels = new List<PanelConfig>();

            if (AppSetting.TouchPanelSettings.EnableTouchPanelIntegration)
            {
                TouchPanelOrchestrator.LoadPlaneProfiles();

                if (TouchPanelOrchestrator.PlaneProfiles == null)
                    return null;

                // Find all selected panels
                var panelConfigs = TouchPanelOrchestrator.PlaneProfiles.SelectMany(p => p.Panels.Where(c => c.IsSelected));

                foreach (var panelConfig in panelConfigs)
                {
                    var caption = $"{panelConfig.Name} (Touch Panel)";

                    // Change width and height to 1080p aspect ratio
                    double aspectRatio = 1;
                    if (panelConfig.Width > 1920)
                    {
                        aspectRatio = Convert.ToDouble(1920) / panelConfig.Width;
                        panelConfig.Width = 1920;                                   // there are hidden padding to make it to 1920
                        panelConfig.Height = Convert.ToInt32(panelConfig.Height * aspectRatio);
                    }

                    OnTouchPanelOpened?.Invoke(this, new TouchPanelOpenEventArg() { PlaneId = panelConfig.PlaneId, PanelId = panelConfig.PanelId, Caption = caption, Width = panelConfig.Width, Height = panelConfig.Height });

                    // detect for a max of 5 seconds
                    int tryCount = 0;
                    while (tryCount < 10)
                    {
                        var touchPanelHandle = WindowActionManager.FindWindowByCaption(caption);
                        if (touchPanelHandle != IntPtr.Zero)
                        {
                            var dimension = WindowActionManager.GetWindowRect(touchPanelHandle);
                            var panelInfo = new PanelConfig
                            {
                                PanelIndex = panelIndex,
                                PanelHandle = touchPanelHandle,
                                PanelName = caption,
                                PanelType = PanelType.MSFSTouchPanel,
                                Top = dimension.Top,
                                Left = dimension.Left,
                                Width = panelConfig.Width,
                                Height = panelConfig.Height,
                                AlwaysOnTop = true     // default to always on top
                            };

                            touchPanels.Add(panelInfo);
                            break;
                        }
                        Thread.Sleep(500);
                        tryCount++;
                    }

                    if (tryCount == 10)
                        return null;

                    panelIndex++;
                }
            }

            return touchPanels.Count == 0 ? null : touchPanels;
        }

        private void LoadAndApplyPanelConfigs(List<PanelConfig> panelResults)
        {
            ActiveProfile.PanelConfigs.ToList().ForEach(p => p.PanelHandle = IntPtr.Zero);

            Parallel.ForEach(panelResults, panel =>
            {
                // Something is wrong here where panel has no window handle
                if (panel.PanelHandle == IntPtr.Zero)
                    return;

                PanelConfig savedPanelConfig = null;

                if (panel.PanelType == PanelType.CustomPopout || panel.PanelType == PanelType.MSFSTouchPanel)
                    savedPanelConfig = ActiveProfile.PanelConfigs.FirstOrDefault(s => s.PanelIndex == panel.PanelIndex);
                else if (panel.PanelType == PanelType.BuiltInPopout)
                    savedPanelConfig = ActiveProfile.PanelConfigs.FirstOrDefault(s => s.PanelName == panel.PanelName);

                if (savedPanelConfig == null) return;

                // Assign window handle to panel config
                savedPanelConfig.PanelHandle = panel.PanelHandle;

                // Apply panel name
                if (savedPanelConfig.PanelType == PanelType.CustomPopout)
                {
                    var caption = savedPanelConfig.PanelName + " (Custom)";
                    PInvoke.SetWindowText(savedPanelConfig.PanelHandle, caption);
                    Thread.Sleep(500);
                }

                // Apply locations
                if (savedPanelConfig.Width != 0 && savedPanelConfig.Height != 0)
                {
                    PInvoke.ShowWindow(savedPanelConfig.PanelHandle, PInvokeConstant.SW_RESTORE);
                    Thread.Sleep(250);
                    WindowActionManager.MoveWindow(savedPanelConfig.PanelHandle, savedPanelConfig.Left, savedPanelConfig.Top, savedPanelConfig.Width, savedPanelConfig.Height);
                    Thread.Sleep(1000);
                }

                // Apply window size again to overcome a bug in MSFS that when moving panel between monitors, panel automatic resize for no reason
                if (savedPanelConfig.PanelType == PanelType.BuiltInPopout)
                {
                    Thread.Sleep(2000);     // Overcome GTN750 bug
                    WindowActionManager.MoveWindow(savedPanelConfig.PanelHandle, savedPanelConfig.Left, savedPanelConfig.Top, savedPanelConfig.Width, savedPanelConfig.Height);
                    Thread.Sleep(1000);
                }

                if (!savedPanelConfig.FullScreen)
                {
                    // Apply always on top
                    if (savedPanelConfig.AlwaysOnTop)
                    {
                        WindowActionManager.ApplyAlwaysOnTop(savedPanelConfig.PanelHandle, savedPanelConfig.PanelType, true, new Rectangle(savedPanelConfig.Left, savedPanelConfig.Top, savedPanelConfig.Width, savedPanelConfig.Height));
                        Thread.Sleep(1000);
                    }

                    // Apply hide title bar
                    if (savedPanelConfig.HideTitlebar)
                        WindowActionManager.ApplyHidePanelTitleBar(savedPanelConfig.PanelHandle, true);
                }

                PInvoke.ShowWindow(savedPanelConfig.PanelHandle, PInvokeConstant.SW_RESTORE);
            });

            // If profile is unlocked, add any new panel into profile
            if (!ActiveProfile.IsLocked)
            {
                // Need this to fix collectionview modification thread issue
                var finalPanelConfigs = ActiveProfile.PanelConfigs.ToList();

                var isAdded = false;

                foreach (var panel in panelResults)
                {
                    if ((panel.PanelType == PanelType.BuiltInPopout || panel.PanelType == PanelType.MSFSTouchPanel) && !ActiveProfile.PanelConfigs.Any(s => s.PanelName == panel.PanelName))
                    {
                        finalPanelConfigs.Add(panel);
                        isAdded = true;
                    }
                    else if (panel.PanelType == PanelType.CustomPopout && !ActiveProfile.PanelConfigs.Any(s => s.PanelIndex == panel.PanelIndex))
                    {
                        finalPanelConfigs.Add(panel);
                        isAdded = true;
                    }
                }

                if (isAdded)
                {
                    ActiveProfile.PanelConfigs = new ObservableCollection<PanelConfig>(finalPanelConfigs);
                    ProfileData.WriteProfiles();
                }
            }

            // Apply full screen (cannot combine with always on top or hide title bar)
            // Cannot run in parallel process
            ActiveProfile.PanelConfigs.ToList().ForEach(panel =>
            {
                if (panel.FullScreen && (!panel.AlwaysOnTop && !panel.HideTitlebar))
                {
                    InputEmulationManager.ToggleFullScreenPanel(panel.PanelHandle);
                    Thread.Sleep(250);

                    // Set full screen mode panel coordinate
                    var windowRectangle = WindowActionManager.GetWindowRect(panel.PanelHandle);
                    var clientRectangle = WindowActionManager.GetClientRect(panel.PanelHandle);
                    panel.FullScreenLeft = windowRectangle.Left;
                    panel.FullScreenTop = windowRectangle.Top;
                    panel.FullScreenWidth = clientRectangle.Width;
                    panel.FullScreenHeight = clientRectangle.Height;
                }
            });
        }

        private void ReturnToAfterPopOutCameraView()
        {
            if (!AppSetting.AfterPopOutCameraView.EnableReturnToCameraView)
                return;

            switch (AppSetting.AfterPopOutCameraView.CameraView)
            {
                case AfterPopOutCameraViewType.CockpitCenterView:
                    InputEmulationManager.CenterView();
                    break;
                case AfterPopOutCameraViewType.CustomCameraView:
                    InputEmulationManager.LoadCustomView(AppSetting.AfterPopOutCameraView.CustomCameraKeyBinding);
                    break;
            }
        }
    }
}

