using System;

namespace MSFSPopoutPanelManager.DomainModel.Profile
{
    public class PanelConfigItem
    {
        public PanelConfigItem()
        {
            PanelHandle = IntPtr.Zero;
        }

        public IntPtr PanelHandle { get; set; }

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
        None
    }
}
