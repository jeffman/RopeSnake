using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLog;
using System.Threading;

namespace RopeSnake.Core
{
    public static class RLog
    {
        internal static ILogger _log;

        internal static int _warnCount = 0;
        internal static int _errorCount = 0;

        public static int WarnCount => _warnCount;
        public static int ErrorCount => _errorCount;

        public static object Context { get; set; }

        static RLog()
        {
            _log = LogManager.GetLogger("RopeSnake");
        }

        internal static void Log(LogLevel level, string message, Exception exception = null)
        {
            if (level == LogLevel.Warn)
                Interlocked.Increment(ref _warnCount);

            if (level == LogLevel.Error)
                Interlocked.Increment(ref _errorCount);

            if (!_log.IsEnabled(level))
                return;

            var logInfo = new LogEventInfo(level, "RopeSnake", message);

            if (Context != null)
                logInfo.Properties["context"] = Context.ToString();

            if (exception != null)
                logInfo.Exception = exception;

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

        public static void Warn(string message, Exception exception)
            => Log(LogLevel.Warn, message, exception);

        public static void Error(string message)
            => Log(LogLevel.Error, message);

        public static void Error(string message, Exception exception)
            => Log(LogLevel.Error, message, exception);

        public static void Fatal(string message)
            => Log(LogLevel.Fatal, message);

        public static void Fatal(string message, Exception ex)
            => Log(LogLevel.Fatal, message, ex);

        public static void ResetCounts()
        {
            _warnCount = 0;
            _errorCount = 0;
        }
    }
}
