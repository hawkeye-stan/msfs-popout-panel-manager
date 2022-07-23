namespace MSFSPopoutPanelManager.Shared
{
    public class Constants
    {
        public const int WEB_HOST_PORT = 27011;
        public const int API_HOST_PORT = 27012;

        public static string WEB_HOST_URI { get { return $"http://localhost:{WEB_HOST_PORT}"; } }
    }
}
