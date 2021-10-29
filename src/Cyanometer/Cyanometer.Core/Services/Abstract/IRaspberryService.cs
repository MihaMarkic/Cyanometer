using System.Drawing;
using System.Threading;
using System.Threading.Tasks;

namespace Cyanometer.Core.Services.Abstract
{
    public interface IRaspberryService
    {
        Task TakePhotoAsync(Settings settings, string filename, Size? size, CancellationToken ct);
    }
}