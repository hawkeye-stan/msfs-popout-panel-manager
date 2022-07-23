
namespace MSFSPopoutPanelManager.WebServer
{
    internal class WebSocketSendData
    {
        public string panelName { get; set; }

        public int xPos { get; set; }

        public int yPos { get; set; }

        public int width { get; set; }

        public int height { get; set; }

        public bool alwaysOnTop { get; set; }

        public bool hideTitleBar { get; set; }

        public bool fullScreenMode { get; set; }

        public bool touchEnabled { get; set; }
    }
}
