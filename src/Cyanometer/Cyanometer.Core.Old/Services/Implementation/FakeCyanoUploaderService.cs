using Cyanometer.Core.Services.Abstract;
using Cyanometer.Core.Services.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Cyanometer.Core.Services.Implementation
{
    public class FakeCyanoUploaderService : IUploaderService
    {
        private readonly ILogger logger;
        public FakeCyanoUploaderService(LoggerFactory loggerFactory)
        {
            logger = loggerFactory(nameof(FakeCyanoUploaderService));
        }
        public Task<string> UploadAsync(DateTime date, string filename, CancellationToken ct)
        {
            logger.LogDebug().WithCategory(LogCategory.System).WithMessage($"Fake Cynao upload for {filename} on {date} completed").Commit();
            return Task.FromResult("nekje");
        }
    }
}
