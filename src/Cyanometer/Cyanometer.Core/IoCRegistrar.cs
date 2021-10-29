using System;
using System.Diagnostics;
using System.Net.Http;
using Autofac;
using Autofac.Builder;
using Autofac.Core;
using Cyanometer.Core.Services.Abstract;
using Cyanometer.Core.Services.Implementation;
using Cyanometer.Core.Services.Logging;

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

        public static void Register(bool useFakeRaspberryService, bool hasWittyPi, ContainerBuilder builder)
        {
            builder.RegisterType<DaylightManager>().As<IDaylightManager>().SingleInstance();
            builder.RegisterType<FileService>().As<IFileService>().SingleInstance();
            builder.RegisterType<CyanoUploaderService>().As<IUploaderService>();
            builder.RegisterType<FileService>().As<IFileService>();
            if (useFakeRaspberryService)
            {
                builder.RegisterType<FakeRaspberryService>().As<IRaspberryService>();
            }
            else
            {
                builder.RegisterType<RaspberryService>().As<IRaspberryService>();
            }
            //if (hasWittyPi)
            //{
            //    builder.RegisterType<WittyPiService>().As<IWittyPiService>();
            //}
            //else
            //{
            //    builder.RegisterType<FakeWittyPiService>().As<IWittyPiService>();
            //}
            builder.RegisterGeneratedFactory<LoggerFactory>(new TypedService(typeof(ILogger)));
            builder.RegisterType<StopCheckService>().As<IStopCheckService>();
            builder.RegisterType<NtpService>().As<INtpService>();
            builder.RegisterType<HeartbeatService>().As<IHeartbeatService>().SingleInstance();
            builder.Register<HttpClient>(c => new HttpClient { Timeout = TimeSpan.FromMinutes(10) }).SingleInstance();
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
