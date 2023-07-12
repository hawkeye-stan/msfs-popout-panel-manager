namespace MSFSPopoutPanelManager.DomainModel.Setting
{
    public class PopOutTitleBarCustomization
    {
        public PopOutTitleBarCustomization()
        {
            IsEnabled = false;
            HexColor = "000000";
        }

        public bool IsEnabled { get; set; }

        public string HexColor { get; set; }
    }
}
