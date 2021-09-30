using System;

namespace MSFSPopoutPanelManager
{
    public class ChildWindow
    {
        public ChildWindow()
        {
            WindowType = WindowType.Undetermined;
        }

        public IntPtr Handle { get; set; }

        public string Title { get; set; }

        public string ClassName { get; set; }

        public WindowType WindowType { get; set; }
    }
}
