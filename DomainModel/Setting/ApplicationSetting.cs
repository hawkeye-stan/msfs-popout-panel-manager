using MSFSPopoutPanelManager.Shared;
using System;

namespace MSFSPopoutPanelManager.DomainModel.Setting
{
    public class ApplicationSetting : ObservableObject
    {
        public ApplicationSetting()
        {
            InitializeChildPropertyChangeBinding();

            PropertyChanged += (_, e) =>
            {
                if (e is PropertyChangedExtendedEventArgs { ObjectName: "MSFSPopoutPanelManager.DomainModel.Setting.KeyboardShortcutSetting", PropertyName: "IsEnabled" })
                    OnIsUsedKeyboardShortcutChanged?.Invoke(this, KeyboardShortcutSetting.IsEnabled);
            };
        }

        public GeneralSetting GeneralSetting { get; set; } = new();

        public AutoPopOutSetting AutoPopOutSetting { get; set; } = new();

        public PopOutSetting PopOutSetting { get; set; } = new();

        public RefocusSetting RefocusSetting { get; set; } = new();

        public TouchSetting TouchSetting { get; set; } = new();

        public TrackIRSetting TrackIRSetting { get; set; } = new();

        public WindowedModeSetting WindowedModeSetting { get; set; } = new();

        public SystemSetting SystemSetting { get; set; } = new();

        public KeyboardShortcutSetting KeyboardShortcutSetting { get; set; } = new();

        public DynamicLodSetting DynamicLodSetting { get; set; } = new();

        public event EventHandler<bool> OnIsUsedKeyboardShortcutChanged;
    }
}
