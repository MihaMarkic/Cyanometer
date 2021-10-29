
using System.Threading;
using System.Threading.Tasks;
using Cyanometer.Core;

namespace Cyanometer.Manager.Services.Abstract
{
    public interface IProcessor
    {
        Task ProcessAsync(Settings settings, CancellationToken ct);
    }
}
