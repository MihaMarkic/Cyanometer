using CoreTweet;
using Cyanometer.AirQuality.Services.Abstract;
using Cyanometer.Core.Core;
using Cyanometer.Core.Services.Abstract;
using Cyanometer.Core.Services.Logging;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Cyanometer.AirQuality.Services.Implementation
{
    public class TwitterPush : ITwitterPush
    {
        private readonly ILogger logger;
        private readonly ITwitterSettings settings;
        private string lastStatus;
        private readonly IFileService fileService;
        private readonly string lastDataDirectory;
        public TwitterPush(LoggerFactory loggerFactory, IFileService fileService, ITwitterSettings settings)
        {
            logger = loggerFactory(nameof(TwitterPush));
            this.fileService = fileService;
            this.settings = settings;
            lastDataDirectory = Path.Combine(Path.GetDirectoryName(typeof(AirQualityProcessor).Assembly.Location), "LastData");
        }

        public async Task PushAsync(AirPollution pollution, Measurement chief, DateTime date, CancellationToken ct)
        {
            if (!settings.IsTwitterEnabled)
            {
                return;
            }
            logger.LogInfo().WithCategory(LogCategory.AirQuality).WithMessage("Updating twitter").Commit();
            try
            {
                logger.LogInfo().WithCategory(LogCategory.AirQuality).WithMessage("Loading previous tweet").Commit();
                string tweetPath = Path.Combine(lastDataDirectory, "tweet.txt");
                logger.LogInfo().WithCategory(LogCategory.AirQuality).WithMessage("Loading credentials").Commit();
                if (fileService.FileExists(tweetPath))
                {
                    try
                    {
                        lastStatus = fileService.GetAllText(tweetPath);
                    }
                    catch (Exception ex)
                    {
                        logger.LogError().WithCategory(LogCategory.AirQuality).WithMessage("Failed loading last tweet's text").WithException(ex).Commit();
                    }
                }
                string text = $"Onesnaženost je {PollutionToSlovene(pollution)}.";
                if (pollution != AirPollution.Low)
                {
                    text += $" Glavni krivec je {chief}.";
                }
                text += $" Zadnja meritev ob {date:HH:mm}";
                if (!string.Equals(lastStatus, text))
                {
                    var token = Tokens.Create(settings.TwitterConsumerKey, settings.TwitterConsumerSecret, 
                        settings.TwitterAccessToken, settings.TwitterAccessTokenSecret);
                    var response = await token.Statuses.UpdateAsync(status: text, cancellationToken: ct);
                    lastStatus = text;
                    try
                    {
                        await fileService.WriteFileAsync(tweetPath, lastStatus, ct);
                    }
                    catch (Exception ex)
                    {
                        logger.LogError().WithCategory(LogCategory.AirQuality).WithMessage("Failed saving last tweet's text").WithException(ex).Commit();
                    }
                    logger.LogInfo().WithCategory(LogCategory.AirQuality).WithMessage($"Twitter updated status to '{text}' with response {response}").Commit();

                }
                else
                {
                    logger.LogInfo().WithCategory(LogCategory.AirQuality).WithMessage("Status didn't change, won't push.").Commit();
                }
            }
            catch (Exception ex)
            {
                logger.LogError().WithCategory(LogCategory.AirQuality).WithMessage("Failed updating twitter").WithException(ex).Commit();
            }
        }

        public static string PollutionToSlovene(AirPollution pollution)
        {
            switch (pollution)
            {
                case AirPollution.Mid:
                    return "srednja";
                case AirPollution.High:
                    return "visoka";
                case AirPollution.VeryHigh:
                    return "zelo visoka";
                default:
                    return "nizka";
            }
        }
    }
}
