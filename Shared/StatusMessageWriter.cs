using System;

namespace MSFSPopoutPanelManager.Shared
{
    public class StatusMessageWriter
    {
        public static event EventHandler<StatusMessageEventArg> OnStatusMessage;

        public static void WriteMessage(string message, StatusMessageType statusMessageType, bool showDialog, int duration, bool displayInAppWindow)
        {
            OnStatusMessage?.Invoke(null, new StatusMessageEventArg(message, statusMessageType, showDialog, duration, displayInAppWindow));
        }

        public static void WriteMessage(string message, StatusMessageType statusMessageType, bool showDialog)
        {
            OnStatusMessage?.Invoke(null, new StatusMessageEventArg(message, statusMessageType, showDialog));
        }
    }
}
