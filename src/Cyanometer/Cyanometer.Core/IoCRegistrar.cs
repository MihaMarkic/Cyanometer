using Autofac;
using Autofac.Builder;
using Autofac.Core;
using Cyanometer.Core.Services.Abstract;
using Cyanometer.Core.Services.Implementation;
using Cyanometer.Core.Services.Logging;
using System;
using System.Diagnostics;

namespace Cyanometer.Core
{
    public static class IoCRegistrar
    {
        /// <summary>
        /// This container holds all registrations. It can be used to resolve them or for any other supported usage.
        /// </summary>
        public static IContainer Container { get; private set; }
        [DebuggerStepThrough]
        public static T Resolve<T>()
        {
            return Container.Resolve<T>();
        }
        [DebuggerStepThrough]
        public static object Resolve(Type type)
        {
            return Container.Resolve(type);
        }

        [DebuggerStepThrough]
        public static T ResolveNamed<T>(string name)
        {
            return Container.ResolveNamed<T>(name);
        }

        [DebuggerStepThrough]
        public static T ResolveKeyed<T>(object serviceKey)
        {
            return Container.ResolveKeyed<T>(serviceKey);
        }

        public static void Register(ContainerBuilder builder)
        {
            builder.RegisterType<DaylightManager>().As<IDaylightManager>().SingleInstance();
            builder.RegisterType<FileService>().As<IFileService>().SingleInstance();
            builder.RegisterType<S3UploaderService>().As<IUploaderService>();
            builder.RegisterType<FileService>().As<IFileService>();
            builder.RegisterType<WebsiteNotificator>().As<IWebsiteNotificator>();
            builder.RegisterType<RaspberryService>().As<IRaspberryService>();
            builder.RegisterGeneratedFactory<LoggerFactory>(new TypedService(typeof(ILogger)));
        }

        /// <summary>
        /// Builds a container out of collected registrations (each project adds its registrations, usually through IoC.Register method).
        /// </summary>
        /// <param name="builder"></param>
        public static void BuildContainer(ContainerBuilder builder)
        {
            // register viewmodels
            Container = builder.Build();
        }
    }
}
