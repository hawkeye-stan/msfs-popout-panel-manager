using System;
using System.IO;

namespace MSFSPopoutPanelManager.Shared
{
    public class FileIo
    {
        public static string GetUserDataFilePath()
        {
#if DEBUG
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "MSFS Pop Out Panel Manager Debug");
#elif LOCAL
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "MSFS Pop Out Panel Manager Local");
#else
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "MSFS Pop Out Panel Manager");
#endif
        }

        public static string GetErrorLogFilePath()
        {
#if DEBUG
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), @"MSFS Pop Out Panel Manager Debug\LogFiles\error.log");
#elif LOCAL
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), @"MSFS Pop Out Panel Manager Local\LogFiles\error.log");
#else
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), @"MSFS Pop Out Panel Manager\LogFiles\error.log");
#endif
        }

        public static string GetDebugLogFilePath()
        {
#if DEBUG
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), @"MSFS Pop Out Panel Manager Debug\LogFiles\debug.log");
#elif LOCAL
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), @"MSFS Pop Out Panel Manager Local\LogFiles\debug.log");
#else
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), @"MSFS Pop Out Panel Manager\LogFiles\debug.log");
#endif
        }

        public static string GetInfoLogFilePath()
        {
#if DEBUG
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), @"MSFS Pop Out Panel Manager Debug\LogFiles\info.log");
#elif LOCAL
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), @"MSFS Pop Out Panel Manager Local\LogFiles\info.log");
#else
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), @"MSFS Pop Out Panel Manager\LogFiles\info.log");
#endif
        }
    }
}
