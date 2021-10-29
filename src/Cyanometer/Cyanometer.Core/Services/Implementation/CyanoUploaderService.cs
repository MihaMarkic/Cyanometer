using Cyanometer.Core.Services.Abstract;
using Cyanometer.Core.Services.Logging;
using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace Cyanometer.Core.Services.Implementation
{
    public class CyanoUploaderService: IUploaderService
    {
        readonly ILogger logger;
        readonly HttpClient client;
#if DEBUG
        const string RootUrl = "http://localhost:5000";
#else
        const string RootUrl = "https://cyanometer.net";
#endif

        public CyanoUploaderService(LoggerFactory loggerFactory, HttpClient client)
        {
            logger = loggerFactory(nameof(CyanoUploaderService));
            this.client = client;
        }
        public async Task<string> UploadAsync(Settings settings, DateTime date, string filename, CancellationToken ct)
        {
            logger.LogInfo().WithCategory(LogCategory.System).WithMessage("Uploading to Cyano web").Commit();
            try
            {
                Stopwatch sw = Stopwatch.StartNew();

                CancellationTokenSource ctsUpload = new CancellationTokenSource();
                using (var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(ctsUpload.Token, ct))
                {
                    var uploadTask = UploadAsync(settings, filename, settings.CyanoToken, linkedCts.Token);

                    //var timeoutTask = Task.Delay(TimeSpan.FromMinutes(5));
                    //var finishedTask = await Task.WhenAny(uploadTask, timeoutTask);
                    //if (finishedTask == timeoutTask)
                    //{
                    //    ctsUpload.Cancel();
                    //    throw new Exception("Upload to Cyano timed out");
                    //}
                    await uploadTask;
                }

                logger.LogInfo().WithCategory(LogCategory.System).WithMessage($"Uploaded id {filename} in {sw.Elapsed}").Commit();
                return "";
            }
            catch (OperationCanceledException)
            {
                logger.LogWarn().WithCategory(LogCategory.System).WithMessage($"Upload cancelled").Commit();
                throw;
            }
            catch (Exception ex)
            {
                logger.LogError().WithCategory(LogCategory.System).WithMessage($"Upload of {filename} failed").WithException(ex).Commit();
                throw;
            }
        }

        async Task UploadAsync(Settings settings, string filename, string token, CancellationToken ct)
        {
            var requestContent = new MultipartFormDataContent();
            //    here you can specify boundary if you need---^
            var imageContent = new ByteArrayContent(File.ReadAllBytes(filename));
            imageContent.Headers.ContentType = MediaTypeHeaderValue.Parse("image/jpeg");

            requestContent.Add(imageContent, "image", Path.GetFileName(filename));
            requestContent.Headers.Add("CyanometerToken", token);

            string url = $"{RootUrl}/api/images/{settings.Country}/{settings.City}";
            logger.LogInfo().WithCategory(LogCategory.ImageProcessor).WithMessage($"Uploading to {url}").Commit();
            var result = await client.PostAsync(url, requestContent);
            logger.LogInfo().WithCategory(LogCategory.ImageProcessor).WithMessage($"Uploading result {result.StatusCode}:{result.IsSuccessStatusCode}").Commit();
            if (!result.IsSuccessStatusCode)
            {
                throw new Exception($"Failed upload on {result.StatusCode}:{result.RequestMessage}");
            }
        }
    }
}
