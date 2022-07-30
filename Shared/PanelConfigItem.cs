namespace MSFSPopoutPanelManager.Shared
{
    public class PanelConfigItem
    {
        public int PanelIndex { get; set; }

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
        Invalid
    }
}
