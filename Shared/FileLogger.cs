using log4net;
using log4net.Appender;
using log4net.Config;
using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace MSFSPopoutPanelManager.Shared
{
    public class FileLogger
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod()?.DeclaringType);
        
        static FileLogger()
        {
            // Setup log4Net
            var logRepository = LogManager.GetRepository(Assembly.GetEntryAssembly());

            // Need to use AppContext.BaseDirectory for Single File Publishing to work
            // https://github.com/dotnet/designs/blob/main/accepted/2020/single-file/design.md#api-semantics
            XmlConfigurator.Configure(logRepository, new FileInfo(Path.Combine(AppContext.BaseDirectory, "log4net.config")));

            if (LogManager.GetRepository(Assembly.GetEntryAssembly()).GetAppenders().First() is not RollingFileAppender errorLogAppender) 
                return;

            errorLogAppender.File = FileIo.GetErrorLogFilePath(UseApplicationDataPath);
            errorLogAppender.ActivateOptions();
        }

        public static bool UseApplicationDataPath { get; set; } = false;

        public static void WriteLog(string message, StatusMessageType messageType)
        {
            switch (messageType)
            {
                case StatusMessageType.Error:
                    Log.Error(message);
                    break;
                //case StatusMessageType.Info:
                //    Log.Info(message);
                //    break;
                //case StatusMessageType.Debug:
                //    Log.Debug(message);
                //    break;
            }
        }

        public static void WriteException(string message, Exception exception)
        {
            Log.Error(message, exception);
        }

        public static void CloseFileLogger()
        {
            LogManager.ShutdownRepository();
        }
    }
}
