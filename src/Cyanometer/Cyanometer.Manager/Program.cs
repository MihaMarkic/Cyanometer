using Cyanometer.Core;
using Cyanometer.Manager.Properties;
using Cyanometer.Manager.Services.Abstract;
using Exceptionless;
using NLog;
using NLog.Common;
using System;

namespace Cyanometer.Manager
{
    class Program
    {
        static void Main(string[] args)
        {
            //InternalLogger.LogToConsole = true;
            //InternalLogger.LogLevel = LogLevel.Trace;

            var exceptConfig = ExceptionlessClient.Default.Configuration;
            exceptConfig.Enabled = Settings.Default.ExceptionlessEnabled;
            exceptConfig.ServerUrl = Settings.Default.ExceptionlessServer;
            exceptConfig.ApiKey = Settings.Default.ExceptionlessApiKey;
            exceptConfig.DefaultData["Country"] = Settings.Default.Country;
            exceptConfig.DefaultData["City"] = Settings.Default.City;
            exceptConfig.DefaultData["Location"] = Settings.Default.Location;
            exceptConfig.DefaultData["LocationId"] = Settings.Default.LocationId;
            exceptConfig.SetUserIdentity(Settings.Default.InstanceName, Settings.Default.InstanceName);

            var log = ExceptionlessClient.Default.Configuration.UseInMemoryLogger();
            ExceptionlessClient.Default.Startup();

            IoC.Register();
            var daylightManager = IoCRegistrar.Resolve<Core.Services.Abstract.IDaylightManager>();
            daylightManager.Load();
            var processor = IoCRegistrar.Resolve<IProcessor>();
            processor.Process();
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
    }
}
