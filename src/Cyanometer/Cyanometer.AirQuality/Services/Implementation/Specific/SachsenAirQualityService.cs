using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Cyanometer.AirQuality.Services.Abstract;
using Cyanometer.AirQuality.Services.Implementation.Arso;
using Cyanometer.Core.Services.Abstract;
using Cyanometer.Core.Services.Logging;
using RestSharp;

namespace Cyanometer.AirQuality.Services.Implementation.Specific
{
    public class SachsenAirQualityService : AirQualityService, IAirQualityService
    {
        public SachsenAirQualityService(LoggerFactory loggerFactory, IAirQualitySettings settings, RestClient client)
            : base(loggerFactory, settings, client, "https://geoportal.umwelt.sachsen.de/arcgis/services/luft/luftmessdaten/MapServer/WFSServer")
        {
        }

        public async Task<AirQualityData> GetIndexAsync(CancellationToken ct)
        {
            logger.LogInfo().WithCategory(LogCategory.AirQuality).WithMessage("Starting retrieving arso data").Commit();
            try
            {
                XDocument doc = await GetDataAsync(ct);
                return ParseData(doc, settings.AirQualityLocationKey);
            }
            catch (Exception ex)
            {
                logger.LogError().WithCategory(LogCategory.AirQuality).WithMessage("Failed retrieving arso data for some reason").WithException(ex).Commit();
                throw;
            }
        }
        public static DateTime GetDate(string date, string time)
        {
            string properDate = $"{date.Split('T')[0]}T{time}:00";
            return DateTime.Parse(properDate);
        }
        public AirQualityData ParseData(XDocument doc, string stationCode)
        {
            var wfs = XNamespace.Get("http://www.opengis.net/wfs/2.0");
            var luftLuftmessdaten = XNamespace.Get("https:geoportal.umwelt.sachsen.de/arcgis/services/luft/luftmessdaten/MapServer/WFSServer");
            var result = doc.Root
                .Elements().Where(e => e.Name == wfs + "member")
                .Elements().Where(e => e.Name == (luftLuftmessdaten + "Luftmessstationen")
                     && e.Elements().Where(e2 => e2.Name == (luftLuftmessdaten + "EU_CODE") && e2.Value == stationCode).Any())
                .Select(e => new AirQualityData
                {
                    Date = GetDate(e.Element(luftLuftmessdaten + "DATUM").Value, e.Element(luftLuftmessdaten + "MESSZEIT").Value),
                    O3 = GetDoubleValue(e.Element(luftLuftmessdaten + "OZON")),
                    PM10 = GetDoubleValue(e.Element(luftLuftmessdaten + "PM10")),
                    NO2 = GetDoubleValue(e.Element(luftLuftmessdaten + "STICKSTOFFDIOXID")),
                    SO2 = GetDoubleValue(e.Element(luftLuftmessdaten + "SCHWEFELDIOXID")),
                })
                .FirstOrDefault();
            return result;
        }

        public async Task<XDocument> GetDataAsync(CancellationToken ct)
        {
            const string parameters = "?SERVICE=WFS&REQUEST=GetFeature&VERSION=2.0.0&TYPENAMES=luft_luftmessdaten:Luftmessstationen&TYPENAME=luft_luftmessdaten:Luftmessstationen&STARTINDEX=0&COUNT=1000&SRSNAME=urn:ogc:def:crs:EPSG::25833&BBOX=5431070.18768121488392353,215209.8988020783290267,5848104.34905883856117725,564673.96820965665392578,urn:ogc:def:crs:EPSG::25833";
            var request = new RestRequest(parameters, Method.Get);
            var response = await client.ExecuteAsync(request, ct);
            var stringReader = new StringReader(response.Content);
            XDocument doc = XDocument.Load(stringReader);
            return doc;
        }
    }
}
