using System;
using System.Threading;
using System.Threading.Tasks;
using Cyanometer.Core;

namespace Cyanometer.Imagging.Services.Abstract
{
    public interface IImageProcessor
    {
        Task LoopAsync(Settings settings, CancellationToken ct);
    }
}
