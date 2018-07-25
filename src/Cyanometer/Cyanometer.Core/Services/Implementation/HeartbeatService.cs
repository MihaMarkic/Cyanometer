﻿using Cyanometer.Core.Services.Abstract;
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
        readonly ISettings settings;
        public HeartbeatService(LoggerFactory loggerFactory, ISettings settings)
        {
            logger = loggerFactory(nameof(HeartbeatService)); ;
            this.settings = settings;
        }
        public async Task SendHeartbeatAsync(CancellationToken ct)
        {
            logger.LogInfo().WithCategory(LogCategory.System).WithMessage("Verify can shutdown").Commit();
            try
            {
                var client = new HttpClient();
                var url = Url.Combine(settings.HeartbeatAddress, $"api/ping/{settings.LocationId}?Country={settings.Country}&City={settings.City}&Location={settings.Location}");
                await client.PostAsync(url, new StringContent(DateTime.Now.ToString()));
            }
            catch (Exception ex)
            {
                logger.LogWarn().WithCategory(LogCategory.System).WithMessage("Can shutdown, got exception on check").WithException(ex).Commit();
            }
        }
    }
}
