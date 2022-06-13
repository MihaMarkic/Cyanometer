using Autofac;
using Cyanometer.AirQuality.Services.Abstract;
using Cyanometer.Core;
using Cyanometer.Core.Services.Abstract;
using Cyanometer.Manager.Properties;
using Cyanometer.Manager.Services.Abstract;
using Cyanometer.Manager.Services.Implementation;

namespace Cyanometer.Manager
{
    public static class IoC
    {
        public static void Register()
        {
            ContainerBuilder builder = new ContainerBuilder();
            Core.IoCRegistrar.Register(Settings.Default.UseFakeRaspberry, Settings.Default.HasWittyPi, builder);
            SkyCalculator.IoC.Register(builder);
            Imaging.IoC.Register(builder);
            AirQuality.IoC.Register(Settings.Default.UseFakeRaspberry, Settings.Default.AirQualitySource, builder);
            builder.RegisterType<NLogger>().As<ILogger>();
            builder.Register(c => Settings.Default)
                .As<ISettings>()
                .As<ITwitterSettings>()
                .As<IAirQualitySettings>();
            builder.RegisterType<Processor>().As<IProcessor>().SingleInstance();
            IoCRegistrar.BuildContainer(builder);
        }
    }
}
