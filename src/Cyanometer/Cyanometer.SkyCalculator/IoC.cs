using Autofac;
using Cyanometer.SkyCalculator.Services.Abstract;
using Cyanometer.SkyCalculator.Services.Implementation;

namespace Cyanometer.SkyCalculator
{
    public static class IoC
    {
        public static void Register(ContainerBuilder builder)
        {
            builder.RegisterType<NearestColorCalculator>().As<IColorCalculator>().SingleInstance();
            builder.RegisterType<Calculator>().As<ISkyCalculator>().SingleInstance();
        }
    }
}
