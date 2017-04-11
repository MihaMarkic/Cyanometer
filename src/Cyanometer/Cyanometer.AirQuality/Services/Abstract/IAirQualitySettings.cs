namespace Cyanometer.AirQuality.Services.Abstract
{
    public interface IAirQualitySettings
    {
        string AirQualityLocationKey { get; }
        string AirQualityUsername { get;  }
        string AirQualityPassword { get; }
    }
}
