using MSFSPopoutPanelManager.DomainModel.Setting;
using MSFSPopoutPanelManager.Shared;
using System;

namespace MSFSPopoutPanelManager.Orchestration
{
    public class AppSettingData : ObservableObject
    {
        public event EventHandler<bool> AlwaysOnTopChanged;
        public event EventHandler<bool> EnablePanelResetWhenLockedChanged;

        public ApplicationSetting ApplicationSetting { get; private set; }

        public void ReadSettings()
        {
            ApplicationSetting = AppSettingDataManager.ReadAppSetting();

            if (ApplicationSetting == null)
            {
                ApplicationSetting = new ApplicationSetting();
                AppSettingDataManager.WriteAppSetting(ApplicationSetting);
            }

            // Autosave data
            ApplicationSetting.PropertyChanged += (sender, e) =>
            {
                AppSettingDataManager.WriteAppSetting(ApplicationSetting);

                switch (e.PropertyName)
                {
                    case "AlwaysOnTop":
                        AlwaysOnTopChanged?.Invoke(this, ApplicationSetting.GeneralSetting.AlwaysOnTop);
                        break;
                    case "EnablePanelResetWhenLocked":
                        EnablePanelResetWhenLockedChanged?.Invoke(this, ApplicationSetting.PopOutSetting.EnablePanelResetWhenLocked);
                        break;
                }
            };
        }
    }
}
