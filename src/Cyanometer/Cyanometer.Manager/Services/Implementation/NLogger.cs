using Cyanometer.Core.Services.Abstract;
using Cyanometer.Core.Services.Logging;
using Exceptionless;
using Exceptionless.Models;
using System;
using System.Diagnostics;
using System.Diagnostics.Contracts;

namespace Cyanometer.Manager.Services.Implementation
{
    public class NLogger : Logger
    {
        const string CategoryContext = "Category";
        private readonly ISettings settings;
        private readonly NLog.Logger logger;
        public NLogger(string className, ISettings settings)
        {
            Contract.Requires(settings != null, nameof(settings) + " is null.");
            this.settings = settings;
            logger = NLog.LogManager.GetLogger("Default logger");
        }

        private NLog.LogLevel ToNLogLevel(LogLevel level)
        {
            switch (level)
            {
                case LogLevel.Debug:
                    return NLog.LogLevel.Debug;
                case LogLevel.Error:
                    return NLog.LogLevel.Error;
                case LogLevel.Fatal:
                    return NLog.LogLevel.Fatal;
                case LogLevel.Info:
                    return NLog.LogLevel.Info;
                case LogLevel.Off:
                    return NLog.LogLevel.Off;
                case LogLevel.Trace:
                    return NLog.LogLevel.Trace;
                case LogLevel.Warn:
                    return NLog.LogLevel.Warn;
                default:
                    throw new Exception("Invalid log level: " + level);
            }
        }

        public override void LogCore(LogItem item)
        {
            NLog.LogLevel nlogLogLevel = ToNLogLevel(item.Level);
            //if (IsEnabled(nlogLogLevel))
            //{
            NLog.LogEventInfo logItem = new NLog.LogEventInfo();
            logItem.Level = nlogLogLevel;
            logItem.TimeStamp = DateTime.UtcNow;
            logItem.Message = item.Message?.ToString();
            logItem.Properties[CategoryContext] = item.Category;
            logItem.Exception = item.Exception;
            if (item.Properties != null)
            {
                foreach (var pair in item.Properties)
                {
                    logItem.Properties.Add(pair.Key, pair.Value);
                }
            }
            logger.Log(logItem);
            if (logItem.Exception != null)
            {
                var except = logItem.Exception.ToExceptionless();
                except.Submit();
            }
            else if (item.Level == LogLevel.Error || item.Level == LogLevel.Warn || item.Level == LogLevel.Info || item.Level == LogLevel.Fatal)
            {
                var ev = new Event
                {
                    Message = logItem.Message
                };
                ev.Data.Add(Event.KnownDataKeys.Level, item.Level);
                ev.Data.Add("@category", item.Category);
                ExceptionlessClient.Default.SubmitEvent(ev);
            }
            OutputToConsole(item);
        }

        [Conditional("DEBUG")]
        public void OutputToConsole(LogItem item)
        {
            Debug.WriteLine($"{item.Level}:{item.Message}");
        }
    }
}
