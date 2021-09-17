using System;
using System.Collections.Generic;

namespace MSFSPopoutPanelManager
{
    public class MainWindow
    {
        public MainWindow()
        {
            ChildWindowsData = new List<ChildWindow>();
        }

        public int ProcessId { get; set; }

        public string ProcessName { get; set; }

        public string Title { get; set; }

        public IntPtr Handle { get; set; }

        public List<ChildWindow> ChildWindowsData { get; set; }

    }
}
