using Autofac;
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
            Core.IoCRegistrar.Register(builder);
            SkyCalculator.IoC.Register(builder);
            Imagging.IoC.Register(builder);
            builder.RegisterType<NLogger>().As<ILogger>();
            builder.Register(c => Settings.Default).As<ISettings>();
            builder.RegisterType<Processor>().As<IProcessor>().SingleInstance();
            //builder.RegisterType<FileService>().As<IFileService>();
            //builder.RegisterType<ArsoService>().As<IArsoService>();
            //builder.RegisterType<ShiftRegister>().As<IShiftRegister>();
            //builder.RegisterType<WittyPiService>().As<IWittyPiService>();
            //builder.RegisterType<WebsiteNotificator>().As<IWebsiteNotificator>();
            //builder.RegisterType<StopCheckService>().As<IStopCheckService>();
            //builder.RegisterType<TwitterPush>().As<ITwitterPush>();
            //builder.RegisterType<NtpService>().As<INtpService>();
            IoCRegistrar.BuildContainer(builder);
        }
    }
}
