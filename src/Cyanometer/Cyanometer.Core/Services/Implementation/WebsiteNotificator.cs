using Cyanometer.Core.Core;
using Cyanometer.Core.Services.Abstract;
using Cyanometer.Core.Services.Logging;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Cyanometer.Core.Services.Implementation
{
    public class WebsiteNotificator : IWebsiteNotificator
    {
        private readonly ILogger logger;
        private readonly ISettings settings;
        public WebsiteNotificator(LoggerFactory loggerFactory, ISettings settings)
        {
            logger = loggerFactory(nameof(WebsiteNotificator));
            this.settings = settings;
        }

        public static RestClient CreateClient(string url)
        {
            var client = new RestClient(url);
            return client;
        }

        public static RestRequest CreateRequest(string url, Method method, string token)
        {
            var request = new RestRequest(url, method);
            request.AddHeader("Content-Type", "application/json");
            request.AddHeader("Authorization", $"Bearer {token}");
            return request;
        }

        public class RequestImage
        {
            [JsonProperty(PropertyName = "s3_url")]
            public string s3_url { get; set; }
            [JsonProperty(PropertyName = "taken_at")]
            public DateTime taken_at { get; set; }
            [JsonProperty(PropertyName = "blueness_index")]
            public string blueness_index { get; set; }
            [JsonProperty(PropertyName = "location_id")]
            public int location_id { get; set; }
        }
        public Task<UploadResponse> NotifyAsync(string url, int factor, DateTime date, CancellationToken ct)
        {
            var image = new RequestImage
            {
                s3_url = url,
                taken_at = date,
                blueness_index = factor.ToString(),
                location_id = settings.LocationId
            };

            var client = CreateClient(settings.CyanoNotificationsUrl);
            var request = CreateRequest($"/api/locations/{settings.LocationId}/images", Method.POST, settings.JwtToken);
            request.AddJsonBody(new { image = image });

            logger.LogInfo().WithCategory(LogCategory.System).WithMessage("Requesting to S3").Commit();
            var response = client.Execute(request);
            logger.LogInfo().WithCategory(LogCategory.System).WithMessage($"Response status is {response.StatusCode}").Commit();
            var value = new UploadResponse();
            if (response.StatusCode == System.Net.HttpStatusCode.Created)
            {
                value.IsSuccess = true;
            }
            else
            {
                value = new UploadResponse { IsSuccess = false, Message = response.Content };
            }
            return Task.FromResult( value);
        }

        public static string MeasurementToString(Measurement measurement)
        {
            switch (measurement)
            {
                case Measurement.NO2:
                    return "car";
                case Measurement.SO2:
                    return "factory";
                case Measurement.PM10:
                    return "house";
                default:
                    return "sun";

            }
        }
        public Task<UploadResponse> NotifyAirQualityMeasurementAsync(int index, Measurement chief, DateTime date, CancellationToken ct)
        {
            var requestBody = new AirQualityContent
            {
                air_pollution_index = index.ToString(),
                taken_at = date,
                icon = MeasurementToString(chief),
                location_id = settings.LocationId
            };

            string content = JsonConvert.SerializeObject(requestBody);

            var client = CreateClient(settings.CyanoNotificationsUrl);
            var request = CreateRequest($"api/locations/{settings.LocationId}/environmental_data", Method.POST, settings.JwtToken);
            request.AddJsonBody(new { environmental_data = requestBody });

            var response = client.Execute(request);
            var value = new UploadResponse();
            if (response.StatusCode == System.Net.HttpStatusCode.Created)
            {
                value.IsSuccess = true;
            }
            else
            {
                value = new UploadResponse { IsSuccess = false, Message = response.Content };
            }
            return Task.FromResult(value);
        }
    }

    public class AirQualityContent
    {
        public string air_pollution_index { get; set; }
        public string icon { get; set; }
        public DateTime taken_at { get; set; }
        public int location_id { get; set; }
    }

    [DebuggerDisplay("{Status,nq}")]
    public class Response
    {
        public string Status { get; set; }
        public string Message { get; set; }
        public DetailItem[] Detail { get; set; }
    }

    [DebuggerDisplay("{Detail,nq}")]
    public class DetailItem
    {
        public string Title { get; set; }
        public string Detail { get; set; }
        public Source Source { get; set; }
    }

    [DebuggerDisplay("{Pointer,nq}")]
    public class Source
    {
        public string Pointer { get; set; }
    }

    //public interface IWebServer
    //{
    //    [Post("/api/images")]
    //    Task<Response> Notify([Body]WebsiteNotificator.Content content);
    //}
}
