using System;
using System.Collections.Generic;

namespace MSFSPopoutPanelManager
{
    public class WindowProcess
    {
        public WindowProcess()
        {
            ChildWindows = new List<ChildWindow>();
        }

        public int ProcessId { get; set; }

        public string ProcessName { get; set; }

        public IntPtr Handle { get; set; }

        public List<ChildWindow> ChildWindows { get; set; }
    }
}
