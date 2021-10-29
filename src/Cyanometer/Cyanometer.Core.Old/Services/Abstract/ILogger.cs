
namespace Cyanometer.Core.Services.Abstract
{
    /// <summary>
    /// Logging contract.
    /// </summary>
    /// <remarks>Logging is available either through DI or through static property Global.Logger.</remarks>
    public interface ILogger
    {
        void Log(LogItem item);
        void LogCore(LogItem item);
    }
}
