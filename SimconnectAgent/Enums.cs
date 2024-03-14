namespace MSFSPopoutPanelManager.SimConnectAgent
{
    public enum DataDefinition
    {
        REQUIRED_DEFINITION = 0,
        HUDBAR_DEFINITION,
        DYNAMICLOD_DEFINITION,
        WRITABLE_TRACK_IR_DEFINITION,
        WRITABLE_COCKPIT_CAMERA_ZOOM_DEFINITION,
        WRITABLE_COCKPIT_CAMERA_STATE_DEFINITION,
        WRITABLE_CAMERA_REQUEST_ACTION_DEFINITION,
        WRITABLE_CAMERA_VIEW_TYPE_INDEX_0_DEFINITION,
        WRITABLE_CAMERA_VIEW_TYPE_INDEX_1_DEFINITION,
        NA
    }

    public enum DataRequest
    {
        REQUIRED_REQUEST = 0,
        HUDBAR_REQUEST,
        DYNAMICLOD_REQUEST,
        NA
    }

    public enum NotificationGroup
    {
        GROUP0
    }

    public enum SystemStateRequestId
    {
        AIRCRAFT_PATH
    }
}
