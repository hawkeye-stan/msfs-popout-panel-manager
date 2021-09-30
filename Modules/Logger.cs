using System;

namespace MSFSPopoutPanelManager
{
    public class Logger
    {
        public static event EventHandler<EventArgs<StatusMessage>> OnStatusLogged;

        public static void LogStatus(string message)
        {
            var statusMessage = new StatusMessage() { Message = message, Priority = StatusPriority.Low };
            OnStatusLogged?.Invoke(null, new EventArgs<StatusMessage>(statusMessage));
        }

        public static void LogStatus(string message, StatusPriority priority)
        {
            var statusMessage = new StatusMessage() { Message = message, Priority = priority };
            OnStatusLogged?.Invoke(null, new EventArgs<StatusMessage>(statusMessage));
        }
    }

    public class EventArgs<T> : EventArgs
    {
        public T Value { get; private set; }

        public EventArgs(T val)
        {
            Value = val;
        }
    }

    public class StatusMessage
    {
        public string Message { get; set; }

        public StatusPriority Priority { get; set; }
    }

    public enum StatusPriority
    {
        High,
        Low
    }
}
