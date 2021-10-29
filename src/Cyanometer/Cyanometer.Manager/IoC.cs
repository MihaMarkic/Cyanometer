using Autofac;
using Cyanometer.Core;
using Cyanometer.Core.Services.Abstract;
using Cyanometer.Manager.Services.Abstract;
using Cyanometer.Manager.Services.Implementation;

namespace Cyanometer.Manager
{
    public static class IoC
    {
        public static void Register(Settings settings)
        {
            ContainerBuilder builder = new ContainerBuilder();
            Core.IoCRegistrar.Register(true, false, builder);
            //SkyCalculator.IoC.Register(builder);
            Imagging.IoC.Register(builder);
            //AirQuality.IoC.Register(Settings.Default.UseFakeRaspberry, Settings.Default.AirQualitySource, builder);
            builder.RegisterType<NLogger>().As<ILogger>();
            builder.RegisterType<Processor>().As<IProcessor>().SingleInstance();
            IoCRegistrar.BuildContainer(builder);
        }
    }
}
