using System;
using System.Threading;
using System.Threading.Tasks;

namespace Cyanometer.AirQuality.Services.Abstract
{
    public interface IAirQualityProcessor
    {
        Task<bool> LoopAsync(CancellationToken ct);
    }
}
