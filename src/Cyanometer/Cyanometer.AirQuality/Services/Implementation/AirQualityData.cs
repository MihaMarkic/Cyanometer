using System;

namespace Cyanometer.AirQuality.Services.Implementation.Arso
{
    public class AirQualityData
    {
        public DateTime Date { get; set; }
        public double? PM10 { get; set; }
        public double? O3 { get; set; }
        public double? NO2 { get; set; }
        public double? SO2 { get; set; }
        public double? CO { get; set; }
    }
}
