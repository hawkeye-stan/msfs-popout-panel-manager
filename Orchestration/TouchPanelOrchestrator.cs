using MSFSPopoutPanelManager.Shared;
using MSFSPopoutPanelManager.TouchPanelAgent;
using MSFSPopoutPanelManager.UserDataAgent;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Dynamic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MSFSPopoutPanelManager.Orchestration
{
    public class TouchPanelOrchestrator : ObservableObject
    {
        private ITouchPanelServer _touchPanelServer;

        internal ProfileData ProfileData { get; set; }

        internal AppSettingData AppSettingData { get; set; }

        public ObservableCollection<PlaneProfile> PlaneProfiles { get; private set; }

        public IntPtr ApplicationHandle { get; set; }

        public void LoadPlaneProfiles()
        {
            if (_touchPanelServer == null || ProfileData.ActiveProfile == null)
                return;

            var dataItems = TouchPanelManager.GetPlaneProfiles();

            // Map plane profiles to bindable objects
            PlaneProfiles = new ObservableCollection<PlaneProfile>();

            foreach (var dataItem in dataItems)
            {
                var planeProfile = new PlaneProfile();
                planeProfile.PlaneId = dataItem.PlaneId;
                planeProfile.Name = dataItem.Name;

                if (dataItem.Panels != null && dataItem.Panels.Count > 0)
                {
                    planeProfile.Panels = new ObservableCollection<PanelProfile>();

                    foreach (var dataItemPanel in dataItem.Panels)
                    {
                        planeProfile.Panels.Add(new PanelProfile()
                        {
                            PlaneId = dataItem.PlaneId,
                            PanelId = dataItemPanel.PanelId,
                            Name = dataItemPanel.Name,
                            Width = dataItemPanel.PanelSize.Width,
                            Height = dataItemPanel.PanelSize.Height,
                            IsSelected = ProfileData.ActiveProfile.TouchPanelBindings == null ? false : ProfileData.ActiveProfile.TouchPanelBindings.Any(b => b.PlaneId == dataItem.PlaneId && b.PanelId == dataItemPanel.PanelId)
                        });
                    }
                }

                PlaneProfiles.Add(planeProfile);
            }
        }

        public void PanelSelected(IList<PlaneProfile> PlaneProfiles)
        {
            ProfileData.ActiveProfile.PanelConfigs.RemoveAll(panelConfig => panelConfig.PanelType == PanelType.MSFSTouchPanel);
            ProfileData.ActiveProfile.TouchPanelBindings.Clear();
            ProfileData.ActiveProfile.IsLocked = false;

            foreach (var planeProfile in PlaneProfiles)
            {
                foreach (var panel in planeProfile.Panels)
                {
                    if (panel.IsSelected)
                        ProfileData.ActiveProfile.TouchPanelBindings.Add(new TouchPanelBinding() { PlaneId = planeProfile.PlaneId, PanelId = panel.PanelId });
                }
            }

            ProfileData.WriteProfiles();
        }

        internal async Task StartServer()
        {
            if (_touchPanelServer == null)
            {
                // This is use to remove all references and dependencies so single file package will not include any ASP.NET core dlls
#if DEBUGTOUCHPANEL || RELEASETOUCHPANEL
                _touchPanelServer = new MSFSPopoutPanelManager.WebServer.WebHost();
#endif
                if (_touchPanelServer != null)
                {
                    _touchPanelServer.WindowHandle = ApplicationHandle;
                    _touchPanelServer.DataRefreshInterval = AppSettingData.AppSetting.TouchPanelSettings.DataRefreshInterval;
                    _touchPanelServer.MapRefreshInterval = AppSettingData.AppSetting.TouchPanelSettings.MapRefreshInterval;
                    _touchPanelServer.IsUsedArduino = AppSettingData.AppSetting.TouchPanelSettings.UseArduino;
                    _touchPanelServer.IsEnabledSound = AppSettingData.AppSetting.TouchPanelSettings.EnableSound;
                    await _touchPanelServer.StartAsync(default(CancellationToken));
                }
            }
        }

        internal async Task StopServer()
        {
            if (_touchPanelServer != null && _touchPanelServer.ServerStarted)
            {
                await _touchPanelServer.StopAsync(default(CancellationToken));
                _touchPanelServer = null;
            }
        }

        internal async Task TouchPanelIntegrationUpdated(bool enabled)
        {
            if ((_touchPanelServer == null || !_touchPanelServer.ServerStarted) && enabled)
                await StartServer();
            else if (_touchPanelServer.ServerStarted && !enabled)
                await StopServer();
        }

        internal async void ReloadTouchPanelSimConnectDataDefinition()
        {
            if (_touchPanelServer == null)
                return;

            // Communicate with Touch Panel API server to reload SimConnect data definitions
            if (ProfileData.ActiveProfile != null && ProfileData.ActiveProfile.TouchPanelBindings.Count > 0 && _touchPanelServer.ServerStarted)
            {
                var planeId = ProfileData.ActiveProfile.TouchPanelBindings.Count > 0 ?
                              ProfileData.ActiveProfile.TouchPanelBindings[0].PlaneId :
                              null;

                using (var client = new HttpClient())
                {
                    dynamic data = new ExpandoObject();
                    data.PlaneId = planeId;
                    var payload = JsonConvert.SerializeObject(data);

                    var url = "http://localhost:27012/posttouchpanelloaded";

                    try
                    {
                        var response = await client.PostAsync(url, new StringContent(payload, Encoding.UTF8, "application/json"));
                        var token = response.Content.ReadAsStringAsync().Result;
                    }
                    catch { }
                }
            }
        }
    }

    public class PlaneProfile : ObservableObject
    {
        public string PlaneId { get; set; }

        public string Name { get; set; }

        public ObservableCollection<PanelProfile> Panels { get; set; }

        public bool HasSelected
        {
            get
            {
                if (Panels == null)
                    return false;

                return Panels.Any(p => p.IsSelected);
            }
        }
    }

    public class PanelProfile : ObservableObject
    {
        public string PlaneId { get; set; }

        public string PanelId { get; set; }

        public string Name { get; set; }

        public int Width { get; set; }

        public int Height { get; set; }

        public bool IsSelected { get; set; }
    }
}
