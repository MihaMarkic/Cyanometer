using System;
using System.Threading;
using System.Threading.Tasks;

namespace Cyanometer.Core.Services.Abstract
{
    public interface IUploaderService
    {
        Task<string> UploadAsync(Settings settings, DateTime date, string filename, CancellationToken ct);
    }
}