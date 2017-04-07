using Cyanometer.Core.Services.Abstract;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Cyanometer.Core.Services.Implementation
{
    public class FileService : IFileService
    {
        public Task<Bitmap> LoadBitmapAsync(string filename, CancellationToken ct)
        {
            return Task.FromResult((Bitmap)Image.FromFile(filename));
        }
        public Task WriteFileAsync(string filename, string content, CancellationToken ct)
        {
            File.WriteAllText(filename, content);
            return Task.FromResult(0);
        }
        public string[] GetFiles(string path, string searchPattern)
        {
            return Directory.GetFiles(path, searchPattern);
        }
        public bool FileExists(string filename)
        {
            return File.Exists(filename);
        }
        public string GetAllText(string filename)
        {
            return File.ReadAllText(filename);
        }
        public string[] GetAllLines(string filename)
        {
            return File.ReadAllLines(filename);
        }
        public void Delete(string filename)
        {
            File.Delete(filename);
        }
        public void CreateDirectory(string directory)
        {
            Directory.CreateDirectory(directory);
        }
    }
}
