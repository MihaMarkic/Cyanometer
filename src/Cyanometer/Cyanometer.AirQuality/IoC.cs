using Autofac;
using Cyanometer.AirQuality.Services.Abstract;
using Cyanometer.AirQuality.Services.Implementation;
using Righthand.WittyPi;

namespace Cyanometer.AirQuality
{
    public static class IoC
    {
        public static void Register(AirQualitySource airQualitySource)
        {
            ContainerBuilder builder = new ContainerBuilder();
            builder.RegisterType<ShiftRegister>().As<IShiftRegister>();
            builder.RegisterType<WittyPiService>().As<IWittyPiService>();
            switch (airQualitySource)
            {
                case AirQualitySource.Arso:
                    builder.RegisterType<ArsoService>().As<IAirQualityService>();
                    break;
            }
            builder.RegisterType<TwitterPush>().As<ITwitterPush>();
            IoCRegistrar.BuildContainer(builder);
        }
    }
}
