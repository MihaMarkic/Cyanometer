namespace Cyanometer.Core.Services.Abstract
{
    public interface ISettings
    {
        string Country { get; }
        string City { get; }
        string Location { get; }
        int LocationId { get; }
        string JwtToken { get; }
        string S3AccessKey { get; }
        string S3PrivateKey { get; }
        string S3Bucket { get; }
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
    }
}
