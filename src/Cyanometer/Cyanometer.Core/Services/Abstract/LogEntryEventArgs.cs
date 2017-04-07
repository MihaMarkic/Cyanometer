using System;

namespace Cyanometer.Core.Services.Abstract
{
    public class LogEntryEventArgs : EventArgs
    {
        public string Category;
        public LogLevel Level;
        public string Message;
        public Exception Exception;
    }
}
