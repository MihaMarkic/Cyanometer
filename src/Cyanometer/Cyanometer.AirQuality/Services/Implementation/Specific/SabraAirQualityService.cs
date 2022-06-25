using Cyanometer.AirQuality.Services.Abstract;
using Cyanometer.AirQuality.Services.Implementation.Arso;
using Cyanometer.Core.Services.Abstract;
using Cyanometer.Core.Services.Logging;
using FluentFTP;
using RestSharp;
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Cyanometer.AirQuality.Services.Implementation.Specific
{
    public class SabraAirQualityService : AirQualityService, IAirQualityService
    {
        const string host = "ftp://fnwi.vps.infomaniak.com";
        const string remoteFile = "data/Necker_xml";
        readonly FtpClient ftpClient;
        public string DataSourceUri => "https://www.ge.ch/connaitre-qualite-air-geneve/dernier-bulletin-qualite-air-geneve";
        public SabraAirQualityService(LoggerFactory loggerFactory, IAirQualitySettings settings, RestClient client)
            : base(loggerFactory, settings, client, "https://www.ge.ch/connaitre-qualite-air-geneve/dernier-bulletin-qualite-air-geneve")
        {
            ftpClient = new FtpClient(host, settings.AirQualityUsername, settings.AirQualityPassword);
        }
        public async Task<AirQualityData> GetIndexAsync(CancellationToken ct)
        {
            logger.LogInfo().WithCategory(LogCategory.AirQuality).WithMessage("Starting retrieving arso data").Commit();
            try
            {
                XDocument doc = await GetDataAsync(ct);
                return ParseData(logger, doc, settings.AirQualityLocationKey);
            }
            catch (Exception ex)
            {
                logger.LogError().WithCategory(LogCategory.AirQuality).WithMessage("Failed retrieving arso data for some reason").WithException(ex).Commit();
                throw;
            }
        }

        public static AirQualityData ParseData(ILogger logger, XDocument doc, string stationCode)
        {
            var root = doc.Root.Element("messstation").Element("messstellengruppe").Element("messstelle");
            return new AirQualityData
            {
                Date = GetDate(logger, doc.Root),
                NO2 = GetValueFor(logger, root, "NO2"),
                O3 = GetValueFor(logger, root, "O3"),
                PM10 = GetValueFor(logger, root, "PM10"),
                SO2 = null,
            };
        }

        public static double? GetValueFor(ILogger logger, XElement root, string groupType)
        {
            try
            {
                var channel = root.Elements()
                    .Where(e => string.Equals((string)e.Attribute("kanalgruppetyp"), groupType, StringComparison.Ordinal))
                    .Single()
                    .Element("kanal");
                var lastRecord = channel.Elements().Where(e => e.Attribute("wert") != null).Last();
                return double.Parse((string)lastRecord.Attribute("wert"), CultureInfo.InvariantCulture);
            }
            catch (Exception ex)
            {
                logger.LogError().WithCategory(LogCategory.AirQuality)
                    .WithMessage($"Couldn't parse value {root.Value} for code {groupType}")
                    .WithException(ex).Commit();
                return null;
            }
        }

        public static DateTime GetDate(ILogger logger, XElement root)
        {
            return DateTime.Parse((string)root.Attribute("ende"), new CultureInfo("ch"));
        }

        public async Task<XDocument> GetDataAsync(CancellationToken ct)
        {
            await ftpClient.ConnectAsync(ct);
            try
            {
                var memoryStream = new MemoryStream();
                await ftpClient.DownloadAsync(memoryStream, remoteFile, token: ct);
                memoryStream.Seek(0, SeekOrigin.Begin);
                using (var reader = new StreamReader(memoryStream))
                {
                    var text = reader.ReadToEnd();
                    XDocument doc = XDocument.Parse(text);
                    return doc;
                }
            }
            catch (Exception ex)
            {
                logger.LogError().WithCategory(LogCategory.AirQuality)
                    .WithMessage("Failed retrieving data")
                    .WithException(ex).Commit();
                throw;
            }
            finally
            {
                await ftpClient.DisconnectAsync(ct);
            }
        }
    }
}
