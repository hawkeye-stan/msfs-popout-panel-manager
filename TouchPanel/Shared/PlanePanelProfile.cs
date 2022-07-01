using System.Collections.Generic;

namespace MSFSPopoutPanelManager.TouchPanel.Shared
{
    public class PlaneProfileInfo
    {
        public string PlaneId { get; set; }

        public string Name { get; set; }

        public List<PanelInfo> Panels { get; set; }
    }

    public class PanelInfo
    {
        public string PanelId { get; set; }

        public string Name { get; set; }

        public string RootPath { get; set; }

        public PanelSize PanelSize { get; set; }

        public List<SubPanelInfo> SubPanels { get; set; }

        public bool ShowMenuBar { get; set; }

        public bool EnableMap { get; set; }
    }

    public class SubPanelInfo
    {
        public string PanelId { get; set; }

        public string Name { get; set; }

        public string RootPath { get; set; }

        public string Definitions { get; set; }

        public int Left { get; set; }

        public int Top { get; set; }

        public float Scale { get; set; }
    }

    public class PanelSize
    {
        public int Width { get; set; }

        public int Height { get; set; }
    }
}
