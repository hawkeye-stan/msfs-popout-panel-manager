using System;

namespace MSFSPopoutPanelManager.Shared
{
    public class DialogMessageEventArg : EventArgs
    {
        public DialogMessageEventArg(string message, MessageDialogIcon messageDialogIcon)
        {
            Message = message;
            MessageDialogIcon = messageDialogIcon;
        }

        public string Message { get; set; }

        public MessageDialogIcon MessageDialogIcon { get; set; }
    }

    public enum MessageDialogIcon
    {
        Info,
        Success,
        Failed
    }
}
