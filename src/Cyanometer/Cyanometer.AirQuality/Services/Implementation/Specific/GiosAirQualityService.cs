using Cyanometer.AirQuality.Services.Abstract;
using Cyanometer.AirQuality.Services.Implementation.Arso;
using Cyanometer.Core.Services.Abstract;
using Cyanometer.Core.Services.Logging;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Cyanometer.AirQuality.Services.Implementation.Specific
{
    public class GiosAirQualityService : AirQualityService, IAirQualityService
    {
        /// <summary>
        /// 
        /// </summary>
        /// <remarks>Got from https://api.gios.gov.pl/pjp-api/rest/station/findAll - Wrocław - Korzeniowskiego</remarks>
        public const int WroclawStationId = 117;
        public GiosAirQualityService(LoggerFactory loggerFactory, IAirQualitySettings settings, IRestClient client) :
            base(loggerFactory, settings, client, "https://api.gios.gov.pl/pjp-api/rest/")
        {
        }
        public async Task<AirQualityData> GetIndexAsync(CancellationToken ct)
        {
            logger.LogInfo().WithCategory(LogCategory.AirQuality).WithMessage("Starting retrieving Wroclaw GIOS data").Commit();
            try
            {
                var sensors = await GetSensorIdsAsync(ct);
                AirQualityData data = new AirQualityData();
                var tasks = ImmutableDictionary<ParamId, Task<Measurements>>.Empty;
                foreach (var pair in sensors)
                {
                    Task<Measurements> dataTask = pair.Value.HasValue ? GetDataAsync(pair.Value.Value, ct) : Task.FromResult<Measurements>(null);
                    tasks = tasks.Add(pair.Key, dataTask);
                }
                await Task.WhenAll(tasks.Values);
                data.SO2 = tasks[ParamId.SO2].Result?.Values?.FirstOrDefault()?.Value;
                data.PM10 = tasks[ParamId.PM10].Result?.Values?.FirstOrDefault()?.Value;
                data.O3 = tasks[ParamId.O3].Result?.Values?.FirstOrDefault()?.Value;
                data.NO2 = tasks[ParamId.NO2].Result?.Values?.FirstOrDefault()?.Value;
                data.CO = tasks[ParamId.CO].Result?.Values?.FirstOrDefault()?.Value;
                // for date reference take the one from PM10 sensor reading
                data.Date = tasks[ParamId.PM10].Result.Values.First().Date;
                return data;
            }
            catch (Exception ex)
            {
                logger.LogError().WithCategory(LogCategory.AirQuality).WithMessage("Failed retrieving Wroclaw GIOS data for some reason").WithException(ex).Commit();
                throw;
            }
        }

        internal async Task<ImmutableDictionary<ParamId, int?>> GetSensorIdsAsync(CancellationToken ct)
        {
            logger.LogInfo().WithCategory(LogCategory.AirQuality).WithMessage($"Getting SensorIds").Commit();
            var sensors = await GetSensorsAsync(ct);
            var result = ImmutableDictionary<ParamId, int?>.Empty;
            ImmutableDictionary<int, SensorResult> map = sensors.Where(s => s.Param != null).ToImmutableDictionary(s => s.Param.IdParam, s => s);
            foreach (var typeId in Enum.GetValues(typeof(ParamId)).Cast<ParamId>())
            {
                if (map.TryGetValue((int)typeId, out SensorResult sr))
                {
                    result = result.Add(typeId, sr.Id);
                }
            }
            logger.LogInfo().WithCategory(LogCategory.AirQuality).WithMessage($"Getting SensorIds ... done").Commit();
            return result;
        }

        public async Task<Measurements> GetDataAsync(int sensorId, CancellationToken ct)
        {
            var request = new RestRequest($"data/getData/{sensorId}", Method.GET);
            logger.LogInfo().WithCategory(LogCategory.AirQuality).WithMessage($"Getting sensorId for {request.Resource}").Commit();
            var response = await client.ExecuteGetTaskAsync(request, ct);
            if (response.StatusCode != HttpStatusCode.OK)
            {
                logger.LogError().WithCategory(LogCategory.AirQuality).WithMessage($"Failed retrieving Wroclaw GIOS raw data: {response.ErrorMessage}").Commit();
            }
            var result = JsonConvert.DeserializeObject<Measurements>(response.Content);
            logger.LogInfo().WithCategory(LogCategory.AirQuality).WithMessage($"Getting sensorId for {request.Resource} ... done").Commit();
            return result;
        }

        public async Task<SensorResult[]> GetSensorsAsync(CancellationToken ct)
        {
            var request = new RestRequest($"station/sensors/{WroclawStationId}", Method.GET);
            logger.LogInfo().WithCategory(LogCategory.AirQuality).WithMessage($"Getting sensors for {request.Resource}").Commit();
            var response = await client.ExecuteGetTaskAsync(request, ct);
            if (response.StatusCode != HttpStatusCode.OK)
            {
                logger.LogError().WithCategory(LogCategory.AirQuality).WithMessage("Failed retrieving Wroclaw GIOS sensors meta data: {response.ErrorMessage}").Commit();
                return null;
            }
            var result = JsonConvert.DeserializeObject<SensorResult[]>(response.Content);
            logger.LogInfo().WithCategory(LogCategory.AirQuality).WithMessage($"Getting sensors for {request.Resource} ... done").Commit();
            return result;
        }

        public enum ParamId
        {
            SO2 = 1,
            PM10 = 3,
            O3 = 5,
            NO2 = 6,
            CO = 8
        }

        [DebuggerDisplay("{Id}:{Param.ParamCode}")]
        public class SensorResult
        {
            public int Id { get; set; }
            public int StationId { get; set; }
            public SensorParamResult Param { get; set; }
        }

        [DebuggerDisplay("{ParamCode,nq}")]
        public class SensorParamResult
        {
            public string ParamName { get; set; }
            public string ParamFormula { get; set; }
            public string ParamCode { get; set; }
            public int IdParam { get; set; }
        }

        [DebuggerDisplay("{Key,nq}")]
        public class Measurements
        {
            public string Key { get; set; }
            public Measurement[] Values { get; set; }
        }

        [DebuggerDisplay("{Date}")]
        public class Measurement
        {
            public DateTime Date { get; set; }
            public double Value { get; set; }
        }
    }
}
