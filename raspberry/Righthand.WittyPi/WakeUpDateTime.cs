namespace Righthand.WittyPi
{
    /// <summary>
    /// A structure representing wake up date. Null values are used when setting periodical date. Not all null fields combinations are valid, depending on the schedule type.
    /// For createing schedules, use on of the static methods <see cref="Daily"/>, <see cref="Hourly"/> or <see cref="Minutely(byte)"/>.
    /// </summary>
    public struct WakeUpDateTime
    {
        /// <summary>
        /// Second component of the date.
        /// </summary>
        public byte Sec { get; }
        /// <summary>
        /// Minute component of the date.
        /// </summary>
        public byte? Min { get; }
        /// <summary>
        /// Hour component of the date.
        /// </summary>
        public byte? Hour { get; }
        /// <summary>
        /// Day component of the date.
        /// </summary>
        public byte? Day { get; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="sec"></param>
        /// <param name="min"></param>
        /// <param name="hour"></param>
        /// <param name="day"></param>
        public WakeUpDateTime(byte? day, byte? hour, byte? min, byte sec)
        {
            Sec = sec;
            Min = min;
            Hour = hour;
            Day = day;
        }
        /// <summary>
        /// Creates a daily schedule.
        /// </summary>
        /// <param name="hour"></param>
        /// <param name="min"></param>
        /// <param name="sec"></param>
        /// <returns></returns>
        public static WakeUpDateTime Daily(byte hour, byte min, byte sec)
        {
            return new WakeUpDateTime
            (
                null,
                hour,
                min,
                sec
            );
        }
        /// <summary>
        /// Creates an hourly schedule.
        /// </summary>
        /// <param name="min"></param>
        /// <param name="sec"></param>
        /// <returns></returns>
        public static WakeUpDateTime Hourly(byte min, byte sec)
        {
            return new WakeUpDateTime
            (
                null,
                null,
                min,
                sec
            );
        }
        /// <summary>
        /// Creates a minutely schedule.
        /// </summary>
        /// <param name="sec"></param>
        /// <returns></returns>
        public static WakeUpDateTime Minutely(byte sec)
        {
            return new WakeUpDateTime
            (
                null,
                null,
                null,
                sec
            );
        }

        /// <summary>
        /// Value compare.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            return obj is WakeUpDateTime && this == (WakeUpDateTime)obj;
        }

        /// <summary>
        /// Custom GetHashCode
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return (Day?.GetHashCode() ?? 0) ^ (Hour?.GetHashCode() ?? 0) ^ (Min?.GetHashCode() ?? 0) ^ Sec.GetHashCode();
        }

        /// <summary>
        /// ==
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public static bool operator ==(WakeUpDateTime x, WakeUpDateTime y)
        {
            return x.Day == y.Day && x.Hour == y.Hour && x.Min == y.Min && x.Sec == y.Sec;
        }
        /// <summary>
        /// !=
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public static bool operator !=(WakeUpDateTime x, WakeUpDateTime y)
        {
            return !(x == y);
        }
    }
}
