using Autofac;
using Cyanometer.Imaging.Services.Abstract;
using Cyanometer.Imaging.Services.Implementation;

namespace Cyanometer.Imaging
{
    public static class IoC
    {
        public static void Register(ContainerBuilder builder)
        {
            builder.RegisterType<ImageProcessor>().As<IImageProcessor>().SingleInstance();
        }
    }
}
