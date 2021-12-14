using System;

namespace MSFSPopoutPanelManager
{
    public class WindowProcess
    {
        public int ProcessId { get; set; }

        public string ProcessName { get; set; }

        public IntPtr Handle { get; set; }
    }
}
