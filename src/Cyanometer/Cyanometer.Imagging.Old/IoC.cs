using Autofac;
using Cyanometer.Imagging.Services.Abstract;
using Cyanometer.Imagging.Services.Implementation;

namespace Cyanometer.Imagging
{
    public static class IoC
    {
        public static void Register(ContainerBuilder builder)
        {
            builder.RegisterType<ImageProcessor>().As<IImageProcessor>().SingleInstance();
        }
    }
}
