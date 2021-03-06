﻿using Cyanometer.Core.Services.Abstract;
using Cyanometer.Core.Services.Logging;
using Flurl;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Cyanometer.Core.Services.Implementation
{
    public class StopCheckService : IStopCheckService
    {
        readonly ILogger logger;
        readonly ISettings settings;
        readonly HttpClient client;
        public StopCheckService(LoggerFactory loggerFactory, ISettings settings, HttpClient client)
        {
            logger = loggerFactory(nameof(StopCheckService));
            this.settings = settings;
            this.client = client;
        }

        public async Task<bool> CanShutdownAsync(CancellationToken ct)
        {
            logger.LogInfo().WithCategory(LogCategory.System).WithMessage("Verify can shutdown").Commit();
            try
            {
                string url = Url.Combine(settings.StopCheckUrl, $"stop-{settings.LocationId}.txt");
                var content = await client.GetStringAsync(url);
                bool canShutdown = !string.Equals("mihies", content, StringComparison.Ordinal);
                if (!canShutdown)
                {
                    logger.LogWarn().WithCategory(LogCategory.System).WithMessage($"Can't shutdown, got {content}").Commit();
                }
                return canShutdown;
            }
            catch (HttpRequestException ex)
            {
                if (!ex.Message.Contains("404 (Not Found)"))
                {
                    logger.LogWarn().WithCategory(LogCategory.System).WithMessage("Failed to check for stop").WithException(ex).Commit();
                }
                return true;
            }
            catch (Exception ex)
            {
                logger.LogWarn().WithCategory(LogCategory.System).WithMessage("Can shutdown, got exception on check").WithException(ex).Commit();
                return true;
            }
        }
    }
}
