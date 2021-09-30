using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MSFSPopoutPanelManager
{
    public class PanelManager
    {
        private Form _appForm;
        private const int MSFS_CONNECTION_RETRY_TIMEOUT = 2000;
        private System.Timers.Timer _timer;
        private WindowProcess _simulatorProcess;

        private PanelLocationSelectionModule _panelLocationSelectionModule;
        private PanelAnalysisModule _panelAnalysisModule;
        private UserPlaneProfile _currentPlaneProfile;

        public PanelManager(Form form)
        {
            _appForm = form;

            _panelLocationSelectionModule = new PanelLocationSelectionModule(form);
            _panelAnalysisModule = new PanelAnalysisModule(form);

            _panelLocationSelectionModule.OnSelectionCompleted += (source, e) => { SavePanelSelectionLocation(); };
        }

        public event EventHandler OnSimulatorStarted;
        public event EventHandler OnPanelSettingsChanged;
        public event EventHandler OnAnalysisCompleted;

        public PanelLocationSelectionModule PanelLocationSelection 
        {
            get { return _panelLocationSelectionModule; }
        }

        public UserPlaneProfile CurrentPanelProfile
        { 
            get { return _currentPlaneProfile; }
        }

        public void CheckSimulatorStarted()
        {
            // Autoconnect to flight simulator
            _timer = new System.Timers.Timer();
            _timer.Interval = MSFS_CONNECTION_RETRY_TIMEOUT;
            _timer.Enabled = true;
            _timer.Elapsed += (source, e) =>
            {
                foreach (var process in Process.GetProcesses())
                {
                    if (process.ProcessName == "FlightSimulator" && _simulatorProcess == null)
                    {
                        _simulatorProcess = new WindowProcess()
                        {
                            ProcessId = process.Id,
                            ProcessName = process.ProcessName,
                            Handle = process.MainWindowHandle
                        };

                        _timer.Enabled = false;
                        OnSimulatorStarted?.Invoke(this, null);
                    }
                }
            };
        }

        public void PlaneProfileChanged(int profileId, bool showCoordinateOverlay)
        {
            Logger.LogStatus(String.Empty);

            _currentPlaneProfile = FileManager.GetUserPlaneProfile(profileId);
            _panelLocationSelectionModule.PlaneProfile = _currentPlaneProfile;
            _panelLocationSelectionModule.ShowPanelLocationOverlay(showCoordinateOverlay);
            _panelLocationSelectionModule.UpdatePanelLocationUI();
        }

        public void SetDefaultProfile()
        {
            var userData = FileManager.ReadUserData();
            userData.DefaultProfileId = _currentPlaneProfile.ProfileId;

            FileManager.WriteUserData(userData);

            var profileName = FileManager.ReadPlaneProfileData().Find(x => x.ProfileId == _currentPlaneProfile.ProfileId).ProfileName;

            Logger.LogStatus($"Profile '{profileName}' has been set as default.");
        }

        public void SavePanelSelectionLocation()
        {
            var profileId = _currentPlaneProfile.ProfileId;

            var userData = FileManager.ReadUserData();

            if (userData == null)
                userData = new UserData();

            if (!userData.Profiles.Exists(x => x.ProfileId == profileId))
            {
                userData.Profiles.Add(_currentPlaneProfile);
            }
            else
            {
                var profileIndex = userData.Profiles.FindIndex(x => x.ProfileId == profileId);
                userData.Profiles[profileIndex] = _currentPlaneProfile;
            }

            FileManager.WriteUserData(userData);

            Logger.LogStatus("Panel location coordinates have been saved.");
        }

        public void Analyze()
        {
            if (PanelLocationSelection.PanelCoordinates == null || PanelLocationSelection.PanelCoordinates.Count == 0)
            {
                Logger.LogStatus("No panel locations to be analyze. Please select at least one panel first.");
                return;
            }

            Logger.LogStatus("Panel analysis in progress. Please wait...");
            Thread.Sleep(1000);     // allow time for the mouse to be stopped moving by the user

            PanelLocationSelection.ShowPanelLocationOverlay(false);

            WindowManager.ExecutePopout(_simulatorProcess.Handle, PanelLocationSelection.PanelCoordinates);
            _panelAnalysisModule.Analyze(_simulatorProcess, _currentPlaneProfile.ProfileId, PanelLocationSelection.PanelCoordinates.Count);

            PInvoke.SetForegroundWindow(_appForm.Handle);

            // Get the identified panel windows and previously saved panel destination location
            List<PanelDestinationInfo> panelDestinationList = new List<PanelDestinationInfo>();
            var panels = _simulatorProcess.ChildWindows.FindAll(x => x.WindowType == WindowType.Custom_Popout || x.WindowType == WindowType.BuiltIn_Popout);

            var hasExistingData = _currentPlaneProfile.PanelSettings.PanelDestinationList.Count > 0;

            foreach (var panel in panels)
            {
                if (hasExistingData)
                {
                    var index = _currentPlaneProfile.PanelSettings.PanelDestinationList.FindIndex(x => x.PanelName == panel.Title);
                    _currentPlaneProfile.PanelSettings.PanelDestinationList[index].PanelHandle = panel.Handle;
                }
                else
                {
                    Rect rect = new Rect();
                    var window = PInvoke.GetWindowRect(panel.Handle, out rect);

                    PanelDestinationInfo panelDestinationInfo = new PanelDestinationInfo
                    {
                        PanelName = panel.Title,
                        PanelHandle = panel.Handle,
                        Left = rect.Left,
                        Top = rect.Top,
                        Width = rect.Right - rect.Left,
                        Height = rect.Bottom - rect.Top
                    };

                    _currentPlaneProfile.PanelSettings.PanelDestinationList.Add(panelDestinationInfo);
                }
            }


            OnAnalysisCompleted?.Invoke(this, null);

            if (panelDestinationList.Count > 0)
                Logger.LogStatus("Analysis has been completed. You may now drag the panels to their desire locations.");
            else
                Logger.LogStatus("No panel has been identified.");

            ApplyPanelSettings();
        }

        public void ApplyPanelSettings()
        {
            var panels = _simulatorProcess.ChildWindows.FindAll(x => x.WindowType == WindowType.Custom_Popout || x.WindowType == WindowType.BuiltIn_Popout);

            int applyCount = 0;

            Parallel.ForEach(panels, panel =>
            {
                var panelDestinationInfo = _currentPlaneProfile.PanelSettings.PanelDestinationList.Find(x => x.PanelName == panel.Title);
                if (panelDestinationInfo != null && panelDestinationInfo.Width != 0 && panelDestinationInfo.Height != 0)
                {
                    if (panelDestinationInfo.Left != 0 && panelDestinationInfo.Top != 0)
                    {
                        // Apply locations
                        PInvoke.MoveWindow(panel.Handle, panelDestinationInfo.Left, panelDestinationInfo.Top, panelDestinationInfo.Width, panelDestinationInfo.Height, true);
                        applyCount++;
                    }

                    // Apply always on top
                    Thread.Sleep(300);
                    WindowManager.ApplyAlwaysOnTop(panel.Handle, _currentPlaneProfile.PanelSettings.AlwaysOnTop);

                    // Apply hide title bar
                    Thread.Sleep(300);
                    WindowManager.ApplyHidePanelTitleBar(panel.Handle, _currentPlaneProfile.PanelSettings.HidePanelTitleBar);
                }
            });

            if(applyCount > 0)
                Logger.LogStatus("Previously saved panel settings have been applied.");
            else if (panels.Count > 0 && applyCount == 0)
                Logger.LogStatus("Please move the newly identified panels to their desire locations. Once everything is perfect, click 'Save Settings' and these settings will be used in future flights.");
            else
                Logger.LogStatus("No panel has been found.");
        }

        public void SavePanelSettings()
        {
            var profileId = _currentPlaneProfile.ProfileId;

            // Get latest panel destination locations from screen
            var panels = _simulatorProcess.ChildWindows.FindAll(x => x.WindowType == WindowType.Custom_Popout || x.WindowType == WindowType.BuiltIn_Popout);
            foreach(var panel in panels)
            {
                Rect rect = new Rect();
                var window = PInvoke.GetWindowRect(panel.Handle, out rect);

                var panelDestinationInfo = _currentPlaneProfile.PanelSettings.PanelDestinationList.Find(x => x.PanelName == panel.Title);

                if (panelDestinationInfo == null)
                {
                    panelDestinationInfo = new PanelDestinationInfo() { PanelName = panel.Title };
                    _currentPlaneProfile.PanelSettings.PanelDestinationList.Add(panelDestinationInfo);
                }

                panelDestinationInfo.Left = rect.Left;
                panelDestinationInfo.Top = rect.Top;
                panelDestinationInfo.Width = rect.Right - rect.Left;
                panelDestinationInfo.Height = rect.Bottom - rect.Top;
            }

            var userData = FileManager.ReadUserData();

            if (!userData.Profiles.Exists(x => x.ProfileId == _currentPlaneProfile.ProfileId))
                userData.Profiles.Add(_currentPlaneProfile);
            else
            {
                var profileIndex = userData.Profiles.FindIndex(x => x.ProfileId == _currentPlaneProfile.ProfileId);
                userData.Profiles[profileIndex] = _currentPlaneProfile;
            }

            FileManager.WriteUserData(userData);

            OnPanelSettingsChanged?.Invoke(this, null);

            Logger.LogStatus("Panel settings have been saved.");
        }

        public void UpdatePanelLocationUI()
        {
            _panelLocationSelectionModule.UpdatePanelLocationUI();
        }
    }
}
