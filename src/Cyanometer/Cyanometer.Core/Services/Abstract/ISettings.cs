namespace Cyanometer.Core.Services.Abstract
{
    public interface ISettings
    {
        string Country { get; }
        string City { get; }
        string Location { get; }
        int LocationId { get; }
        bool ExceptionlessEnabled { get; }
        string ExceptionlessServer { get; }
        string ExceptionlessApiKey { get; }
        bool ProcessImages { get; }
        bool ProcessAirQuality { get; }
        int SleepMinutes { get; }
        int CycleWaitMinutes { get; }
        int InitialDelay { get; }
        string StopCheckUrl { get; }
        string CyanoNotificationsUrl { get; }
        bool SyncWithNntp { get; }
        /// <summary>
        /// When true a heartbeat signal is sent to <see cref="HeartbeatAddress"/>.
        /// </summary>
        bool SendHeartbeat { get; }
        /// <summary>
        /// Server address for the heartbeat signal.
        /// </summary>
        string HeartbeatAddress { get; }
        /// <summary>
        /// This is a token used to communicate with Cyanometer web
        /// </summary>
        string CyanoToken { get; }
        /// <summary>
        /// Adds additional arguments to raspistill command line photo utility
        /// </summary>
        string RaspiStillAdditionalArguments { get; }
        /// <summary>
        /// Use Libcamera for taking photos
        /// </summary>
        bool UseLibcamera { get; }
    }
}
