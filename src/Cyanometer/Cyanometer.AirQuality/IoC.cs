﻿using Autofac;
using Cyanometer.AirQuality.Services.Abstract;
using Cyanometer.AirQuality.Services.Implementation;
using Cyanometer.AirQuality.Services.Implementation.Specific;

namespace Cyanometer.AirQuality
{
    public static class IoC
    {
        public static void Register(bool useFakeRaspberryService, AirQualitySource airQualitySource, ContainerBuilder builder)
        {
            if (useFakeRaspberryService)
            {
                builder.RegisterType<FakeShiftRegister>().As<IShiftRegister>();
            }
            else
            {
                builder.RegisterType<ShiftRegister>().As<IShiftRegister>();
            }
            switch (airQualitySource)
            {
                case AirQualitySource.Arso:
                    builder.RegisterType<ArsoService>().As<IAirQualityService>();
                    break;
                case AirQualitySource.WroclawPios:
                    builder.RegisterType<WroclawPiosService>().As<IAirQualityService>();
                    break;
            }
            builder.RegisterType<TwitterPush>().As<ITwitterPush>();
            builder.RegisterType<AirQualityProcessor>().As<IAirQualityProcessor>();
        }
    }
}