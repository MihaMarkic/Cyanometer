using Amazon;
using Amazon.S3;
using Amazon.S3.Transfer;
using Cyanometer.Core.Services.Abstract;
using Cyanometer.Core.Services.Logging;
using System;
using System.Diagnostics;
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
        public async Task UploadAsync(string filename, int? factor, CancellationToken ct)
        {
            logger.LogInfo().WithCategory(LogCategory.System).WithMessage("Uploading to S3").Commit();
            try
            {
                Stopwatch sw = Stopwatch.StartNew();
                IAmazonS3 client = new AmazonS3Client(settings.S3AccessKey, settings.S3PrivateKey, RegionEndpoint.EUCentral1);
                TransferUtility transfer = new TransferUtility(client);
                await transfer.UploadAsync(filename, settings.S3Bucket, ct);
                //string photoId = flickr.UploadPicture(filename, "Cyanometer", factor?.ToString(), null, isPublic: true, isFamily: false, isFriend: false);
                logger.LogInfo().WithCategory(LogCategory.System).WithMessage($"Uploaded id {filename} in {sw.Elapsed}").Commit();
            }
            catch (OperationCanceledException)
            {
                logger.LogWarn().WithCategory(LogCategory.System).WithMessage($"Upload cancelled").Commit();
            }
            catch (Exception ex)
            {
                logger.LogError().WithCategory(LogCategory.System).WithMessage($"Upload of {filename} failed").WithException(ex).Commit();
                throw;
            }
        }
    }
}
