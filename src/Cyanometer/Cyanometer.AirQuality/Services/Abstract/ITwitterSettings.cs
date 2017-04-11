namespace Cyanometer.AirQuality.Services.Abstract
{
    public interface ITwitterSettings
    {
        bool IsTwitterEnabled { get; }
        string TwitterConsumerKey { get; }
        string TwitterConsumerSecret { get; }
        string TwitterAccessToken { get; }
        string TwitterAccessTokenSecret { get; }
    }
}
