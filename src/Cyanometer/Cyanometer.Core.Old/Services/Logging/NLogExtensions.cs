using Cyanometer.Core.Services.Abstract;
using System;
using System.Diagnostics.Contracts;

namespace Cyanometer.Core.Services.Logging
{
    public static class LogExtensions
    {
        private const string SourceLogger = "Logger";
        public static LogItem LogInfo(this ILogger logger)
        {
            return logger.Log(LogLevel.Info);
        }
        public static LogItem LogDebug(this ILogger logger)
        {
            return logger.Log(LogLevel.Debug);
        }
        public static LogItem LogWarn(this ILogger logger)
        {
            return logger.Log(LogLevel.Warn);
        }
        public static LogItem LogError(this ILogger logger)
        {
            return logger.Log(LogLevel.Error);
        }
        public static LogItem Log(this ILogger logger, LogLevel level)
        {
            Contract.Requires(logger != null);
            Contract.Ensures(Contract.Result<LogItem>() != null);

            var item = new LogItem { Level = level, TimeStamp = DateTime.Now };
            item.Properties[SourceLogger] = logger;
            return item;
        }
        public static LogItem WithCategory(this LogItem item, string category)
        {
            Contract.Requires(item != null);
            Contract.Ensures(Contract.Result<LogItem>() != null);

            item.Category = category;
            return item;
        }
        public static LogItem WithMessage(this LogItem item, string message)
        {
            Contract.Requires(item != null);
            Contract.Ensures(Contract.Result<LogItem>() != null);

            item.Message = message;
            return item;
        }
        public static LogItem WithLevel(this LogItem item, LogLevel level)
        {
            Contract.Requires(item != null);
            Contract.Ensures(Contract.Result<LogItem>() != null);

            item.Level = level;
            return item;
        }
        public static LogItem WithException(this LogItem item, Exception ex)
        {
            Contract.Requires(item != null);
            Contract.Ensures(Contract.Result<LogItem>() != null);

            item.Exception = ex;
            return item;
        }
        public static void Commit(this LogItem item)
        {
            Contract.Requires(item != null);

            var logger = (ILogger)item.Properties[SourceLogger];
            logger.Log(item);
        }
    }
}
