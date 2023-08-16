using System;
using System.Collections.Generic;

namespace MSFSPopoutPanelManager.Shared
{
    public class StatusMessageWriter
    {
        private static List<StatusMessage> _messages = new List<StatusMessage>();

        public static event EventHandler<List<StatusMessage>> OnStatusMessage;

        public static void WriteMessage(string message, StatusMessageType statusMessageType)
        {
            _messages.Add(new StatusMessage { Message = message, StatusMessageType = statusMessageType });

            if (IsEnabled)
                OnStatusMessage?.Invoke(null, _messages);
        }

        public static void WriteMessageWithNewLine(string message, StatusMessageType statusMessageType)
        {
            _messages.Add(new StatusMessage { Message = message, StatusMessageType = statusMessageType, NewLine = true });

            if (IsEnabled)
                OnStatusMessage?.Invoke(null, _messages);
        }

        public static void WriteExecutingStatusMessage()
        {
            _messages.Add(new StatusMessage { Message = "  (Executing)", StatusMessageType = StatusMessageType.Executing, NewLine = false });

            if (IsEnabled)
                OnStatusMessage?.Invoke(null, _messages);
        }

        public static void WriteOkStatusMessage()
        {
            _messages.Add(new StatusMessage { Message = "  (OK)", StatusMessageType = StatusMessageType.Success, NewLine = true });

            if (IsEnabled)
                OnStatusMessage?.Invoke(null, _messages);
        }

        public static void WriteFailureStatusMessage()
        {
            _messages.Add(new StatusMessage { Message = "  (FAILED)", StatusMessageType = StatusMessageType.Failure, NewLine = true });

            if (IsEnabled)
                OnStatusMessage?.Invoke(null, _messages);
        }

        public static void RemoveLastMessage()
        {
            _messages.RemoveAt(_messages.Count - 1);
        }

        public static void ClearMessage()
        {
            _messages.Clear();
            OnStatusMessage?.Invoke(null, new List<StatusMessage>());
        }

        public static bool IsEnabled { get; set; }
    }
}
