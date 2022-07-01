using CommunityToolkit.Mvvm.ComponentModel;
using MSFSPopoutPanelManager.TouchPanel.Shared;
using MSFSPopoutPanelManager.TouchPanel.TouchPanelHost;
using System;
using System.Threading.Tasks;

namespace MSFSPopoutPanelManager.WpfApp.ViewModel
{
    public class TouchPanelManagementViewModel : ObservableObject
    {
        private IWebHost _webHost;
        private IWebApiHost _webApiHost;
        private DataStore _dataStore;

        public string ClientLog { get; set; }

        public string ServerLog { get; set; }

        public bool IsServerStarted { get; set;}

        public bool IsServerActionDisabled { get; set; }

        public DelegateCommand StartServerCommand => new DelegateCommand(OnStartServer, CanExecute);

        public DelegateCommand StopServerCommand => new DelegateCommand(OnStopServer, CanExecute);

        public DelegateCommand RestartServerCommand => new DelegateCommand(OnRestartServer, CanExecute);

        public DelegateCommand ClearServerLogCommand => new DelegateCommand(OnClearServerLog, CanExecute);

        public DelegateCommand ClearClientLogCommand => new DelegateCommand(OnClearClientLog, CanExecute);

        public TouchPanelManagementViewModel(DataStore dataStore)
        {
            _dataStore = dataStore;

            TouchPanelLogger.OnServerLogged += Logger_OnServerLogged;
            TouchPanelLogger.OnClientLogged += Logger_OnClientLogged;
        }

        private void HandleTouchPanelSettingsDataChanged(object sender, Shared.EventArgs<string> e)
        {
            if (_dataStore.AppSetting != null)
            {
                _webApiHost.TouchPanelConfigSetting = new TouchPanelConfigSetting()
                {
                    DataRefreshInterval = _dataStore.AppSetting.TouchPanelSettings.DataRefreshInterval,
                    MapRefreshInterval = _dataStore.AppSetting.TouchPanelSettings.MapRefreshInterval,
                    IsUsedArduino = _dataStore.AppSetting.TouchPanelSettings.UseArduino,
                    IsEnabledSound = _dataStore.AppSetting.TouchPanelSettings.EnableSound
                };

                // shut down server
                if (IsServerStarted && !_dataStore.AppSetting.TouchPanelSettings.EnableIntegration)
                    StopServer();
            }
        }

        public async Task StopServer()
        {
            try
            {
                if (_webApiHost != null && IsServerStarted)
                {
                    await _webApiHost.StopAsync();
                    _webApiHost.Dispose();
                }

                if (_webHost != null && IsServerStarted)
                {
                    await _webHost.StopAsync();
                    _webHost.Dispose();
                }

                IsServerStarted = false;
                IsServerActionDisabled = false;
            }
            catch { }
        }

        private async void OnStartServer(object commandParameter)
        {
            OnClearServerLog(null);
            OnClearClientLog(null);

            IsServerActionDisabled = true;

            if (_webHost == null)
                _webHost = new WebHost(_dataStore.ApplicationHandle);

            await _webHost.StartAsync();

            if (_webApiHost == null)
            {
                _webApiHost = new WebApiHost(_dataStore.ApplicationHandle);
                _dataStore.AppSetting.TouchPanelSettings.DataChanged += HandleTouchPanelSettingsDataChanged;
                HandleTouchPanelSettingsDataChanged(this, null);
            }

            await _webApiHost.StartAsync();

            IsServerStarted = true;
            IsServerActionDisabled = false;


        }
     

        private async void OnRestartServer(object commandParameter)
        {
            IsServerActionDisabled = true;

            if (_webHost != null)
                await _webHost.RestartAsync();

            if(_webApiHost != null)
                await _webApiHost.RestartAsync();

            IsServerStarted = true;
            IsServerActionDisabled = false;
        }

        private async void OnStopServer(object commandParameter)
        {
            IsServerActionDisabled = true;

            await StopServer();

            IsServerStarted = false;
            IsServerActionDisabled = false;
        }

        private void OnClearClientLog(object commandParameter)
        {
            ClientLog = null;
        }

        private void OnClearServerLog(object commandParameter)
        {
            ServerLog = null;
        }

        private void Logger_OnClientLogged(object sender, TouchPanel.Shared.EventArgs<string> e)
        {
            if(!String.IsNullOrEmpty(e.Value))
            {
                if (String.IsNullOrEmpty(ClientLog))
                    ClientLog += e.Value;
                else
                    ClientLog += (Environment.NewLine + e.Value);
            }
        }

        private void Logger_OnServerLogged(object sender, TouchPanel.Shared.EventArgs<string> e)
        {
            if (!String.IsNullOrEmpty(e.Value))
            {
                if (String.IsNullOrEmpty(ServerLog))
                    ServerLog += e.Value;
                else
                    ServerLog += (Environment.NewLine + e.Value);
            }
        }

        private bool CanExecute(object commandParameter)
        {
            return true;
        }
    }
}
