using System;

namespace MSFSPopoutPanelManager.Shared
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
        DisableGameRefocus,
        RowHeader,
        None,
        Invalid,
    }
}
