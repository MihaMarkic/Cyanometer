using Cyanometer.Core.Services.Abstract;
using Cyanometer.Core.Services.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using Cyanometer.Core.Core;
using System.Linq;

namespace Cyanometer.Core.Services.Implementation
{
    public class DaylightManager : IDaylightManager
    {
        private readonly ILogger logger;

        private Dictionary<(int day, int month) , Daylight> times;

        public DaylightManager(LoggerFactory loggerFactory)
        {
            logger = loggerFactory(nameof(DaylightManager));
            //times = new Dictionary<string, Daylight> {
            //    {"1_6", new Daylight(new TimeSpan(05, 15, 0), new TimeSpan(20, 44, 0))},
            //};
        }

        public void Load()
        {
            string directory = Path.GetDirectoryName(typeof(DaylightManager).Assembly.Location);
            string fileName = Path.Combine(directory, "daylight.xml");
            try
            {
                if (File.Exists(fileName))
                {
                    var doc = XDocument.Load(fileName);
                    LoadFrom(doc);
                    logger.LogInfo().WithCategory(LogCategory.ImageProcessor).WithMessage($"Loaded {times.Count} daylight items").Commit();
                }
                else
                {
                    logger.LogWarn().WithCategory(LogCategory.ImageProcessor).WithMessage($"Failed to load configuration, couldn't find {fileName}").Commit();
                }
            }
            catch (Exception ex)
            {
                logger.LogWarn().WithCategory(LogCategory.ImageProcessor).WithMessage($"Failed to load configuration from {fileName}").WithException(ex).Commit();
            }
        }

        public void LoadFrom(XDocument doc)
        {
            var query = from d in doc.Root.Elements()
                        let key = (day: int.Parse((string)d.Attribute("Day")), month: int.Parse((string)d.Attribute("Month")))
                        let daylight = new Daylight(
                            TimeSpan.Parse((string)d.Attribute("Sunrise")),
                            TimeSpan.Parse((string)d.Attribute("Sunset")))
                        select new { Key = key, Daylight = daylight };
            times = query.ToDictionary(g => g.Key, g => g.Daylight);
        }

        public bool IsDay()
        {
            DateTime now = DateTime.Now;
            return IsDay(now);
        }

        public bool IsDay(DateTime now)
        {
            Daylight daylight;
            //logger.LogInfo().With($"Checking whether is day {now}");
            if (times.TryGetValue((now.Day, now.Month), out daylight))
            {
                //logger.Info($"Daylight data: from {daylight.Sunrise} to {daylight.Sunset}");
                TimeSpan current = new TimeSpan(now.Hour, now.Minute, now.Second);
                var isDay = current >= daylight.Sunrise && current <= daylight.Sunset;
                //logger.Info($"IsDay: {isDay}");
                return isDay;
            }
            else
            {
                //logger.Info($"Couldn't retrieve daylight data");
                return true;
            }
        }
    }
}
