using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLog;

namespace RopeSnake.Core
{
    public static class RLog
    {
        internal static ILogger _log;

        public static object Context { get; set; }

        static RLog()
        {
            _log = LogManager.GetLogger("RopeSnake");
        }

        internal static void Log(LogLevel level, string message)
        {
            if (!_log.IsEnabled(level))
                return;

            var logInfo = new LogEventInfo(level, "RopeSnake", message);

            if (Context != null)
                logInfo.Properties["context"] = Context.ToString();

            _log.Log(typeof(RLog), logInfo);
        }

        public static void Trace(string message)
            => Log(LogLevel.Trace, message);

        public static void Debug(string message)
            => Log(LogLevel.Debug, message);

        public static void Info(string message)
            => Log(LogLevel.Info, message);

        public static void Warn(string message)
            => Log(LogLevel.Warn, message);

        public static void Error(string message)
            => Log(LogLevel.Error, message);

        public static void Fatal(string message)
            => Log(LogLevel.Fatal, message);
    }
}
