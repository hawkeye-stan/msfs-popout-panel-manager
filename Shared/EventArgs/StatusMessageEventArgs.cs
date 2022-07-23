using System;

namespace MSFSPopoutPanelManager.Shared
{
    public class StatusMessageEventArg : EventArgs
    {
        public StatusMessageEventArg(string message, StatusMessageType statusMessageType, bool showDialog)
        {
            Message = message;
            StatusMessageType = statusMessageType;
            ShowDialog = showDialog;
            Duration = -1;
            DisplayInAppWindow = false;
        }

        public StatusMessageEventArg(string message, StatusMessageType statusMessageType, bool showDialog, int duration, bool displayInAppWindow)
        {
            Message = message;
            StatusMessageType = statusMessageType;
            ShowDialog = showDialog;
            Duration = duration;
            DisplayInAppWindow = displayInAppWindow;
        }

        public string Message { get; set; }

        public StatusMessageType StatusMessageType { get; set; }

        public bool ShowDialog { get; set; }

        public int Duration { get; set; }

        public bool DisplayInAppWindow { get; set; }
    }

    public enum StatusMessageType
    {
        Info,
        Error,
        Debug
    }
}
