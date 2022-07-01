using System;

namespace MSFSPopoutPanelManager.TouchPanel.Shared
{
    public class TouchPanelLogger
    {
        public static event EventHandler<EventArgs<string>> OnServerLogged;
        public static event EventHandler<EventArgs<string>> OnClientLogged;

        public static void ServerLog(string message, LogLevel logLevel)
        {
            var log = $"{logLevel}: {message}";
            OnServerLogged?.Invoke(null, new EventArgs<string>(log));
        }

        public static void ClientLog(string message, LogLevel logLevel)
        {
            var log = $"{logLevel}: {message}";
            OnClientLogged?.Invoke(null, new EventArgs<string>(log));
        }
    }

    public enum LogLevel
    {
        INFO,
        ERROR,
        TRACE
    }
}
