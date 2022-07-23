namespace MSFSPopoutPanelManager.SimConnectAgent
{
    public enum ActionEvent
    {
        KEY_MASTER_BATTERY_SET,
        KEY_ALTERNATOR_SET,
        KEY_AVIONICS_MASTER_SET,        // must have this dummy event register first for "TOGGLE_AVIONICS_MASTER" to work, weird!
        KEY_TOGGLE_AVIONICS_MASTER,
    }
}
