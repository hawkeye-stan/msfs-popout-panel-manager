namespace MSFSPopoutPanelManager.UIController
{
    public interface IPanelConfigurationView
    {
        public bool IsPanelLocked { set; }

        public bool IsPanelChangeDisabled { set; }
    }
}
