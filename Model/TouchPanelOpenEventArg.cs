using System;

namespace MSFSPopoutPanelManager.Model
{
    public class TouchPanelOpenEventArg
    {
        public string PlaneId { get; set; }

        public string PanelId { get; set; }

        public string Caption { get; set; }

        public int Width { get; set; }

        public int Height { get; set; }
    }
}
