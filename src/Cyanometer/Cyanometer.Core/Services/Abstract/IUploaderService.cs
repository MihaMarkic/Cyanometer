using System.Threading;
using System.Threading.Tasks;

namespace Cyanometer.Core.Services.Abstract
{
    public interface IUploaderService
    {
        Task UploadAsync(string filename, int? factor, CancellationToken ct);
    }
}