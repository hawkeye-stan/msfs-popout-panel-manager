namespace MSFSPopoutPanelManager.SimConnectAgent
{
    public enum DATA_DEFINITION
    {
        REQUIRED_DEFINITION = 0,
        HUDBAR_DEFINITION,
        WRITEABLE_TRACKIR_DEFINITION,
        WRITEABLE_COCKPITCAMERAZOOM_DEFINITION,
        WRITEABLE_CAMERAREQUESTACTION_DEFINITION,
        NA
    }

    public enum DATA_REQUEST
    {
        REQUIRED_REQUEST = 0,
        HUDBAR_REQUEST,
        NA
    }

    public enum NotificationGroup
    {
        GROUP0
    }

    public enum SystemStateRequestId
    {
        AIRCRAFTPATH
    }
}
