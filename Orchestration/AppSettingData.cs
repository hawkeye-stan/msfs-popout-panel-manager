using MSFSPopoutPanelManager.DomainModel.Setting;
using MSFSPopoutPanelManager.Shared;
using System;
using System.Collections.Specialized;

namespace MSFSPopoutPanelManager.Orchestration
{
    public class AppSettingData : ObservableObject
    {
        public event EventHandler<bool> OnAlwaysOnTopChanged;
        public event EventHandler<bool> OnEnablePanelResetWhenLockedChanged;

        public ApplicationSetting ApplicationSetting { get; private set; }

        public void ReadSettings()
        {
            ApplicationSetting = AppSettingDataManager.ReadAppSetting();

            if (ApplicationSetting == null)
            {
                ApplicationSetting = new ApplicationSetting();
                AppSettingDataManager.WriteAppSetting(ApplicationSetting);
            }
            
            // Auto Save data
            ApplicationSetting.PropertyChanged += (_, e) =>
            {
                AppSettingDataManager.WriteAppSetting(ApplicationSetting);

                switch (e.PropertyName)
                {
                    case "AlwaysOnTop":
                        OnAlwaysOnTopChanged?.Invoke(this, ApplicationSetting.GeneralSetting.AlwaysOnTop);
                        break;
                    case "EnablePanelResetWhenLocked":
                        OnEnablePanelResetWhenLockedChanged?.Invoke(this,
                            ApplicationSetting.PopOutSetting.EnablePanelResetWhenLocked);
                        break;
                }
            };
        }
    }
}
