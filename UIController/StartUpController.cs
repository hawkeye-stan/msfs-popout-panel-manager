
using MSFSPopoutPanelManager.Provider;
using MSFSPopoutPanelManager.Shared;
using System;
using System.Windows.Forms;

namespace MSFSPopoutPanelManager.UIController
{
    public class StartUpController : BaseController
    {
        private const int MSFS_CONNECTION_CHECK_INTERVAL = 3000;
        private Form _appForm;
        private System.Timers.Timer _timer;

        public StartUpController(Form form)
        {
            _appForm = form;
            BaseController.OnPopOutCompleted += (source, e) => OnPanelConfigurationActivated?.Invoke(this, null);
            BaseController.OnRestart += (source, e) => OnPanelSelectionActivated?.Invoke(this, null);
        }

        public event EventHandler<EventArgs<bool>> OnSimConnectionChanged;
        public event EventHandler OnPanelSelectionActivated;
        public event EventHandler OnPanelConfigurationActivated;

        public bool IsMinimizeToTray { get; set; }
        public bool IsAlwaysOnTop { get; set; }
        public bool IsAutoStart { get; set; }
        public bool UseAutoPanning { get; set; }

        public void Initialize()
        {
            var appSettings = FileManager.ReadAppSettingData();
            IsMinimizeToTray = appSettings.MinimizeToTray;
            IsAlwaysOnTop = appSettings.AlwaysOnTop;
            UseAutoPanning = appSettings.UseAutoPanning;
            IsAutoStart = Autostart.CheckIsAutoStart();

            OnPanelSelectionActivated?.Invoke(this, null);

            CheckSimulatorStarted();
        }

        public void SetAutoStart(bool isSet)
        {
            if (isSet)
                Autostart.Activate();
            else
                Autostart.Deactivate();
        }

        public void SetMinimizeToTray(bool isSet)
        {
            var appSettingData = FileManager.ReadAppSettingData();
            appSettingData.MinimizeToTray = isSet;
            FileManager.WriteAppSettingData(appSettingData);
        }

        public void SetAutoPanning(bool isSet)
        {
            var appSettingData = FileManager.ReadAppSettingData();
            appSettingData.UseAutoPanning = isSet;
            FileManager.WriteAppSettingData(appSettingData);
        }

        public void SetAlwaysOnTop(bool isSet)
        {
            var appSettingData = FileManager.ReadAppSettingData();
            appSettingData.AlwaysOnTop = isSet;
            FileManager.WriteAppSettingData(appSettingData);

            WindowManager.ApplyAlwaysOnTop(_appForm.Handle, isSet, _appForm.Bounds);
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
    }
}
