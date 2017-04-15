using Cyanometer.AirQuality.Services.Abstract;
using Cyanometer.AirQuality.Services.Implementation.Arso;
using Cyanometer.Core.Services.Abstract;
using Cyanometer.Core.Services.Logging;
using RestSharp;
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Cyanometer.AirQuality.Services.Implementation
{
    public class ArsoService : AirQualityService, IAirQualityService
    {
        private const string Url = "ones_zrak_urni_podatki_zadnji.xml";
        public ArsoService(LoggerFactory loggerFactory, IAirQualitySettings settings): base(loggerFactory, settings, "http://www.arso.gov.si/xml/zrak/")
        {
        }
        public async Task<AirQualityData> GetIndexAsync(CancellationToken ct)
        {
            logger.LogInfo().WithCategory(LogCategory.AirQuality).WithMessage("Starting retrieving arso data").Commit();
            try
            {
                XDocument doc = await GetDataAsync(client, ct);
                return ParseData(doc, settings.AirQualityLocationKey);
            }
            catch (Exception ex)
            {
                logger.LogError().WithCategory(LogCategory.AirQuality).WithMessage("Failed retrieving arso data for some reason").WithException(ex).Commit();
                throw;
            }
        }

        public AirQualityData ParseData(XDocument doc, string stationCode)
        {
            XElement root = doc.Root;
            if (!string.Equals(root.Name.LocalName, "arsopodatki", StringComparison.Ordinal))
            {
                logger.LogError().WithCategory(LogCategory.AirQuality).WithMessage($"Root node name is {root.Name.LocalName} while expected arsopodatki").Commit();
                return null;
            }
            AirQualityData result = new AirQualityData
            {
                Date = DateTime.Parse(root.Element("datum_priprave").Value, CultureInfo.InvariantCulture)
            };

            var query = from e in root.Elements("postaja")
                        let code = e.Attribute("sifra")
                        where code != null && code.Value == stationCode
                        select e;
            var station = query.SingleOrDefault();
            if (station == null)
            {
                logger.LogError().WithCategory(LogCategory.AirQuality).WithMessage($"Couldn't find station with sifra={stationCode}").Commit();
                return null;
            }
            result.SO2 = GetDoubleValue(station.Element("so2"));
            result.PM10 = GetDoubleValue(station.Element("pm10"));
            result.O3 = GetDoubleValue(station.Element("o3"));
            result.NO2 = GetDoubleValue(station.Element("no2"));
            result.CO = GetDoubleValue(station.Element("co"));
            return result;
        }

        public async Task<XDocument> GetDataAsync(IRestClient client, CancellationToken ct)
        {
            var request = new RestRequest(Url, Method.GET);
            var response = await client.ExecuteTaskAsync(request, ct);
            var stringReader = new StringReader(response.Content);
            XDocument doc = XDocument.Load(stringReader);
            return doc;
        }
    }
}
