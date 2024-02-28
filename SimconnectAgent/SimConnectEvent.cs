namespace MSFSPopoutPanelManager.SimConnectAgent
{
    public enum SimConnectEvent
    {
        SIM_START,
        SIM_STOP,
        AIRCRAFT_LOADED,
        VIEW
    }

    public enum ActionEvent
    {
        DUMMY1,         // must register 4 dummy events to reserve space for SimConnectEvent above
        DUMMY2,
        DUMMY3,
        DUMMY4,
        MASTER_BATTERY_SET,
        AVIONICS_MASTER_SET,
        PAUSE_SET,
        SIM_RATE_DECR,
        SIM_RATE_INCR
    }
}
