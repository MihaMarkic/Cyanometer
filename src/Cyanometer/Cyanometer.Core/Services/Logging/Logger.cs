using Cyanometer.Core.Services.Abstract;

namespace Cyanometer.Core.Services.Logging
{
    /// <summary>
    /// Abstract logging implementation that serves as a base for derived implementations to provide
    /// boilerplate code.
    /// </summary>
    public abstract class Logger : ILogger
    {
        public void Log(LogItem item)
        {
            LogCore(item);
        }

        public abstract void LogCore(LogItem item);
    }
    public delegate ILogger LoggerFactory(string className);
}
