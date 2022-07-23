using System;

namespace MSFSPopoutPanelManager.Shared
{
    public class TouchPanelOpenEventArg : EventArgs
    {
        public string PlaneId { get; set; }

        public string PanelId { get; set; }

        public string Caption { get; set; }

        public int Width { get; set; }

        public int Height { get; set; }
    }
}
