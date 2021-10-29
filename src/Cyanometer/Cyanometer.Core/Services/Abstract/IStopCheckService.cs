using System.Threading;
using System.Threading.Tasks;

namespace Cyanometer.Core.Services.Abstract
{
    public interface IStopCheckService
    {
        Task<bool> CanShutdownAsync(Settings settings, CancellationToken ct);
    }
}