using System;
using System.IO;

namespace MSFSPopoutPanelManager.Shared
{
    public class FileIo
    {
        public static string GetUserDataFilePath(bool isRoamingPath)
        {
            var specialFolder = isRoamingPath ? Environment.SpecialFolder.ApplicationData : Environment.SpecialFolder.MyDocuments;
            return Path.Combine(Environment.GetFolderPath(specialFolder), GetBuildConfigPath());
        }

        public static string GetErrorLogFilePath(bool isRoamingPath)
        {
            var specialFolder = isRoamingPath ? Environment.SpecialFolder.ApplicationData : Environment.SpecialFolder.MyDocuments;
            return Path.Combine(Environment.GetFolderPath(specialFolder), GetBuildConfigPath(), @"LogFiles\error.log");
        }

        public static string GetDebugLogFilePath(bool isRoamingPath)
        {
            var specialFolder = isRoamingPath ? Environment.SpecialFolder.ApplicationData : Environment.SpecialFolder.MyDocuments;
            return Path.Combine(Environment.GetFolderPath(specialFolder), GetBuildConfigPath(), @"LogFiles\debug.log");
        }

        public static string GetInfoLogFilePath(bool isRoamingPath)
        {
            var specialFolder = isRoamingPath ? Environment.SpecialFolder.ApplicationData : Environment.SpecialFolder.MyDocuments;
            return Path.Combine(Environment.GetFolderPath(specialFolder), GetBuildConfigPath(), @"LogFiles\info.log");
        }

        private static string GetBuildConfigPath()
        {
#if DEBUG
            return "MSFS Pop Out Panel Manager Debug";
#elif LOCAL
            return "MSFS Pop Out Panel Manager Local";
#else
            return "MSFS Pop Out Panel Manager";
#endif
        }
    }
}
