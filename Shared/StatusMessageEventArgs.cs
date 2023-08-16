using System;
using System.Collections.Generic;

namespace MSFSPopoutPanelManager.Shared
{
    public class StatusMessageEventArg : EventArgs
    {
        public StatusMessageEventArg(List<StatusMessage> messages)
        {
            Messages = messages;
        }

        public List<StatusMessage> Messages { get; set; }
    }

    public class StatusMessage
    {
        public StatusMessageType StatusMessageType { get; set; }

        public string Message { get; set; }

        public bool NewLine { get; set; }
    }
}
