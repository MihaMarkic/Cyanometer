using Cyanometer.Core.Services.Abstract;
using Cyanometer.Core.Services.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Cyanometer.Core.Core
{
    public static class Retrier
    {
        public static async Task RetryAsync(Action action, ILogger logger, int times, string failure, bool throwOnFailure, CancellationToken ct)
        {
            for (int i = 0; i < times; i++)
            {
                try
                {
                    if (i > 0)
                    {
                        await Task.Delay(100, ct);
                    }
                    ct.ThrowIfCancellationRequested();
                    action();
                    break;
                }
                catch (Exception ex) when (i < times - 1)
                {
                    logger.LogWarn().WithCategory(LogCategory.Common).WithMessage($"Failed {failure}:{ex.Message} on loop {i + 1}, will retry").Commit();
                }
                catch (Exception ex)
                {
                    logger.LogError().WithCategory(LogCategory.Common).WithMessage($"Failed {failure} on last try, giving up").WithException(ex).Commit();
                    if (throwOnFailure)
                    {
                        throw;
                    }
                }
            }
        }
        public static async Task<T> RetryAsync<T>(Func<T> func, ILogger logger, string failure, CancellationToken ct)
        {
            int loop = 0;
            while (true)
            {
                try
                {
                    ct.ThrowIfCancellationRequested();
                    return func();
                }
                catch (OperationCanceledException)
                {
                    logger.LogWarn().WithCategory(LogCategory.Common).WithMessage($"Exiting {failure}").Commit();
                }
                catch (Exception ex)
                {
                    logger.LogWarn().WithCategory(LogCategory.Common).WithMessage($"Failed {failure}:{ex.Message} on loop {loop + 1}, will retry").Commit();
                    await Task.Delay(100, ct);
                    loop++;
                }
            }
        }
    }
}
