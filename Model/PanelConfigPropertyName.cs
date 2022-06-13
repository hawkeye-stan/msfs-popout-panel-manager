using System;

namespace MSFSPopoutPanelManager.Model
{
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
        Invalid
    }

    public class PanelConfigItem
    {
        public int PanelIndex { get; set; }

        public PanelConfigPropertyName PanelConfigProperty { get; set; }
    }
}
