using Cyanometer.Core.Services.Abstract;
using Cyanometer.Core.Services.Logging;
using Cyanometer.Imaging.Services.Abstract;
//using Exceptionless;
using System;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Cyanometer.Imaging.Services.Implementation
{
    public class ImageProcessor : IImageProcessor
    {
        private string imagePath;
        private readonly ILogger logger;
        private readonly ISettings settings;
        private readonly IDaylightManager daylightManager;
        private readonly IFileService fileService;
        private readonly IRaspberryService raspberry;
        private readonly IUploaderService uploader;

        public ImageProcessor(LoggerFactory loggerFactory, ISettings settings, IDaylightManager daylightManager, IFileService fileService,
            IRaspberryService raspberry, IUploaderService uploader)
        {
            logger = loggerFactory(nameof(ImageProcessor));
            this.settings = settings;
            this.daylightManager = daylightManager;
            this.fileService = fileService;
            this.raspberry = raspberry;
            this.uploader = uploader;
        }

        public async Task LoopAsync(CancellationToken ct)
        {
            logger.LogInfo().WithCategory(LogCategory.Manager).WithMessage("Images processor started").Commit();
            try
            {
                imagePath = Path.Combine(Path.GetDirectoryName(typeof(ImageProcessor).Assembly.Location), "Images");
                fileService.CreateDirectory(imagePath);

                await UploadAllAsync(ct);

                if (daylightManager.IsDay())
                {
                    var now = DateTime.Now;
                    logger.LogInfo().WithCategory(LogCategory.ImageProcessor).WithMessage($"Date {now}").Commit();
                    string imageName = GetImageName(now);

                    try
                    {
                        // write index                    
                        string largeFileName = GetFullImageFileName(imageName, "large", "jpg");

                        if (settings.UseLibcamera)
                        {
                            await raspberry.TakeLibcameraPhotoAsync(largeFileName, size: new Size(1920, 1080), ct: ct);
                        }
                        else
                        {
                            await raspberry.TakePhotoAsync(largeFileName, size: new Size(1920, 1080), ct: ct);
                        }

                        await UploadGroupAsync(largeFileName, ct);
                    }
                    catch (OperationCanceledException) { }
                    catch (Exception ex)
                    {
                        logger.LogError().WithCategory(LogCategory.Manager).WithMessage("Error during loop").WithException(ex).Commit();
                    }
                }
                else
                {
                    logger.LogInfo().WithCategory(LogCategory.Manager).WithMessage("It is night, waiting for next").Commit();
                }
            }
            catch (OperationCanceledException)
            {
                logger.LogWarn().WithCategory(LogCategory.Manager).WithMessage($"Cancelled").Commit();
            }
        }

        public async Task UploadAllAsync(CancellationToken ct)
        {
            logger.LogDebug().WithCategory(LogCategory.ImageProcessor).WithMessage($"Starting upload all files").Commit();
            try
            {
                string[] photos = fileService.GetFiles(imagePath, "*.jpg");
                if (photos.Length > 0)
                {
                    logger.LogInfo().WithCategory(LogCategory.ImageProcessor).WithMessage($"Found {photos.Length} old index files for upload").Commit();
                    foreach (string photo in photos)
                    {
                        await UploadGroupAsync(photo, ct);
                    }
                }
                else
                {
                    logger.LogDebug().WithCategory(LogCategory.ImageProcessor).WithMessage($"No old files").Commit();
                }
            }
            catch (OperationCanceledException)
            {
                logger.LogInfo().WithCategory(LogCategory.ImageProcessor).WithMessage("Cancelled").Commit();
            }
            catch (Exception ex)
            {
                logger.LogError().WithCategory(LogCategory.ImageProcessor).WithMessage("Failed to upload all old").WithException(ex).Commit();
            }
        }

        public async Task UploadGroupAsync(string largeFilePath, CancellationToken ct)
        {
            string fileName = Path.GetFileName(largeFilePath);
            logger.LogDebug().WithCategory(LogCategory.ImageProcessor).WithMessage($"Uploading group {fileName}").Commit();
            try
            {
                bool largeFileExists = fileService.FileExists(largeFilePath);

                logger.LogDebug().WithCategory(LogCategory.ImageProcessor).WithMessage($"Files exists large={fileName}").Commit();

                DateTime photoDate = DateTimeFromFileName(fileName);
                
                if (largeFileExists)
                {
                    logger.LogDebug().WithCategory(LogCategory.ImageProcessor).WithMessage($"Uploading large file {fileName}").Commit();
                    string imageUrl = await uploader.UploadAsync(photoDate, largeFilePath, ct);
                    logger.LogDebug().WithCategory(LogCategory.ImageProcessor).WithMessage($"Large file {fileName} uploaded").Commit();
                    logger.LogDebug().WithCategory(LogCategory.ImageProcessor).WithMessage($"Deleting file").Commit();
                    fileService.Delete(largeFilePath);
                }
                logger.LogDebug().WithCategory(LogCategory.ImageProcessor).WithMessage($"Group {fileName} done").Commit();
            }
            catch (OperationCanceledException)
            {
                logger.LogWarn().WithCategory(LogCategory.ImageProcessor).WithMessage("Cancelled").Commit();
            }
            catch (Exception ex)
            {
                logger.LogError().WithCategory(LogCategory.ImageProcessor).WithMessage($"Failed uploading group {fileName}").WithException(ex).Commit();
            }
        }

        public static string GetImageName(DateTime now)
        {
            return $"sky-{now:dd.MM.yyyy-HH_mm_ss}";
        }

        public static DateTime DateTimeFromFileName(string filename)
        {
            string[] parts = filename.Split('-');
            string[] dateParts = parts[1].Split('.');
            string[] timeParts = parts[2].Split('_');

            DateTime date = new DateTime(
                int.Parse(dateParts[2]), int.Parse(dateParts[1]), int.Parse(dateParts[0]),
                int.Parse(timeParts[0]), int.Parse(timeParts[1]), int.Parse(timeParts[2]));
            return date;
        }

        private string GetFullImageFileName(string name, string size, string extension)
        {
            string sizeText = string.IsNullOrEmpty(size) ? "" : $"-{size}";
            string fileName = $"{name}{sizeText}.{extension}";
            return Path.Combine(imagePath, fileName);
        }
    }
}
