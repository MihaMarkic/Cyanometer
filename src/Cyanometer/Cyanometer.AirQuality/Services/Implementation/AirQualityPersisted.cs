using System;
using System.Collections.Generic;
using System.Linq;

namespace Cyanometer.AirQuality.Services.Implementation
{
    public class AirQualityPersisted
    {
        public AirQualityValue PM10 { get; set; }
        public AirQualityValue SO2 { get; set; }
        public AirQualityValue O3 { get; set; }
        public AirQualityValue NO2 { get; set; }
        public AirQualityValue CO { get; set; }

        public IEnumerable<AirQualityValue> AllValues()
        {
            yield return PM10;
            yield return SO2;
            yield return NO2;
            yield return PM10;
        }
        public DateTime? NewestDate
        {
            get
            {
                return AllValues().Where(v => v != null && v.LastDate != null).Select(v => (DateTime?)v.LastDate).Max();
            }
        }
        public DateTime? OldestDate
        {
            get
            {
                return AllValues().Where(v => v != null && v.LastDate != null).Select(v => v.LastDate).Min();
            }
        }
    }
}
