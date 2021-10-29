using System;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Cyanometer.Core;
using Cyanometer.Core.Services.Abstract;
using Cyanometer.Manager.Services.Abstract;

namespace Cyanometer.Manager
{
    class Program
    {
        static async Task Main(string[] args)
        {
            //InternalLogger.LogToConsole = true;
            //InternalLogger.LogLevel = LogLevel.Trace;

            //var exceptConfig = ExceptionlessClient.Default.Configuration;
            //exceptConfig.Enabled = Settings.Default.ExceptionlessEnabled;
            //exceptConfig.ServerUrl = Settings.Default.ExceptionlessServer;
            //exceptConfig.ApiKey = Settings.Default.ExceptionlessApiKey;
            //exceptConfig.DefaultData["Country"] = Settings.Default.Country;
            //exceptConfig.DefaultData["City"] = Settings.Default.City;
            //exceptConfig.DefaultData["Location"] = Settings.Default.Location;
            //exceptConfig.DefaultData["LocationId"] = Settings.Default.LocationId;
            //exceptConfig.SetUserIdentity(Settings.Default.InstanceName, Settings.Default.InstanceName);

            //var log = ExceptionlessClient.Default.Configuration.UseInMemoryLogger();
            //ExceptionlessClient.Default.Startup();

            Settings settings = await LoadSettingsAsync(CancellationToken.None);
            if (settings is null)
            {
                //logger.LogError().WithCategory(LogCategory.Manager).WithMessage("No settings were loaded, will exit.").Commit();
                Console.WriteLine("No settings were loaded, will exit.");
                return;
            }
            IoC.Register(settings);
            var daylightManager = IoCRegistrar.Resolve<Core.Services.Abstract.IDaylightManager>();
            daylightManager.Load();
            var processor = IoCRegistrar.Resolve<IProcessor>();
            var cts = new CancellationTokenSource();
            Console.CancelKeyPress += (_, ea) =>
            {
                // Tell .NET to not terminate the process
                ea.Cancel = true;

                Console.WriteLine("Received SIGINT (Ctrl+C)");
                cts.Cancel();
            };

            AppDomain.CurrentDomain.ProcessExit += (_, _) =>
            {
                if (!cts.IsCancellationRequested)
                {
                    Console.WriteLine("Received SIGTERM");
                    cts.Cancel();
                }
                else
                {
                    Console.WriteLine("Received SIGTERM, ignoring it because already processed SIGINT");
                }
            };
            await processor.ProcessAsync(settings, cts.Token);
            //Console.WriteLine("Exceptionless log is");
            //ExceptionlessClient.Default.ProcessQueue();
            //foreach (var l in log.GetLogEntries())
            //{
            //    Console.WriteLine($"{l.Level}:{l.Message}");
            //};
#if DEBUG
            Console.WriteLine("Press ENTER to exit");
            Console.ReadLine();
#endif
        }

        static async Task<Settings> LoadSettingsAsync(CancellationToken ct = default)
        {
            string fileName = Path.Combine(AppContext.BaseDirectory, "settings.json");
            if (!File.Exists(fileName))
            {
                return null;
            }
            try
            {
                using (FileStream openStream = File.OpenRead(fileName))
                {
                    var settings = await JsonSerializer.DeserializeAsync<Settings>(openStream);
                    return settings;
                }
            }
            catch (Exception ex)
            {
                //logger.LogError().WithCategory(LogCategory.Manager).WithException(ex).WithMessage($"Failed loading settings from {fileName}").Commit();
                return null;
            }
        }
    }
}
