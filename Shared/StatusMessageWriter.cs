using System;
using System.Collections.Generic;

namespace MSFSPopoutPanelManager.Shared
{
    public class StatusMessageWriter
    {
        private static List<StatusMessage> _messages = new List<StatusMessage>();

        public static event EventHandler<StatusMessageEventArg> OnStatusMessage;

        public static void WriteMessage(string message, StatusMessageType statusMessageType, int duration = -1)
        {
            _messages.Add(new StatusMessage { Message = message, StatusMessageType = statusMessageType, NewLine = false });

            if (IsEnabled)
                OnStatusMessage?.Invoke(null, new StatusMessageEventArg(_messages, duration));
        }

        public static void WriteMessageNewLine(string message, StatusMessageType statusMessageType, int duration = -1)
        {
            _messages.Add(new StatusMessage { Message = message, StatusMessageType = statusMessageType, NewLine = true });

            if (IsEnabled)
                OnStatusMessage?.Invoke(null, new StatusMessageEventArg(_messages, duration));
        }

        public static void WriteOkStatusMessage(int duration = -1)
        {
            _messages.Add(new StatusMessage { Message = "  (OK)", StatusMessageType = StatusMessageType.Success, NewLine = true });

            if (IsEnabled)
                OnStatusMessage?.Invoke(null, new StatusMessageEventArg(_messages, duration));
        }

        public static void WriteFailureStatusMessage(int duration = -1)
        {
            _messages.Add(new StatusMessage { Message = "  (FAILED)", StatusMessageType = StatusMessageType.Failure, NewLine = true });

            if (IsEnabled)
                OnStatusMessage?.Invoke(null, new StatusMessageEventArg(_messages, duration));
        }

        public static void ClearMessage()
        {
            _messages.Clear();
            OnStatusMessage?.Invoke(null, new StatusMessageEventArg(new List<StatusMessage>()));
        }

        public static bool IsEnabled { get; set; }
    }
}
