using Cyanometer.Core.Core;

namespace Cyanometer.AirQuality
{
    public class CalculatedPollution
    {
        public AirPollution Pollution { get; set; }
        public Measurement Measurement { get; set; }
        public double Index { get; set; }
    }
}
