using System;

namespace Cyanometer.Core.Core
{
    public struct Daylight
    {
        public TimeSpan Sunrise { get; set; }
        public TimeSpan Sunset { get; set; }
        public Daylight(TimeSpan dawn, TimeSpan dusk)
        {
            Sunrise = dawn;
            Sunset = dusk;
        }
    }
}
