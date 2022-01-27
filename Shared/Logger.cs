using System;

namespace MSFSPopoutPanelManager.Shared
{
    public class Logger
    {
        public static event EventHandler<EventArgs<StatusMessage>> OnStatusLogged;

        public static void LogStatus(string message, StatusMessageType MessageType)
        {
            var statusMessage = new StatusMessage() { Message = message, MessageType = MessageType };
            OnStatusLogged?.Invoke(null, new EventArgs<StatusMessage>(statusMessage));
        }

        public static void ClearStatus()
        {
            LogStatus(String.Empty, StatusMessageType.Info);
        }
    }

    public class StatusMessage
    {
        public string Message { get; set; }

        public StatusMessageType MessageType { get; set; }
    }

    public enum StatusMessageType
    {
        Info,
        Error
    }

    public class PopoutManagerException : Exception
    {
        public PopoutManagerException(string message) : base(message) { }
    }
}
