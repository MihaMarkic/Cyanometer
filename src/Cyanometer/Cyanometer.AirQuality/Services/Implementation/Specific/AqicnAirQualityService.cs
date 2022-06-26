using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Cyanometer.AirQuality.Services.Abstract;
using Cyanometer.AirQuality.Services.Implementation.Arso;
using Cyanometer.Core.Services.Abstract;
using Cyanometer.Core.Services.Logging;
using Newtonsoft.Json;
using RestSharp;

namespace Cyanometer.AirQuality.Services.Implementation.Specific
{
    public class AqicnAirQualityService : AirQualityService, IAirQualityService
    {
        readonly string url;
        public AqicnAirQualityService(LoggerFactory loggerFactory, IAirQualitySettings settings, RestClient client)
            : base(loggerFactory, settings, client, "https://api.waqi.info/feed/")
        {
            url = $"{settings.AirQualityLocationKey}/?token={settings.AirQualityPassword}";
        }
        public async Task<AirQualityData> GetIndexAsync(CancellationToken ct)
        {
            logger.LogInfo().WithCategory(LogCategory.AirQuality).WithMessage("Starting retrieving Aqicn data").Commit();
            try
            {
                var data = await GetDataAsync(ct);
                return new AirQualityData
                {
                    Date = data.Data.Time.Iso.DateTime,
                    NO2 = data.Data.Iaqi.No2?.V,
                    O3 = data.Data.Iaqi.O3?.V,
                    PM10 = data.Data.Iaqi.Pm10?.V,
                    SO2 = data.Data.Iaqi.So2?.V,
                };
            }
            catch (Exception ex)
            {
                logger.LogError().WithCategory(LogCategory.AirQuality).WithMessage("Failed retrieving Aqicn data for some reason").WithException(ex).Commit();
                throw;
            }
        }
        public async Task<AqicnResult> GetDataAsync(CancellationToken ct)
        {
            var request = new RestRequest(url, Method.Get);
            var response = await client.ExecuteAsync(request, ct);
            var stringReader = new StringReader(response.Content);
            var result = DeserializeData(stringReader.ReadToEnd());
            return result;
        }
        public static AqicnResult DeserializeData(string content) => JsonConvert.DeserializeObject<AqicnResult>(content);

        public class AqicnResult
        {
            public string Status { get; set; }
            public AqicnData Data { get; set; }
        }
        public class AqicnData
        {
            public string Aqi { get; set; }
            public AqicnIaqi Iaqi { get; set; }
            public AqicnTime Time { get; set; }
        }
        public class AqicnIaqi
        {
            public AqicnValue No2 { get; set; }
            public AqicnValue O3 { get; set; }
            public AqicnValue Pm10 { get; set; }
            public AqicnValue So2 { get; set; }
        }
        public class AqicnValue
        {
            public double? V { get; set; }
        }
        public class AqicnTime
        {
            public DateTimeOffset Iso { get; set; }
        }
    }
}
