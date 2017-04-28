namespace Righthand.WittyPi
{
    /// <summary>
    /// A structure representing sleep date.
    /// </summary>
    public struct SleepDateTime
    {
        /// <summary>
        /// Minute component of the date.
        /// </summary>
        public byte Min { get; }
        /// <summary>
        /// Hour component of the date.
        /// </summary>
        public byte Hour { get; }
        /// <summary>
        /// Day component of the date.
        /// </summary>
        public byte Day { get; }
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="day"></param>
        /// <param name="hour"></param>
        /// <param name="min"></param>
        public SleepDateTime(byte day, byte hour, byte min)
        {
            Day = day;
            Hour = hour;
            Min = min;
        }
        /// <summary>
        /// Value compare.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            return obj is SleepDateTime && this == (SleepDateTime)obj;
        }

        /// <summary>
        /// Custom GetHashCode
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return Day.GetHashCode() ^ Hour.GetHashCode() ^ Min.GetHashCode();
        }

        /// <summary>
        /// ==
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public static bool operator ==(SleepDateTime x, SleepDateTime y)
        {
            return x.Day == y.Day && x.Hour == y.Hour && x.Min == y.Min;
        }
        /// <summary>
        /// !=
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public static bool operator !=(SleepDateTime x, SleepDateTime y)
        {
            return !(x == y);
        }
    }
}
