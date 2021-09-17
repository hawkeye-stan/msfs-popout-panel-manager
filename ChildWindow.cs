using System;

namespace MSFSPopoutPanelManager
{
    public class ChildWindow
    {
        public ChildWindow()
        {
            PopoutType = PopoutType.BuiltIn;
        }

        public int ProcessId { get; set; }

        public string Title { get; set; }

        public IntPtr Handle { get; set; }

        public string ClassName { get; set; }

        public PopoutType PopoutType { get; set; }
    }

    public enum PopoutType
    {
        BuiltIn,
        Custom
    }
}
