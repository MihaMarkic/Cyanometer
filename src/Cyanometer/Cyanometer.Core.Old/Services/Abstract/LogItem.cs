using System;
using System.Collections.Generic;

namespace Cyanometer.Core.Services.Abstract
{
    public class LogItem
    {
        public LogLevel Level;
        public string Category;
        public object Message;
        public Exception Exception;
        public DateTime? TimeStamp;
        public Dictionary<string, object> properties;

        public Dictionary<string, object> Properties
        {
            get
            {
                if (properties == null)
                {
                    properties = new Dictionary<string, object>();
                }
                return properties;
            }
        }
    }
}
