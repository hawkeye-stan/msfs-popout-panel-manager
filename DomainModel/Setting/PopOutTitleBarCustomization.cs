using MSFSPopoutPanelManager.Shared;

namespace MSFSPopoutPanelManager.DomainModel.Setting
{
    public class PopOutTitleBarCustomization : ObservableObject
    {
        public bool IsEnabled { get; set; } = false;

        public string HexColor { get; set; } = "000000";
    }
}
