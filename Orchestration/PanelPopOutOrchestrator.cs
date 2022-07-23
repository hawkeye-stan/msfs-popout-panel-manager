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

        internal ProfileData ProfileData { get; set; }

        internal AppSettingData AppSettingData { get; set; }

        internal FlightSimData FlightSimData { get; set; }

        internal FlightSimOrchestrator FlightSimOrchestrator { private get; set; }

        internal PanelSourceOrchestrator PanelSourceOrchestrator { private get; set; }

        internal TouchPanelOrchestrator TouchPanelOrchestrator { private get; set; }

        private Profile ActiveProfile { get { return ProfileData == null ? null : ProfileData.ActiveProfile; } }

        private AppSetting AppSetting { get { return AppSettingData == null ? null : AppSettingData.AppSetting; } }

        public event EventHandler OnPopOutStarted;
        public event EventHandler OnPopOutCompleted;
        public event EventHandler<TouchPanelOpenEventArg> OnTouchPanelOpened;

        public void ManualPopOut()
        {
            if (ActiveProfile == null)
                return;

            InputHookManager.EndHook();

            if (ActiveProfile.PanelSourceCoordinates.Count > 0 || ActiveProfile.TouchPanelBindings.Count > 0)
            {
                StatusMessageWriter.WriteMessage($"Panels pop out in progress for profile:\n{ActiveProfile.ProfileName}", StatusMessageType.Info, true);
                CorePopOutSteps();
            }
        }

        public void AutoPopOut()
        {
            if (ActiveProfile == null)
                return;

            ProfileData.AutoSwitchProfile(FlightSimData.CurrentMsfsPlaneTitle);

            FlightSimData.IsEnteredFlight = true;

            // find the profile with the matching binding plane title
            var profile = ProfileData.Profiles.FirstOrDefault(p => p.BindingAircraftLiveries.Any(p => p == FlightSimData.CurrentMsfsPlaneTitle));

            // Do not do auto pop out if no profile matches the current livery
            if (profile == null)
                return;

            // Match the delay for Ready to Fly button to disappear
            Thread.Sleep(READY_TO_FLY_BUTTON_APPEARANCE_DELAY);

            if (ActiveProfile.PanelSourceCoordinates.Count > 0 || ActiveProfile.TouchPanelBindings.Count > 0)
            {
                StatusMessageWriter.WriteMessage($"Automatic pop out is starting for profile:\n{profile.ProfileName}", StatusMessageType.Info, true);

                // Extra wait for cockpit view to align
                Thread.Sleep(1000);

                CorePopOutSteps();
            }
        }

        private void CorePopOutSteps()
        {
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

            if (ActiveProfile.PanelSourceCoordinates.Count == 0 && ActiveProfile.TouchPanelBindings.Count == 0)
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
                var panelResults = AddMsfsTouchPanels(panelConfigs.Count + 1);
                if (panelResults == null)
                    return;

                panelConfigs.AddRange(panelResults);
            }

            if (panelConfigs.Count == 0)
            {
                OnPopOutCompleted?.Invoke(this, null);
                StatusMessageWriter.WriteMessage("No panels have been found. Please select at least one in-game panel.", StatusMessageType.Error, false);
                return;
            }

            // Line up all the panels
            for (var i = panelConfigs.Count - 1; i >= 0; i--)
            {
                if (panelConfigs[i].PanelType == PanelType.CustomPopout)
                {
                    WindowActionManager.MoveWindow(panelConfigs[i].PanelHandle, panelConfigs[i].PanelType, panelConfigs[i].Top, panelConfigs[i].Left, panelConfigs[i].Width, panelConfigs[i].Height);
                    PInvoke.SetForegroundWindow(panelConfigs[i].PanelHandle);
                    Thread.Sleep(200);
                }
            }

            if (panelConfigs.Count > 0 || ActiveProfile.PanelConfigs.Count > 0)
            {

                LoadAndApplyPanelConfigs(panelConfigs);
                ActiveProfile.PanelConfigs = new ObservableCollection<PanelConfig>(panelConfigs);
                StatusMessageWriter.WriteMessage("Panels have been popped out succesfully and saved panel settings have been applied.", StatusMessageType.Info, true);
                OnPopOutCompleted?.Invoke(this, null);
            }
            else
            {
                ActiveProfile.PanelConfigs = new ObservableCollection<PanelConfig>(panelConfigs);
                StatusMessageWriter.WriteMessage("Panels have been popped out succesfully.", StatusMessageType.Info, true);
                OnPopOutCompleted?.Invoke(this, null);
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
                InputEmulationManager.PopOutPanel(x, y, AppSetting.UseLeftRightControlToPopOut);

                var handle = PInvoke.FindWindow("AceApp", String.Empty);    // Get an AceApp window with empty title

                // Need to move the window to upper left corner first. There is a possible bug in the game that panel pop out to full screen that prevents further clicking.
                if (handle != IntPtr.Zero)
                    WindowActionManager.MoveWindow(handle, PanelType.CustomPopout, 0, 0, 800, 600);

                if (i > 1)
                    SeparatePanel(panels[0].PanelHandle);       // The joined panel is always the first panel that got popped out

                handle = PInvoke.FindWindow("AceApp", String.Empty);    // Get an AceApp window with empty title

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

            //Performance validation, make sure the number of pop out panels is equal to the number of selected panel
            if (WindowActionManager.GetWindowsCountByPanelType(new List<PanelType>() { PanelType.CustomPopout }) != ActiveProfile.PanelSourceCoordinates.Count)
            {
                StatusMessageWriter.WriteMessage("Unable to pop out all panels. Please align all panel number circles with in-game panel locations.", StatusMessageType.Error, false);
                return null;
            }

            return panels;
        }

        private void SeparatePanel(IntPtr hwnd)
        {
            // Resize all windows to 800x600 when separating and shimmy the panel
            // MSFS draws popout panel differently at different time for same panel

            // ToDo: Need to figure mouse click code to separate window
            WindowActionManager.MoveWindow(hwnd, PanelType.CustomPopout, -8, 0, 800, 600);
            PInvoke.SetForegroundWindow(hwnd);
            Thread.Sleep(500);

            // Find the magnifying glass coordinate    
            var point = PanelAnalyzer.GetMagnifyingGlassIconCoordinate(hwnd);

            InputEmulationManager.LeftClick(point.X, point.Y);
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

            return touchPanels;
        }

        private void LoadAndApplyPanelConfigs(List<PanelConfig> panelResults)
        {
            Parallel.ForEach(panelResults, panel =>
            {
                // Something is wrong here where panel has no window handle
                if (panel.PanelHandle == IntPtr.Zero)
                    return;

                var savedPanelConfig = ActiveProfile.PanelConfigs.FirstOrDefault(s => s.PanelIndex == panel.PanelIndex);

                // Assign previous saved values
                if (savedPanelConfig != null)
                {
                    panel.PanelName = savedPanelConfig.PanelName;
                    panel.Top = savedPanelConfig.Top;
                    panel.Left = savedPanelConfig.Left;
                    panel.Width = savedPanelConfig.Width;
                    panel.Height = savedPanelConfig.Height;
                    panel.FullScreen = savedPanelConfig.FullScreen;
                    panel.AlwaysOnTop = savedPanelConfig.AlwaysOnTop;
                    panel.HideTitlebar = savedPanelConfig.HideTitlebar;
                    panel.TouchEnabled = savedPanelConfig.TouchEnabled;
                }

                // Apply panel name
                if (panel.PanelType == PanelType.CustomPopout)
                {
                    var caption = panel.PanelName + " (Custom)";
                    PInvoke.SetWindowText(panel.PanelHandle, caption);
                    Thread.Sleep(500);
                }

                // Apply locations
                if (panel.Width != 0 && panel.Height != 0)
                {
                    PInvoke.ShowWindow(panel.PanelHandle, PInvokeConstant.SW_RESTORE);
                    Thread.Sleep(250);
                    WindowActionManager.MoveWindow(panel.PanelHandle, panel.PanelType, panel.Left, panel.Top, panel.Width, panel.Height);
                    Thread.Sleep(1000);
                }

                if (!panel.FullScreen)
                {
                    // Apply always on top
                    if (panel.AlwaysOnTop)
                    {
                        WindowActionManager.ApplyAlwaysOnTop(panel.PanelHandle, panel.PanelType, true, new Rectangle(panel.Left, panel.Top, panel.Width, panel.Height));
                        Thread.Sleep(1000);
                    }

                    // Apply hide title bar
                    if (panel.HideTitlebar)
                        WindowActionManager.ApplyHidePanelTitleBar(panel.PanelHandle, true);
                }

                PInvoke.ShowWindow(panel.PanelHandle, PInvokeConstant.SW_RESTORE);
            });

            // Apply full screen (cannot combine with always on top or hide title bar)
            // Cannot run in parallel process
            panelResults.ForEach(panel =>
            {
                if (panel.FullScreen && (!panel.AlwaysOnTop && !panel.HideTitlebar))
                {
                    InputEmulationManager.ToggleFullScreenPanel(panel.PanelHandle);
                    Thread.Sleep(250);
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

