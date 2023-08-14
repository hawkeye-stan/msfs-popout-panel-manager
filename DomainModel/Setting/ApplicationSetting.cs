using MSFSPopoutPanelManager.Shared;
using System;

namespace MSFSPopoutPanelManager.DomainModel.Setting
{
    public class ApplicationSetting : ObservableObject
    {
        public ApplicationSetting()
        {
            GeneralSetting = new GeneralSetting();
            AutoPopOutSetting = new AutoPopOutSetting();
            PopOutSetting = new PopOutSetting();
            RefocusSetting = new RefocusSetting();
            TouchSetting = new TouchSetting();
            TrackIRSetting = new TrackIRSetting();
            WindowedModeSetting = new WindowedModeSetting();
            SystemSetting = new SystemSetting();
            KeyboardShortcutSetting = new KeyboardShortcutSetting();

            InitializeChildPropertyChangeBinding();

            this.PropertyChanged += (sender, e) =>
            {
                var evtArg = e as PropertyChangedExtendedEventArgs;

                if (evtArg.ObjectName == "MSFSPopoutPanelManager.DomainModel.Setting.KeyboardShortcutSetting" && evtArg.PropertyName == "IsEnabled")
                    IsUsedKeyboardShortcutChanged?.Invoke(this, KeyboardShortcutSetting.IsEnabled);
            };
        }

        public GeneralSetting GeneralSetting { get; set; }

        public AutoPopOutSetting AutoPopOutSetting { get; set; }

        public PopOutSetting PopOutSetting { get; set; }

        public RefocusSetting RefocusSetting { get; set; }

        public TouchSetting TouchSetting { get; set; }

        public TrackIRSetting TrackIRSetting { get; set; }

        public WindowedModeSetting WindowedModeSetting { get; set; }

        public SystemSetting SystemSetting { get; set; }

        public KeyboardShortcutSetting KeyboardShortcutSetting { get; set; }

        public event EventHandler<bool> IsUsedKeyboardShortcutChanged;
    }
}
