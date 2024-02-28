using System;
using System.Collections.Generic;

namespace MSFSPopoutPanelManager.Shared
{
    public class StatusMessageWriter
    {
        private static readonly List<StatusMessage> Messages = new();

        public static event EventHandler<List<StatusMessage>> OnStatusMessage;

        public static void WriteMessage(string message, StatusMessageType statusMessageType)
        {
            Messages.Add(new StatusMessage { Message = message, StatusMessageType = statusMessageType });

            if (IsEnabled)
                OnStatusMessage?.Invoke(null, Messages);
        }

        public static void WriteMessageWithNewLine(string message, StatusMessageType statusMessageType)
        {
            Messages.Add(new StatusMessage { Message = message, StatusMessageType = statusMessageType, NewLine = true });

            if (IsEnabled)
                OnStatusMessage?.Invoke(null, Messages);
        }

        public static void WriteExecutingStatusMessage()
        {
            Messages.Add(new StatusMessage { Message = "  (Executing)", StatusMessageType = StatusMessageType.Executing, NewLine = false });

            if (IsEnabled)
                OnStatusMessage?.Invoke(null, Messages);
        }

        public static void WriteOkStatusMessage()
        {
            Messages.Add(new StatusMessage { Message = "  (OK)", StatusMessageType = StatusMessageType.Success, NewLine = true });

            if (IsEnabled)
                OnStatusMessage?.Invoke(null, Messages);
        }

        public static void WriteFailureStatusMessage()
        {
            Messages.Add(new StatusMessage { Message = "  (FAILED)", StatusMessageType = StatusMessageType.Failure, NewLine = true });

            if (IsEnabled)
                OnStatusMessage?.Invoke(null, Messages);
        }

        public static void RemoveLastMessage()
        {
            Messages.RemoveAt(Messages.Count - 1);
        }

        public static void ClearMessage()
        {
            Messages.Clear();
            OnStatusMessage?.Invoke(null, new List<StatusMessage>());
        }

        public static bool IsEnabled { get; set; }
    }
}
