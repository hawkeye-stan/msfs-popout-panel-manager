using System;

namespace MSFSPopoutPanelManager.Provider
{
    public class WindowProcess
    {
        public int ProcessId { get; set; }

        public string ProcessName { get; set; }

        public IntPtr Handle { get; set; }
    }
}
