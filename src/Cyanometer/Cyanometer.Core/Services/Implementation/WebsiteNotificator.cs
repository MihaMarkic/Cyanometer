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
        public WebsiteNotificator(LoggerFactory loggerFactory)
        {
            logger = loggerFactory(nameof(WebsiteNotificator));
        }

        public class Content
        {
            [JsonProperty(PropertyName = "s3_url")]
            public string s3_url { get; set; }
            [JsonProperty(PropertyName = "taken_at")]
            public DateTime taken_at { get; set; }
            [JsonProperty(PropertyName = "blueness_index")]
            public string blueness_index { get; set; }
        }
        public Task<UploadResponse> NotifyAsync(string url, int factor, DateTime date, CancellationToken ct)
        {
            var requestBody = new Content
            {
                s3_url = url,
                taken_at = date,
                blueness_index = factor.ToString()
            };

            //HttpClient client = new HttpClient
            //{
            //    BaseAddress = new Uri("http://cyanometer.herokuapp.com/")
            //};
            //client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            string content = JsonConvert.SerializeObject(requestBody);
            //var stringContent = new StringContent(content, encoding: Encoding.UTF8, mediaType: "application/json");
            //var response = await client.PostAsync("api/images", stringContent);
            //if (!response.IsSuccessStatusCode)
            //{
            //    throw new Exception($"Sending request failed: {response.StatusCode}/{response.ReasonPhrase}");
            //}
            //var result = JsonConvert.DeserializeObject<Response>(await response.Content.ReadAsStringAsync());

            //var api = RestService.For<IWebServer>("http://cyanometer.herokuapp.com");
            //var result = await api.Notify(content);

            var client = new RestClient("http://cyanometer.herokuapp.com/");
            var request = new RestRequest("api/images", Method.POST);
            request.AddHeader("Content-Type", "application/json");
            request.AddJsonBody(requestBody);

            logger.LogInfo().WithCategory(LogCategory.System).WithMessage("Requesting to S3").Commit();
            var response = client.Execute(request);
            logger.LogInfo().WithCategory(LogCategory.System).WithMessage($"Response content is null {response?.Content == null}").Commit();
            var result = JsonConvert.DeserializeObject<Response>(response.Content);
            var value = new UploadResponse
            {
                IsSuccess = string.Equals(result.Status, "ok", StringComparison.Ordinal)
            };
            if (value.IsSuccess)
            {
                value.Message = result.Message;
            }
            else
            {
                value.Message = string.Join(",", result.Detail?.Select(d => d.Detail));
            }
            return Task.FromResult( value);
        }
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
