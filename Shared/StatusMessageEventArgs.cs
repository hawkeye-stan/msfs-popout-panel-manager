using System;
using System.Collections.Generic;

namespace MSFSPopoutPanelManager.Shared
{
    public class StatusMessageEventArg : EventArgs
    {
        public StatusMessageEventArg(List<StatusMessage> messages)
        {
            Messages = messages;
            Duration = -1;
        }

        public StatusMessageEventArg(List<StatusMessage> messages, int duration)
        {
            Messages = messages;
            Duration = duration;
        }

        public List<StatusMessage> Messages { get; set; }

        public StatusMessageType StatusMessageType { get; set; }

        public int Duration { get; set; }
    }

    public class StatusMessage
    {
        public StatusMessageType StatusMessageType { get; set; }

        public string Message { get; set; }

        public bool NewLine { get; set; }
    }
}
