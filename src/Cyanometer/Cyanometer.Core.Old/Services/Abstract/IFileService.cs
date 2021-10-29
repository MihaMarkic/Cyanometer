using System.Drawing;
using System.Threading;
using System.Threading.Tasks;

namespace Cyanometer.Core.Services.Abstract
{
    public interface IFileService
    {
        Task<Bitmap> LoadBitmapAsync(string filename, CancellationToken ct);
        Task WriteFileAsync(string filename, string content, CancellationToken ct);
        string[] GetFiles(string path, string searchPattern);
        bool FileExists(string filename);
        string GetAllText(string filename);
        void Delete(string filename);
        void CreateDirectory(string directory);
        string[] GetAllLines(string filename);
    }
}
