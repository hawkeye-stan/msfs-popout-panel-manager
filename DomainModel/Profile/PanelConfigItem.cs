using System;

namespace MSFSPopoutPanelManager.DomainModel.Profile
{
    public class PanelConfigItem
    {
        public IntPtr PanelHandle { get; set; } = IntPtr.Zero;

        public PanelConfigPropertyName PanelConfigProperty { get; set; }
    }

    public enum PanelConfigPropertyName
    {
        PanelName,
        Left,
        Top,
        Width,
        Height,
        AlwaysOnTop,
        HideTitlebar,
        FullScreen,
        TouchEnabled,
        AutoGameRefocus,
        AllowFloatPanel,
        IsHiddenOnStart,
        None
    }
}
