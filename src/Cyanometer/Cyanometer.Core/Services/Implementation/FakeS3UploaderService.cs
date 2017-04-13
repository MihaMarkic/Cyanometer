using Cyanometer.Core.Services.Abstract;
using Cyanometer.Core.Services.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Cyanometer.Core.Services.Implementation
{
    public class FakeS3UploaderService : IUploaderService
    {
        private readonly ILogger logger;
        public FakeS3UploaderService(LoggerFactory loggerFactory)
        {
            logger = loggerFactory(nameof(FakeS3UploaderService));
        }
        public Task<string> UploadAsync(DateTime date, string filename, int? factor, CancellationToken ct)
        {
            logger.LogDebug().WithCategory(LogCategory.System).WithMessage($"Fake S3 upload for {filename} on {date} completed").Commit();
            return Task.FromResult("nekje");
        }
    }
}
