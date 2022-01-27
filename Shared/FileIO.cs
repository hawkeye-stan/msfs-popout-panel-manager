using System.IO;

namespace MSFSPopoutPanelManager.Shared
{
    public class FileIo
    {
        public static string GetUserDataFilePath()
        {
            var startupPath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            return Path.Combine(startupPath, "userdata");
        }
    }
}
