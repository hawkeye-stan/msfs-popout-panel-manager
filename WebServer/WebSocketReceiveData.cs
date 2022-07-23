namespace MSFSPopoutPanelManager.WebServer
{
    internal class WebSocketReceiveData
    {
        public WebSocketReceiveData(string k, object v)
        {
            key = k;
            value = v;
        }

        public string key { get; set; }

        public object value { get; set; }
    }
}
