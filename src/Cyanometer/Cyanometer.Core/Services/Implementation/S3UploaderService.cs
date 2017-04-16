using Amazon;
using Amazon.S3;
using Amazon.S3.Transfer;
using Cyanometer.Core.Services.Abstract;
using Cyanometer.Core.Services.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Cyanometer.Core.Services.Implementation
{
    public class S3UploaderService : IUploaderService
    {
        private readonly ILogger logger;
        private readonly ISettings settings;
        public S3UploaderService(LoggerFactory loggerFactory, ISettings settings)
        {
            logger = loggerFactory(nameof(S3UploaderService));
            this.settings = settings;
        }
        public async Task<string> UploadAsync(DateTime date, string filename, int? factor, CancellationToken ct)
        {
            logger.LogInfo().WithCategory(LogCategory.System).WithMessage("Uploading to S3").Commit();
            try
            {
                Stopwatch sw = Stopwatch.StartNew();
                IAmazonS3 client = new AmazonS3Client(settings.S3AccessKey, settings.S3PrivateKey, RegionEndpoint.EUCentral1);
                TransferUtility transfer = new TransferUtility(client);
                var tasks = new List<Task>(2);
                string folder = $"prod/{settings.Country}/{settings.City}/{settings.Location}/{date.Year}/{date.Month:00}/{date.Day:00}/{Path.GetFileName(filename)}";
                tasks.Add(transfer.UploadAsync(filename, "cyanometer-v2", folder, ct));
                
                await Task.WhenAll(tasks);
                //string photoId = flickr.UploadPicture(filename, "Cyanometer", factor?.ToString(), null, isPublic: true, isFamily: false, isFriend: false);
                logger.LogInfo().WithCategory(LogCategory.System).WithMessage($"Uploaded id {filename} in {sw.Elapsed}").Commit();
                return $"https://s3.eu-central-1.amazonaws.com/cyanometer-v2/{folder}";
            }
            catch (OperationCanceledException)
            {
                logger.LogWarn().WithCategory(LogCategory.System).WithMessage($"Upload cancelled").Commit();
                throw;
            }
            catch (Exception ex)
            {
                logger.LogError().WithCategory(LogCategory.System).WithMessage($"Upload of {filename} failed").WithException(ex).Commit();
                throw;
            }
        }
    }
}
