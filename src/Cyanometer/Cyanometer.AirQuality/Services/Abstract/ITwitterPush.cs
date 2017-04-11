using Cyanometer.Core.Core;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Cyanometer.AirQuality.Services.Abstract
{
    public interface ITwitterPush
    {
        Task PushAsync(AirPollution pollution, Measurement chief, DateTime date, CancellationToken ct);
    }
}