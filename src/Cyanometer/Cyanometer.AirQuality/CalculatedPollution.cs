using Cyanometer.Core.Core;
using System.Diagnostics;

namespace Cyanometer.AirQuality
{
    [DebuggerDisplay("{Measurement}:{Pollution}")]
    public class CalculatedPollution
    {
        public AirPollution Pollution { get; set; }
        public Measurement Measurement { get; set; }
        public double Index { get; set; }
    }
}
