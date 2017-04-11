using System;
using System.Threading;
using System.Threading.Tasks;

namespace Cyanometer.Imagging.Services.Abstract
{
    public interface IImageProcessor
    {
        Task LoopAsync(CancellationToken ct);
    }
}
