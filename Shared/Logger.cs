using System;

namespace MSFSPopoutPanelManager.Shared
{
    public class Logger
    {
        public static event EventHandler<EventArgs<StatusMessage>> OnStatusLogged;
        public static event EventHandler<EventArgs<StatusMessage>> OnBackgroundStatusLogged;

        public static void Status(string message, StatusMessageType MessageType)
        {
            var statusMessage = new StatusMessage() { Message = message, MessageType = MessageType };
            OnStatusLogged?.Invoke(null, new EventArgs<StatusMessage>(statusMessage));
        }

        public static void ClearStatus()
        {
            Status(String.Empty, StatusMessageType.Info);
        }

        public static void BackgroundStatus(string message, StatusMessageType MessageType)
        {
            var statusMessage = new StatusMessage() { Message = message, MessageType = MessageType };
            OnBackgroundStatusLogged?.Invoke(null, new EventArgs<StatusMessage>(statusMessage));
        }

        public static void ClearBackgroundStatus()
        {
            BackgroundStatus(String.Empty, StatusMessageType.Info);
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
