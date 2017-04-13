using System;
using System.Threading;
using System.Threading.Tasks;

namespace Cyanometer.Core.Services.Abstract
{
    public interface IUploaderService
    {
        Task<string> UploadAsync(DateTime date, string filename, int? factor, CancellationToken ct);
    }
}