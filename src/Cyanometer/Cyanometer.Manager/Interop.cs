using System.Runtime.InteropServices;

namespace Cyanometer.Manager
{
    public static class Interop
    {
        /// <summary>
        /// ID of RTC clock.
        /// </summary>
        public const int CLOCK_REALTIME = 0;
        /// <summary>
        /// Gets RTC time from WittyPi.
        /// </summary>
        /// <param name="clk_id"></param>
        /// <param name="tp"></param>
        /// <returns></returns>
        [DllImport("librt")]
        public static extern int clock_gettime(int clk_id, [Out]Timespec tp);
        [DllImport("librt")]
        public static extern int clock_settime(int clk_id, [In]Timespec tp);
    }

    /// <summary>
    /// Represents Linux timespec struct
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public class Timespec
    {
        /// <summary>
        /// Seconds from 1970
        /// </summary>
        public int tv_sec;
        /// <summary>
        /// Nanoseconds.
        /// </summary>
        public int tv_nsec;
    }
}
