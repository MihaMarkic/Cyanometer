namespace Cyanometer.Core
{
    public record Settings
    {
        public string Country { get; }
        public string City { get; }
        public string Location { get; }
        public int LocationId { get; }
        //bool ExceptionlessEnabled { get; }
        //string ExceptionlessServer { get; }
        //string ExceptionlessApiKey { get; }
        public bool ProcessImages { get; }
        //bool ProcessAirQuality { get; }
        //int SleepMinutes { get; }
        public int CycleWaitMinutes { get; }
        public int InitialDelay { get; }
        //string StopCheckUrl { get; }
        public string CyanoNotificationsUrl { get; }
        public bool SyncWithNntp { get; }
        /// <summary>
        /// When true a heartbeat signal is sent to <see cref="HeartbeatAddress"/>.
        /// </summary>
        public bool SendHeartbeat { get; }
        /// <summary>
        /// Server address for the heartbeat signal.
        /// </summary>
        public string HeartbeatAddress { get; }
        /// <summary>
        /// This is a token used to communicate with Cyanometer web
        /// </summary>
        public string CyanoToken { get; }
        /// <summary>
        /// Adds additional arguments to raspistill command line photo utility
        /// </summary>
        public string RaspiStillAdditionalArguments { get; }
        public Settings(string country, string city, string location, int locationId, bool processImages, int cycleWaitMinutes, int initialDelay, string cyanoNotificationsUrl, bool syncWithNntp, bool sendHeartbeat, string heartbeatAddress, string cyanoToken, string raspiStillAdditionalArguments)
        {
            Country = country;
            City = city;
            Location = location;
            LocationId = locationId;
            ProcessImages = processImages;
            CycleWaitMinutes = cycleWaitMinutes;
            InitialDelay = initialDelay;
            CyanoNotificationsUrl = cyanoNotificationsUrl;
            SyncWithNntp = syncWithNntp;
            SendHeartbeat = sendHeartbeat;
            HeartbeatAddress = heartbeatAddress;
            CyanoToken = cyanoToken;
            RaspiStillAdditionalArguments = raspiStillAdditionalArguments;
        }
    }
}
