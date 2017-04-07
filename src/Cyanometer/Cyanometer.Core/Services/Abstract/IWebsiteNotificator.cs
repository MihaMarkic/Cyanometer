using Cyanometer.Core.Core;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Cyanometer.Core.Services.Abstract
{
    public interface IWebsiteNotificator
    {
        Task<UploadResponse> NotifyAsync(string url, int factor, DateTime date, CancellationToken ct);
    }
}