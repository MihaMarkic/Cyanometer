using Cyanometer.AirQuality.Services.Abstract;
using Cyanometer.Core.Core;
using Cyanometer.Core.Services.Abstract;
using Cyanometer.Core.Services.Logging;
using Righthand.WittyPi;
using System;
using System.Diagnostics.Contracts;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Cyanometer.AirQuality.Services.Implementation
{
    public class AirQualityProcessor : IAirQualityProcessor
    {
        protected readonly ILogger logger;
        private readonly string lastDataDirectory;
        private readonly string lastDataPath;
        private readonly IFileService file;
        private readonly IAirQualityService arso;
        private readonly IShiftRegister shiftRegister;
        private readonly IWittyPiService wittyPiService;
        private readonly IWebsiteNotificator websiteNotificator;
        private readonly IStopCheckService stopCheckService;
        private readonly ITwitterPush twitterPush;
        private readonly INtpService ntpService;

        public AirQualityProcessor(LoggerFactory loggerFactory, IFileService file, IAirQualityService arso, IShiftRegister shiftRegister,
                IWittyPiService wittyPiService, IWebsiteNotificator websiteNotificator,
                IStopCheckService stopCheckService, ITwitterPush twitterPush, INtpService ntpService)
        {
            Contract.Requires(file != null);
            Contract.Requires(arso != null);
            Contract.Requires(shiftRegister != null);
            Contract.Requires(wittyPiService != null);
            Contract.Requires(websiteNotificator != null, "websiteNotificator is null.");
            Contract.Requires(stopCheckService != null, "stopCheckService is null.");
            Contract.Requires(twitterPush != null, "tweeterPush is null.");
            Contract.Requires(ntpService != null, "ntpService is null.");

            logger = loggerFactory(nameof(AirQualityProcessor));
            this.file = file;
            this.arso = arso;
            this.shiftRegister = shiftRegister;
            this.wittyPiService = wittyPiService;
            this.websiteNotificator = websiteNotificator;
            this.stopCheckService = stopCheckService;
            this.twitterPush = twitterPush;
            this.ntpService = ntpService;

            lastDataDirectory = Path.Combine(Path.GetDirectoryName(typeof(AirQualityProcessor).Assembly.Location), "LastData");
            file.CreateDirectory(lastDataDirectory);
            lastDataPath = Path.Combine(lastDataDirectory, "arso.xml");
        }

        public async Task<bool> LoopAsync(CancellationToken ct)
        {
            AirQualityPersisted state;
            logger.LogInfo().WithCategory(LogCategory.AirQuality).WithMessage($"Checking for state at {lastDataPath}").Commit();
            if (file.FileExists(lastDataPath))
            {
                logger.LogInfo().WithCategory(LogCategory.AirQuality).WithMessage($"Loading state").Commit();
                XElement root = XElement.Parse(file.GetAllText(lastDataPath));
                state = arso.XElementToPersisted(root);
            }
            else
            {
                logger.LogInfo().WithCategory(LogCategory.AirQuality).WithMessage($"Creating new state state").Commit();
                state = new AirQualityPersisted();
            }
            var data = await arso.GetIndexAsync(ct);
            arso.UpdatePersisted(data, state);
            XElement element = arso.PersistedToXElement(state);
            logger.LogInfo().WithCategory(LogCategory.AirQuality).WithMessage("Saving ARSO state").Commit();
            await file.WriteFileAsync(lastDataPath, element.ToString(), ct);
            var pollutions = CalculatePollutions(data.Date, state);
            var calculatedPollution = CalculateMaxPollution(pollutions);
            AirPollution pollution = calculatedPollution.Pollution;
            Measurement chief = GetChiefPolluter(pollutions);
            logger.LogInfo().WithCategory(LogCategory.AirQuality)
                .WithMessage($"Max pollution is {pollution} with index {calculatedPollution.Index:0} comming from {chief}").Commit();
            Lights light = PollutionToLights(pollution);
            if (pollution != AirPollution.Low)
            {
                light |= ChiefPolluterToLights(chief);
            }
            logger.LogInfo().WithCategory(LogCategory.AirQuality).WithMessage($"Calculated pollution is {pollution} using light {light}").Commit();
            DateTime? newestDate = state.NewestDate;
            var uploadResult = await websiteNotificator.NotifyAirQualityMeasurementAsync((int)calculatedPollution.Pollution + 1, chief, newestDate.Value, ct);
            if (uploadResult.IsSuccess)
            {
                logger.LogInfo().WithCategory(LogCategory.AirQuality).WithMessage($"Notification was successful: {uploadResult.Message}").Commit();
            }
            else
            {
                logger.LogWarn().WithCategory(LogCategory.AirQuality).WithMessage(($"Notification failed: {uploadResult.Message}")).Commit();
            }
#if !WINDOWS
            shiftRegister.EnableLight(light);
#endif
            await twitterPush.PushAsync(pollution, chief, state.NewestDate ?? DateTime.MinValue, ct);
            logger.LogInfo().WithCategory(LogCategory.AirQuality).WithMessage("ARSO processor done").Commit();
            return true;
        }

        public static Lights PollutionToLights(AirPollution pollution)
        {
            switch (pollution)
            {
                case AirPollution.Low:
                    return Lights.Eight;
                case AirPollution.Mid:
                    return Lights.Seven;
                case AirPollution.High:
                    return Lights.Six;
                default:
                    return Lights.Five;
            }
        }
        public static Lights ChiefPolluterToLights(Measurement chief)
        {
            switch (chief)
            {
                case Measurement.NO2:
                    return Lights.One;
                case Measurement.SO2:
                    return Lights.Two;
                case Measurement.PM10:
                    return Lights.Three;
                default:
                    return Lights.Four;
            }
        }

        public CalculatedPollution[] CalculatePollutions(DateTime date, AirQualityPersisted state)
        {
            logger.LogInfo().WithCategory(LogCategory.AirQuality).WithMessage($"Calcullating pollutions").Commit();
            CalculatedPollution[] pollution = new CalculatedPollution[]
            {
                CalculatePollution(date, Measurement.PM10, state.PM10, CalculatePM10PollutionIndex(state.PM10.Value)),
                CalculatePollution(date, Measurement.O3, state.O3, CalculateO3PollutionIndex(state.O3.Value)),
                CalculatePollution(date, Measurement.NO2, state.NO2, CalculateNO2PollutionIndex(state.NO2.Value)),
                CalculatePollution(date, Measurement.SO2, state.SO2, CalculateSO2PollutionIndex(state.SO2.Value))
            };
            return pollution;
        }

        public static CalculatedPollution CalculateMaxPollution(CalculatedPollution[] pollutions)
        {
            CalculatedPollution result = null;
            foreach (var pollution in pollutions)
            {
                if (result == null || pollution.Index > result.Index)
                {
                    result = pollution;
                }
            }
            return result;
        }

        public static Measurement GetChiefPolluter(CalculatedPollution[] pollutions)
        {
            Measurement measurement = Measurement.NO2;
            double maxIndex = double.MinValue;
            foreach (var pollution in pollutions)
            {
                if (pollution.Index > maxIndex)
                {
                    measurement = pollution.Measurement;
                    maxIndex = pollution.Index;
                }
            }
            return measurement;
        }

        public CalculatedPollution CalculatePollution(DateTime measurementDate, Measurement measurement, AirQualityValue value, double? index)
        {
            CalculatedPollution result = new CalculatedPollution { Measurement = measurement, Index = value.Value };
            bool isOutdated = false;
            if (value == null || (isOutdated = (measurementDate - value.LastDate).TotalHours > 4))
            {
                result.Pollution = AirPollution.Low;
                result.Index = 0;
            }
            else
            {
                if (index.Value < 50)
                {
                    result.Pollution = AirPollution.Low;
                }
                else if (index < 75)
                {
                    result.Pollution = AirPollution.Mid;
                }
                else if (index < 100)
                {
                    result.Pollution = AirPollution.High;
                }
                else
                {
                    result.Pollution = AirPollution.VeryHigh;
                }
                result.Index = index.Value;
            }
            string outdatedText = isOutdated ? "OUTDATED " : "";
            logger.LogInfo().WithCategory(LogCategory.AirQuality)
                .WithMessage($"{outdatedText}{measurement} is {result.Pollution} ({value?.Value} with index {result.Index:0.00}) on {value?.LastDate}")
                .Commit();
            return result;
        }

        public static double CalculatePM10PollutionIndex(double c)
        {
            if (c < 30)
            {
                return c * 50.0 / 30.0;
            }
            else if (c < 50)
            {
                return 50.0 + 25.0 / 20.0 * (c - 30);
            }
            else if (c < 100)
            {
                return 75 + 25.0 / 50.0 * (c - 50);
            }
            else
            {
                return c;
            }
        }

        public static double CalculateO3PollutionIndex(double c)
        {
            if (c < 60)
            {
                return c * 50.0 / 60.0;
            }
            else if (c < 120)
            {
                return 50 + 25.0 / 60.0 * (c - 60);
            }
            else if (c < 180)
            {
                return 75 + 25.0 / 60.0 * (c - 120);
            }
            else
            {
                return c * 100.0 / 180.0;
            }
        }

        public static double CalculateNO2PollutionIndex(double c)
        {
            if (c < 50)
            {
                return c;
            }
            else if (c < 100)
            {
                return 50 + 25.0 / 50.0 * (c - 50);
            }
            else if (c < 200)
            {
                return 50 + 25.0 / 100.0 * (c - 100);
            }
            else
            {
                return c * 100.0 / 200.0;
            }
        }

        public static double CalculateSO2PollutionIndex(double c)
        {
            if (c < 50)
            {
                return c;
            }
            else if (c < 100)
            {
                return 50 + 25.0 / 50.0 * (c - 50);
            }
            else if (c < 350)
            {
                return 75 + 25.0 / 250.0 * (c - 100);
            }
            else
            {
                return c * 100.0 / 350.0;
            }
        }
    }
}
