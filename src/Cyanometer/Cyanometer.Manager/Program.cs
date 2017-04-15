using Cyanometer.Core;
using Cyanometer.Core.Services.Abstract;
using Cyanometer.Manager.Properties;
using Cyanometer.Manager.Services.Abstract;
using Exceptionless;
using System;

namespace Cyanometer.Manager
{
    class Program
    {
        static void Main(string[] args)
        {
            var exceptConfig = ExceptionlessClient.Default.Configuration;
            exceptConfig.Enabled = Settings.Default.ExceptionlessEnabled;
            exceptConfig.ServerUrl = Settings.Default.ExceptionlessServer;
            exceptConfig.ApiKey = Settings.Default.ExceptionlessApiKey;
            exceptConfig.DefaultData["Country"] = Settings.Default.Country;
            exceptConfig.DefaultData["City"] = Settings.Default.City;
            exceptConfig.DefaultData["Location"] = Settings.Default.Location;
            exceptConfig.DefaultData["LocationId"] = Settings.Default.LocationId;
            exceptConfig.SetUserIdentity(Settings.Default.LocationId.ToString(), $"{Settings.Default.City}");

            var log = ExceptionlessClient.Default.Configuration.UseInMemoryLogger();
            ExceptionlessClient.Default.Startup();

            IoC.Register();
            var daylightManager = IoCRegistrar.Resolve<IDaylightManager>();
            daylightManager.Load();
            var processor = IoCRegistrar.Resolve<IProcessor>();
            processor.Process();
#if DEBUG
            Console.WriteLine("Log is");
            //ExceptionlessClient.Default.ProcessQueue();
            foreach (var l in log.GetLogEntries())
            {
                Console.WriteLine($"{l.Level}:{l.Message}");
            };
            Console.WriteLine("Press ENTER to exit");
            Console.ReadLine();
#endif
        }
    }
}
