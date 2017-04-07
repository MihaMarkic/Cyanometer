using Cyanometer.Core;
using Cyanometer.Core.Services.Abstract;
using Cyanometer.Manager.Properties;
using Cyanometer.Manager.Services.Abstract;
using Exceptionless;

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

            IoC.Register();
            var daylightManager = IoCRegistrar.Resolve<IDaylightManager>();
            daylightManager.Load();
            var processor = IoCRegistrar.Resolve<IProcessor>();
            processor.Process();
        }
    }
}
