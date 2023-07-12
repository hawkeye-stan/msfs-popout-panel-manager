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
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        static FileLogger()
        {
            // Setup log4Net
            var logRepository = LogManager.GetRepository(Assembly.GetEntryAssembly());

            // Need to use AppContext.BaseDirectory for Single File Publishing to work
            // https://github.com/dotnet/designs/blob/main/accepted/2020/single-file/design.md#api-semantics
            XmlConfigurator.Configure(logRepository, new FileInfo(Path.Combine(AppContext.BaseDirectory, "log4net.config")));

            var errorLogAppender = LogManager.GetRepository(Assembly.GetEntryAssembly()).GetAppenders().First() as RollingFileAppender;
            errorLogAppender.File = FileIo.GetErrorLogFilePath();
            errorLogAppender.ActivateOptions();

            //var infoLogAppender = LogManager.GetRepository(Assembly.GetEntryAssembly()).GetAppenders().Skip(1).First() as RollingFileAppender;
            //infoLogAppender.File = FileIo.GetInfoLogFilePath();
            //infoLogAppender.ActivateOptions();

            //var debugLogAppender = LogManager.GetRepository(Assembly.GetEntryAssembly()).GetAppenders().Skip(2).First() as RollingFileAppender;
            //debugLogAppender.File = FileIo.GetDebugLogFilePath();
            //debugLogAppender.ActivateOptions();
        }

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
    }
}
