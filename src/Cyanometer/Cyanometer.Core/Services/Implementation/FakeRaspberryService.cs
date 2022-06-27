using Cyanometer.Core.Services.Abstract;
using Cyanometer.Core.Services.Logging;
using System.Drawing;
using System.Drawing.Imaging;
using System.Threading;
using System.Threading.Tasks;

namespace Cyanometer.Core.Services.Implementation
{
    public class FakeRaspberryService : IRaspberryService
    {
        private readonly ILogger logger;
        public FakeRaspberryService(LoggerFactory loggerFactory)
        {
            logger = loggerFactory(nameof(FakeRaspberryService));
        }
        public Task TakePhotoAsync(string filename, Size? size, CancellationToken ct)
        {
            using (Bitmap bm = new Bitmap(size?.Width ?? 640, size?.Height ?? 400))
            using (var g = Graphics.FromImage(bm))
            {
                g.FillRectangle(Brushes.AliceBlue, new Rectangle(0, 0, bm.Width, bm.Height));
                bm.Save(filename, ImageFormat.Jpeg);
            }
            logger.LogDebug().WithCategory(LogCategory.System).WithMessage($"Fake image for {filename} generated").Commit();
            return Task.FromResult(true);
        }
        public Task TakeLibcameraPhotoAsync(string filename, Size? size, CancellationToken ct)
        {
            using (Bitmap bm = new Bitmap(size?.Width ?? 640, size?.Height ?? 400))
            using (var g = Graphics.FromImage(bm))
            {
                g.FillRectangle(Brushes.AliceBlue, new Rectangle(0, 0, bm.Width, bm.Height));
                bm.Save(filename, ImageFormat.Jpeg);
            }
            logger.LogDebug().WithCategory(LogCategory.System).WithMessage($"Fake image for {filename} generated").Commit();
            return Task.FromResult(true);
        }
    }
}
