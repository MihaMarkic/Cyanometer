using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Cyanometer.AirQuality.Services.Abstract;
using Cyanometer.AirQuality.Services.Implementation.Arso;
using Cyanometer.Core.Services.Logging;
using RestSharp;
using Cyanometer.Core.Services.Abstract;
using RestSharp.Authenticators;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Cyanometer.AirQuality.Services.Implementation
{
    public class WroclawPiosService : AirQualityService, IAirQualityService
    {
        public WroclawPiosService(LoggerFactory loggerFactory, IAirQualitySettings settings) : 
            base(loggerFactory, settings, "http://air.wroclaw.pios.gov.pl/dane-pomiarowe/api/automatyczne/stacja/DOL012/12O3_43I-12SO2_43I-12NO2A-12PM10/dzienny/")
        {
            client.Authenticator = new HttpBasicAuthenticator(settings.AirQualityUsername, settings.AirQualityPassword);
        }

        public async Task<AirQualityData> GetIndexAsync(CancellationToken ct)
        {
            logger.LogInfo().WithCategory(LogCategory.AirQuality).WithMessage("Starting retrieving Wroclaw Pios data").Commit();
            try
            {
                var uri = DateTime.Now.Date.ToString("yyyy-MM-dd");
                var response = await GetDataAsync(uri, client, ct);
                AirQualityData data = ParseData(response);
                return data;
            }
            catch (Exception ex)
            {
                logger.LogError().WithCategory(LogCategory.AirQuality).WithMessage("Failed retrieving Wroclaw Pios data for some reason").WithException(ex).Commit();
                throw;
            }
        }

        public AirQualityData ParseData(PiosResponse response)
        {
            var o3 = GetLatestValue(GetSeries(response.Data, "o3"));
            var no2 = GetLatestValue(GetSeries(response.Data, "no2"));
            var pm10 = GetLatestValue(GetSeries(response.Data, "pm10"));
            var so2 = GetLatestValue(GetSeries(response.Data, "so2"));
            return new AirQualityData
            {
                Date = GetMaxDate(o3?.Date, no2?.Date, pm10?.Date, so2?.Date),
                O3 = o3?.Value,
                NO2 = no2?.Value,
                PM10 = pm10?.Value,
                SO2 = so2?.Value
            };
        }

        public DateTime GetMaxDate(params DateTime?[] dates)
        {
            var max = dates.Where(d => d.HasValue).OrderByDescending(d => d).FirstOrDefault();
            return max.Value;
        }

        public Series GetSeries(ResponseData data, string key)
        {
            return data.Series.SingleOrDefault(
                s =>  
                    string.Equals(s.AggType, "A1h", StringComparison.InvariantCultureIgnoreCase)
                     && string.Equals(s.ParamId, key, StringComparison.InvariantCultureIgnoreCase));
        }

        public SeriesItem? GetLatestValue(Series series)
        {
            if ((series?.Data?.Count ?? 0) == 0)
            {
                return null;
            }
            var query = from p in series.Data
                        let item = new SeriesItem {
                                Date = new DateTime(1970, 1, 1, 0, 0, 0, 0).AddSeconds(Convert.ToInt64(p[0])),
                                Value = Convert.ToDouble(p[1])
                        }
                        orderby item.Date descending
                        select item;
            var newest = query.First();
            return newest;
        }

        public async Task<PiosResponse> GetDataAsync(string url, IRestClient client, CancellationToken ct)
        {
            var request = new RestRequest(url, Method.GET);
            var response = await client.ExecuteGetTaskAsync<PiosResponse>(request, ct);
            if (response.StatusCode != HttpStatusCode.OK)
            {
                logger.LogError().WithCategory(LogCategory.AirQuality).WithMessage("Failed retrieving Wroclaw Pios raw data for some reason").Commit();
            }
            return response.Data;
        }

        [DebuggerDisplay("{Success}")]
        public class PiosResponse
        {
            public bool Success { get; set; }
            public ResponseData Data { get; set; }
        }

        [DebuggerDisplay("{Title,nq}")]
        public class ResponseData
        {
            public string Title { get; set; }
            public List<Series> Series { get; set; }
        }

        [DebuggerDisplay("{ParamId,nq}")]
        public class Series
        {
            public string Label { get; set;  }
            public string ParamId { get; set; }
            public string AggType { get; set; }
            // data is stored as array of date ticks and value (like SeriesItem)
            public List<List<decimal>> Data { get; set; }
        }

        public struct SeriesItem
        {
            public DateTime Date { get; set; }
            public double Value { get; set; }
        }
    }
}
