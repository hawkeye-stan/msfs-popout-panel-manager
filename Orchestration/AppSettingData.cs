using MSFSPopoutPanelManager.Shared;
using MSFSPopoutPanelManager.UserDataAgent;
using System;

namespace MSFSPopoutPanelManager.Orchestration
{
    public class AppSettingData : ObservableObject
    {
        public event EventHandler AppSettingUpdated;
        public event EventHandler<bool> AlwaysOnTopChanged;
        public event EventHandler<bool> AutoPopOutPanelsChanged;
        public event EventHandler<bool> TouchPanelIntegrationChanged;

        public AppSetting AppSetting { get; private set; }

        public void ReadSettings()
        {
            AppSetting = AppSettingManager.ReadAppSetting();

            if (AppSetting == null)
            {
                AppSetting = new AppSetting();
                AppSettingManager.WriteAppSetting(AppSetting);
            }

            // Autosave data
            AppSetting.PropertyChanged += (sender, e) =>
            {
                var arg = e as PropertyChangedExtendedEventArgs;

                if (arg.OldValue != arg.NewValue)
                {
                    AppSettingManager.WriteAppSetting(AppSetting);

                    switch (arg.PropertyName)
                    {
                        case "AlwaysOnTop":
                            AlwaysOnTopChanged?.Invoke(this, (bool)arg.NewValue);
                            break;
                        case "AutoPopOutPanels":
                            AutoPopOutPanelsChanged?.Invoke(this, (bool)arg.NewValue);
                            break;
                        case "EnableTouchPanelIntegration":
                            TouchPanelIntegrationChanged?.Invoke(this, (bool)arg.NewValue);
                            break;
                    }

                    AppSettingUpdated?.Invoke(this, null);
                }
            };
        }
    }
}
