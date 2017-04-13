using Cyanometer.Core.Core;
using Cyanometer.Core.Services.Abstract;
using Cyanometer.Core.Services.Logging;
using Cyanometer.Imagging.Services.Abstract;
using Cyanometer.SkyCalculator.Services.Abstract;
using Exceptionless;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Cyanometer.Imagging.Services.Implementation
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
        private readonly IWebsiteNotificator webSiteNotificator;
        private readonly ISkyCalculator calculator;

        public ImageProcessor(LoggerFactory loggerFactory, ISettings settings, IDaylightManager daylightManager, IFileService fileService,
            IRaspberryService raspberry, IUploaderService uploader, IWebsiteNotificator webSiteNotificator, ISkyCalculator calculator)
        {
            logger = loggerFactory(nameof(ImageProcessor));
            this.settings = settings;
            this.daylightManager = daylightManager;
            this.fileService = fileService;
            this.raspberry = raspberry;
            this.uploader = uploader;
            this.webSiteNotificator = webSiteNotificator;
            this.calculator = calculator;
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
                        string indexFileName = GetFullImageFileName(imageName, null, "index");
                        string coreName = Path.GetFileName(indexFileName);
                        logger.LogDebug().WithCategory(LogCategory.Manager).WithMessage($"Writing index {coreName}");
                        await fileService.WriteFileAsync(indexFileName, "", ct);

                        string smallFileName = GetFullImageFileName(imageName, "small", "jpg");
                        string largeFileName = GetFullImageFileName(imageName, "large", "jpg");
                        string factorFileName = GetFullImageFileName(imageName, null, "factor");

                        logger.LogDebug().WithCategory(LogCategory.ImageProcessor).WithMessage($"Taking small photo {Path.GetFileName(smallFileName)}").Commit();
                        await raspberry.TakePhotoAsync(smallFileName, new Size(800, 600), ct);
                        logger.LogDebug().WithCategory(LogCategory.ImageProcessor).WithMessage($"Taking big photo {Path.GetFileName(largeFileName)}").Commit();
                        Task bigPhoto = raspberry.TakePhotoAsync(largeFileName, size: new Size(1920, 1080), ct: ct);

                        logger.LogDebug().WithCategory(LogCategory.ImageProcessor).WithMessage("Starting computing factor").Commit();
                        var factor = await ComputeBluenessAsync(smallFileName, ct);
                        logger.LogInfo().WithCategory(LogCategory.ImageProcessor).WithMessage($"Factor is {factor.Index}").Commit();
                        await fileService.WriteFileAsync(factorFileName, $"{factor.Index}\n{now.ToString(CultureInfo.InvariantCulture)}", ct);
                        logger.LogDebug().WithCategory(LogCategory.ImageProcessor).WithMessage($"Factor written to {Path.GetFileName(factorFileName)}").Commit();
                        return; 

                        await bigPhoto;
                        await UploadGroupAsync(indexFileName, imageName, ct);
                        ExceptionlessClient.Default.SubmitLog(nameof(ImageProcessor), $"Image processor calculated blueness to {factor.Index}", Exceptionless.Logging.LogLevel.Info);
                    }
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
                string[] indexes = fileService.GetFiles(imagePath, "*.index");
                if (indexes.Length > 0)
                {
                    logger.LogInfo().WithCategory(LogCategory.ImageProcessor).WithMessage($"Found {indexes.Length} old index files for upload").Commit();
                    foreach (string index in indexes)
                    {
                        await UploadGroupAsync(index, Path.GetFileNameWithoutExtension(index), ct);
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

        public async Task UploadGroupAsync(string indexFileName, string imageName, CancellationToken ct)
        {
            logger.LogDebug().WithCategory(LogCategory.ImageProcessor).WithMessage($"Uploading group {imageName}").Commit();
            try
            {
                string smallFileName = GetFullImageFileName(imageName, "small", "jpg");
                string largeFileName = GetFullImageFileName(imageName, "large", "jpg");
                string factorFileName = GetFullImageFileName(imageName, null, "factor");

                bool smallFileExists = fileService.FileExists(smallFileName);
                bool largeFileExists = fileService.FileExists(largeFileName);
                bool factorFileExists = fileService.FileExists(factorFileName);

                logger.LogDebug().WithCategory(LogCategory.ImageProcessor).WithMessage($"Files exists small={smallFileExists} large={largeFileExists} factor={factorFileExists}").Commit();

                DateTime? photoDate = null;
                bool indexIsValid = false;
                int index = 0;
                if (factorFileExists)
                {
                    string[] text = fileService.GetAllLines(factorFileName);
                    string factorText = text[0];
                    string dateText = text[1];
                    logger.LogDebug().WithCategory(LogCategory.ImageProcessor).WithMessage($"Read factor index text {factorText}").Commit();
                    if (!int.TryParse(factorText, out index))
                    {
                        logger.LogWarn().WithCategory(LogCategory.ImageProcessor).WithMessage($"Couldn't parse index value {factorText}").Commit();
                    }
                    else
                    {
                        indexIsValid = true;
                    }
                    DateTime tempDate;
                    if (DateTime.TryParse(dateText, CultureInfo.InvariantCulture, DateTimeStyles.None, out tempDate))
                    {
                        photoDate = tempDate;
                        logger.LogInfo().WithCategory(LogCategory.ImageProcessor).WithMessage($"Parsed date text {photoDate}").Commit();
                    }
                }
                else
                {
                    logger.LogWarn().WithCategory(LogCategory.ImageProcessor).WithMessage($"Factor file {Path.GetFileName(factorFileName)} doesn't exist").Commit();
                }
                if (smallFileExists)
                {
                    if (!indexIsValid)
                    {
                        logger.LogWarn().WithCategory(LogCategory.ImageProcessor).WithMessage("Will try recompute the factor").Commit();
                        var factor = await ComputeBluenessAsync(smallFileName, ct);
                        index = factor.Index;
                        photoDate = DateTimeFromFileName(imageName);
                        logger.LogInfo().WithCategory(LogCategory.ImageProcessor).WithMessage($"Factor is {index} and date is {photoDate}").Commit();
                        await fileService.WriteFileAsync(factorFileName, index.ToString(), ct);
                        factorFileExists = true;
                    }
                    await UploadAndDeleteAsync(photoDate.Value, smallFileName, index, ct);
                }
                if (largeFileExists)
                {
                    logger.LogDebug().WithCategory(LogCategory.ImageProcessor).WithMessage($"Uploading large file {Path.GetFileName(largeFileName)}").Commit();
                    string imageUrl = await uploader.UploadAsync(photoDate.Value, largeFileName, index, ct);
                    logger.LogDebug().WithCategory(LogCategory.ImageProcessor).WithMessage($"Large file {Path.GetFileName(largeFileName)} uploaded").Commit();
                    var result = await webSiteNotificator.NotifyAsync(imageUrl, index, photoDate.Value, ct);
                    if (result.IsSuccess)
                    {
                        logger.LogInfo().WithCategory(LogCategory.ImageProcessor).WithMessage($"Notification was successful: {result.Message}").Commit();
                    }
                    else
                    {
                        throw (new Exception($"Notification failed: {result.Message}"));
                    }
                    logger.LogDebug().WithCategory(LogCategory.ImageProcessor).WithMessage($"Deleting file").Commit();
                    fileService.Delete(largeFileName);
                }
                if (factorFileExists)
                {
                    logger.LogDebug().WithCategory(LogCategory.ImageProcessor).WithMessage($"Deleting factor file {Path.GetFileName(factorFileName)}").Commit();
                    fileService.Delete(factorFileName);
                }
                logger.LogDebug().WithCategory(LogCategory.ImageProcessor).WithMessage($"Deleting index {indexFileName}").Commit();
                fileService.Delete(indexFileName);
                logger.LogDebug().WithCategory(LogCategory.ImageProcessor).WithMessage($"Group {imageName} done").Commit();
            }
            catch (OperationCanceledException)
            {
                logger.LogWarn().WithCategory(LogCategory.ImageProcessor).WithMessage("Cancelled").Commit();
            }
            catch (Exception ex)
            {
                logger.LogError().WithCategory(LogCategory.ImageProcessor).WithMessage($"Failed uploading group {imageName}").WithException(ex).Commit();
            }
        }

        public async Task UploadAndDeleteAsync(DateTime photoDate, string filename, int? index, CancellationToken ct)
        {
            logger.LogDebug().WithCategory(LogCategory.ImageProcessor).WithMessage($"Uploading {Path.GetFileName(filename)}").Commit();
            await uploader.UploadAsync(photoDate, filename, index, ct);
            logger.LogDebug().WithCategory(LogCategory.ImageProcessor).WithMessage($"Deleting file").Commit();
            fileService.Delete(filename);
        }

        public async Task<GetBluenessIndexResult> ComputeBluenessAsync(string fileName, CancellationToken ct)
        {
            logger.LogInfo().WithCategory(LogCategory.ImageProcessor).WithMessage("Computing factor").Commit();
            Stopwatch watch = Stopwatch.StartNew();
            List<Core.Core.Color> colors = await CollectColorsAsync(fileName, ct);
            logger.LogInfo().WithCategory(LogCategory.ImageProcessor).WithMessage($"Colors collected in {watch.ElapsedMilliseconds:#,##0}ms").Commit();
            return await Task.Run(() =>
            {
                Stopwatch w2 = Stopwatch.StartNew();
                var result = calculator.GetBluenessIndexTopPixels(colors, 30);
                logger.LogInfo().WithCategory(LogCategory.ImageProcessor).WithMessage($"Calculated in {w2.ElapsedMilliseconds:#,##0}ms").Commit();
                return result;
            }, ct);
        }

        public async Task<List<Core.Core.Color>> CollectColorsAsync(string filename, CancellationToken ct)
        {
            List<Core.Core.Color> colors = new List<Core.Core.Color>();

            try
            {
                using (Bitmap bitmap = await fileService.LoadBitmapAsync(filename, ct))
                {
                    for (int x = 0; x < bitmap.Width; x++)
                    {
                        for (int y = 0; y < bitmap.Height; y++)
                        {
                            var pixel = bitmap.GetPixel(x, y);
                            colors.Add(Core.Core.Color.FromRgb(pixel.R, pixel.G, pixel.B));
                        }
                    }
                    //foreach (var rect in imageAreas)
                    //{
                    //    for (int x = rect.X; x < rect.Right; x++)
                    //    {
                    //        for (int y = rect.Y; y < rect.Bottom; y++)
                    //        {
                    //            var pixel = wb.GetPixel(x, y);
                    //            colors.Add(CynEngine.Color.FromRgb(pixel.R, pixel.G, pixel.B));
                    //        }
                    //    }
                    //}
                }
            }
            catch (Exception ex)
            {
                logger.LogError().WithCategory(LogCategory.ImageProcessor).WithMessage($"Couldn't read from {filename}: {ex.Message}").WithException(ex).Commit();
            }
            return colors;
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
