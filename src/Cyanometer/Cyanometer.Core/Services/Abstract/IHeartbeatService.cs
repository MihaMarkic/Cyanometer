using System;
using System.Threading;
using System.Threading.Tasks;

namespace Cyanometer.Core.Services.Abstract
{
    public interface IHeartbeatService
    {
        Task SendHeartbeatAsync(CancellationToken ct);
    }
}
