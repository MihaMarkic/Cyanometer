using Cyanometer.AirQuality.Services.Abstract;
using Cyanometer.AirQuality.Services.Implementation.Arso;
using Cyanometer.Core.Services.Abstract;
using Cyanometer.Core.Services.Logging;
using RestSharp;
using System;
using System.Globalization;
using System.Xml.Linq;

namespace Cyanometer.AirQuality.Services.Implementation
{
    public abstract class AirQualityService
    {
        protected readonly ILogger logger;
        protected readonly IAirQualitySettings settings;
        protected readonly RestClient client;
        public AirQualityService(LoggerFactory loggerFactory, IAirQualitySettings settings, RestClient client, string baseUrl)
        {
            logger = loggerFactory(nameof(AirQualityService));
            this.client = client;
            client.Options.BaseUrl = new Uri(baseUrl);
            this.settings = settings;
        }
        

        public double? GetDoubleValue(XElement element)
        {
            try
            {
                if (element?.Value == null || string.IsNullOrEmpty(element.Value))
                {
                    return null;
                }
                // special case for ARSO
                if (element.Value.StartsWith("<", StringComparison.InvariantCultureIgnoreCase))
                {
                    return 0;
                }
                return double.Parse(element.Value, CultureInfo.InvariantCulture);
            }
            catch (Exception ex)
            {
                logger.LogError().WithCategory(LogCategory.AirQuality).WithMessage($"Couldn't parse value {element.Value} for code {element.Name}").WithException(ex).Commit();
                return null;
            }
        }

        public XElement PersistedToXElement(AirQualityPersisted data)
        {
            return new XElement("ArsoPersisted", 
                ValueToXElement(nameof(data.PM10), data.PM10),
                ValueToXElement(nameof(data.SO2), data.SO2),
                ValueToXElement(nameof(data.O3), data.O3),
                ValueToXElement(nameof(data.NO2), data.NO2),
                ValueToXElement(nameof(data.CO), data.CO)
            );
        }

        public static XElement ValueToXElement(string name, AirQualityValue node)
        {
            if (node != null)
            {
                return new XElement(name,
                   new XAttribute(nameof(node.LastDate), node.LastDate.ToString(CultureInfo.InvariantCulture)),
                   node.Value.ToString(CultureInfo.InvariantCulture));
            }
            else
            {
                return null;
            }
        }

        public static AirQualityValue XElementToNode(XElement element)
        {
            if (element == null)
            {
                return null;
            }
            else
            {
                AirQualityValue value = new AirQualityValue
                {
                    LastDate = DateTime.Parse((string)element.Attribute(nameof(value.LastDate)), CultureInfo.InvariantCulture),
                    Value = double.Parse(element.Value, CultureInfo.InvariantCulture)
                };
                return value;
            }
        }

        public AirQualityPersisted XElementToPersisted(XElement root)
        {
            AirQualityPersisted result = new AirQualityPersisted
            {
                NO2 = XElementToNode(root.Element(nameof(result.NO2))),
                PM10 = XElementToNode(root.Element(nameof(result.PM10))),
                SO2 = XElementToNode(root.Element(nameof(result.SO2))),
                O3 = XElementToNode(root.Element(nameof(result.O3))),
                CO = XElementToNode(root.Element(nameof(result.CO)))
            };
            return result;
        }

        public void UpdatePersisted(AirQualityData data, AirQualityPersisted persisted)
        {
            if (data.NO2.HasValue)
            {
                persisted.NO2 = new AirQualityValue { LastDate = data.Date, Value = data.NO2.Value };
            }
            if (data.PM10.HasValue)
            {
                persisted.PM10 = new AirQualityValue { LastDate = data.Date, Value = data.PM10.Value };
            }
            if (data.SO2.HasValue)
            {
                persisted.SO2 = new AirQualityValue { LastDate = data.Date, Value = data.SO2.Value };
            }
            if (data.O3.HasValue)
            {
                persisted.O3 = new AirQualityValue { LastDate = data.Date, Value = data.O3.Value };
            }
            if (data.CO.HasValue)
            {
                persisted.CO = new AirQualityValue { LastDate = data.Date, Value = data.CO.Value };
            }
        }
    }
}
