using System;
using System.Threading;
using System.Threading.Tasks;

namespace Cyanometer.Imaging.Services.Abstract
{
    public interface IImageProcessor
    {
        Task LoopAsync(CancellationToken ct);
    }
}
