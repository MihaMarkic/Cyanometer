using Cyanometer.Core.Services.Abstract;
using Cyanometer.Core.Services.Logging;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;

namespace Cyanometer.Core.Services.Implementation
{
    public class RaspberryService : IRaspberryService
    {
        private readonly ILogger logger;
        public RaspberryService(LoggerFactory loggerFactory)
        {
            logger = loggerFactory(nameof(RaspberryService));
        }
        public Task TakePhotoAsync(string filename, Size? size, CancellationToken ct)
        {
            TaskCompletionSource<object> tcs = new TaskCompletionSource<object>();
            try
            {
                logger.LogDebug().WithCategory(LogCategory.System).WithMessage($"Starting taking photo {filename} of size {size}").Commit();
                string resolution = size.HasValue ? $"--width {size.Value.Width} --height {size.Value.Height}" : string.Empty;
                Process proc = Process.Start("raspistill", $" -ISO 100 -awb horizon -o {filename} {resolution}");
                if (proc.HasExited)
                {
                    proc.Dispose();
                    tcs.SetResult(null);
                }
                else
                {
                    ct.Register(() => {
                        tcs.TrySetCanceled();
                    });
                    proc.EnableRaisingEvents = true;
                    proc.Exited += (sender, e) =>
                    {
                        proc.Dispose();
                        tcs.SetResult(null);
                    };
                }
            }
            catch (OperationCanceledException)
            {
                logger.LogInfo().WithCategory(LogCategory.System).WithMessage("Photo taking cancelled").Commit();
            }
            catch (Exception ex)
            {
                logger.LogError().WithCategory(LogCategory.System).WithMessage($"Failed taking a photo {filename} of size {size}").WithException(ex).Commit();
                tcs.SetException(ex);
            }
            return tcs.Task;
        }
    }
}
