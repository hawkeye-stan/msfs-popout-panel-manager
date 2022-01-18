
using MSFSPopoutPanelManager.Provider;
using MSFSPopoutPanelManager.Shared;
using System;

namespace MSFSPopoutPanelManager.UIController
{
    public class ApplicationController
    {
        private const int MSFS_CONNECTION_CHECK_INTERVAL = 3000;
        private System.Timers.Timer _timer;
        private IApplicationView _view;
        private AppSettingData _appSettings;

        protected DataStore UserProfileDataStore { get; set; }

        public ApplicationController(IApplicationView view)
        {
            _view = view;
            UserProfileDataStore = new DataStore();
        }

        public PanelSelectionController PanelSelectionController { get; set; }

        public PanelConfigurationController PanelConfigurationController { get; set; }

        public event EventHandler<EventArgs<bool>> OnSimConnectionChanged;

        public event EventHandler OnPanelSelectionActivated;

        public event EventHandler OnPanelConfigurationActivated;

        public void Initialize()
        {
            PanelSelectionController = new PanelSelectionController(_view.PanelSelection, UserProfileDataStore);
            PanelConfigurationController = new PanelConfigurationController(_view.PanelConfiguration, UserProfileDataStore);

            PanelSelectionController.OnPopOutCompleted += (source, e) =>
            {
                PanelConfigurationController.Initialize();
                OnPanelConfigurationActivated?.Invoke(this, null);
            };

            OnPanelSelectionActivated?.Invoke(this, null);
            CheckSimulatorStarted();

            _appSettings = FileManager.ReadAppSettingData();
            _view.AlwaysOnTop = _appSettings.AlwaysOnTop;
            _view.AutoPanning = _appSettings.UseAutoPanning;
            _view.AutoStart = Autostart.CheckIsAutoStart();
            _view.MinimizeToTray = _appSettings.MinimizeToTray;

            if (_view.AlwaysOnTop)
                _view.Form.TopMost = true;
        }

        private void CheckSimulatorStarted()
        {
            OnSimConnectionChanged?.Invoke(this, new EventArgs<bool>(false));

            // Autoconnect to flight simulator
            _timer = new System.Timers.Timer();
            _timer.Interval = MSFS_CONNECTION_CHECK_INTERVAL;
            _timer.Enabled = true;
            _timer.Elapsed += (source, e) =>
            {
                var simulatorProcess = WindowManager.GetSimulatorProcess();
                OnSimConnectionChanged?.Invoke(this, new EventArgs<bool>(simulatorProcess != null));
            };
        }

        public void Restart()
        {
            PanelConfigurationController.UnhookWinEvent();

            // Try to close all Cutome Panel window
            UserProfileDataStore.ActiveUserProfile.PanelConfigs.FindAll(p => p.PanelType == PanelType.CustomPopout).ForEach(panel => WindowManager.CloseWindow(panel.PanelHandle));

            OnPanelSelectionActivated?.Invoke(this, null);
            Logger.ClearStatus();
        }

        public void SetMinimizeToTray(bool value) { 
            _appSettings.MinimizeToTray = value;
            FileManager.WriteAppSettingData(_appSettings);
        }

        public void SetAlwaysOnTop(bool value)
        {
            _appSettings.AlwaysOnTop = value;
            FileManager.WriteAppSettingData(_appSettings);

            WindowManager.ApplyAlwaysOnTop(_view.Form.Handle, value, _view.Form.Bounds);
        }

        public void SetAutoStart(bool value)
        {
            if (value)
                Autostart.Activate();
            else
                Autostart.Deactivate();
        }

        public void SetAutoPanning(bool value)
        {
            _appSettings.UseAutoPanning = value;
            FileManager.WriteAppSettingData(_appSettings);
        }

        public void MinimizeAllPanels(bool isMinimized)
        {
            if (isMinimized)
            {
                PanelConfigurationController.DisablePanelChanges(true);
                PInvoke.EnumWindows(new PInvoke.CallBack(EnumAllPanels), 0);
            }
            else
            {
                PanelConfigurationController.DisablePanelChanges(false);
                PInvoke.EnumWindows(new PInvoke.CallBack(EnumAllPanels), 1);
            }
        }

        public bool EnumAllPanels(IntPtr hwnd, int index)
        {
            var className = PInvoke.GetClassName(hwnd);
            var caption = PInvoke.GetWindowText(hwnd);

            if (className == "AceApp" && caption.IndexOf("Microsoft Flight Simulator") == -1)      // MSFS windows designation
            {
                if(index == 0)
                    PInvoke.ShowWindow(hwnd, PInvokeConstant.SW_MINIMIZE);
                else
                    PInvoke.ShowWindow(hwnd, PInvokeConstant.SW_RESTORE);
            }

            return true;
        }
    }
}
