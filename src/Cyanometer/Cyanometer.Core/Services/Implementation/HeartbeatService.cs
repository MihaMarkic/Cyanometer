using Cyanometer.Core.Services.Abstract;
using Cyanometer.Core.Services.Logging;
using Flurl;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Cyanometer.Core.Services.Implementation
{
    public class HeartbeatService : IHeartbeatService
    {
        readonly ILogger logger;
        readonly HttpClient client;
        public HeartbeatService(LoggerFactory loggerFactory, HttpClient client)
        {
            logger = loggerFactory(nameof(HeartbeatService)); ;
            this.client = client;
        }
        public async Task SendHeartbeatAsync(Settings settings, CancellationToken ct)
        {
            logger.LogInfo().WithCategory(LogCategory.System).WithMessage("Sending heartbeat").Commit();
            try
            {
                var url = Url.Combine(settings.HeartbeatAddress, $"api/ping/{settings.LocationId}?Country={settings.Country}&City={settings.City}&Location={settings.Location}");
                await client.PostAsync(url, new StringContent(DateTime.Now.ToString()));
            }
            catch (Exception ex)
            {
                logger.LogWarn().WithCategory(LogCategory.System).WithMessage("Heartbeat sending failure").WithException(ex).Commit();
            }
        }
    }
}
