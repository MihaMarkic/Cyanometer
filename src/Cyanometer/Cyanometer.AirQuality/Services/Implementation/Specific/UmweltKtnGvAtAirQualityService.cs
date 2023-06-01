using Cyanometer.AirQuality.Services.Abstract;
using Cyanometer.AirQuality.Services.Implementation.Arso;
using Cyanometer.Core.Services.Abstract;
using Cyanometer.Core.Services.Logging;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Cyanometer.AirQuality.Services.Implementation.Specific
{
    public class UmweltKtnGvAtAirQualityService : AirQualityService, IAirQualityService
    {
        public UmweltKtnGvAtAirQualityService(LoggerFactory loggerFactory, IAirQualitySettings settings, RestClient client)
            : base(loggerFactory, settings, client, "http://www.umwelt.ktn.gv.at/luft/online/Daten/AtelierPrusnik/AtelierPrusnik.txt")
        {
        }
        public async Task<AirQualityData> GetIndexAsync(CancellationToken ct)
        {
            var lines = await GetDataAsync(ct);
            var stations = ParseData(logger, lines);
            var date = stations
                    .SingleOrDefault(s => s.ComponentCode == ComponentCode.O3)
                    .Measurements
                    .OrderByDescending(m => m.Date)
                    .FirstOrDefault(m => m.IsValid)
                    .Date;
            var result = new AirQualityData
            {
                Date = date,
                O3 = GetMeasurement(stations, ComponentCode.O3),
                PM10 = GetMeasurement(stations, ComponentCode.PM10),
                NO2 = GetMeasurement(stations, ComponentCode.NO2),
                SO2 = GetMeasurement(stations, ComponentCode.SO2),
            };
            return result;
        }
        double? GetMeasurement(Station[] stations, ComponentCode componentCode)
        {
            var result = stations
                    .Single(s => s.ComponentCode == componentCode)
                    .Measurements
                    .OrderByDescending(m => m.Date)
                    .FirstOrDefault(m => m.IsValid);
            if (result != null)
            {
                return result.Value;
            }
            return null;
        }
        public static Station[] ParseData(ILogger logger, string[] lines)
        {
            int index = 0;
            var stations = new List<Station>();
            while (true)
            {
                List<Measurement> measurements = new List<Measurement>();
                Station station = null;
                string line = lines[index];
                if (line[0] != '>')
                {
                    throw new Exception("Expected station start char > but got " + line[0]);
                }
                ComponentCode? componentCode;
                station = new Station
                {
                    MeasurementNetworkIdentifier = line.Substring(1, 2),
                    StationIdentifier = line.Substring(3, 4),
                    ComponentSubNumber = line.Substring(17, 2).TrimEnd(),
                    TimeSeriesType = line.Substring(19, 8).TrimEnd(),
                    MeasuredValueUnit = line.Substring(27, 10).TrimEnd(),
                    Description = line.Substring(37).TrimEnd(),
                };
                switch (line.Substring(7, 10).TrimEnd())
                {
                    case "SO2":
                        componentCode = ComponentCode.SO2;
                        break;
                    case "PM10":
                        componentCode = ComponentCode.PM10;
                        break;
                    case "NO2":
                        componentCode = ComponentCode.NO2;
                        break;
                    case "O3":
                        componentCode = ComponentCode.O3;
                        break;
                    default:
                        componentCode = null;
                        break;
                }
                index++;
                while (!lines[index].StartsWith(">") && !lines[index].StartsWith("==ENDE=="))
                {
                    if (componentCode != null)
                    {
                        line = lines[index];
                        ControlLevel controlLevel;
                        switch (line[13 - 1])
                        {
                            case '1':
                                controlLevel = ControlLevel.Unverified;
                                break;
                            case '2':
                                controlLevel = ControlLevel.Measured;
                                break;
                            case '3':
                                controlLevel = ControlLevel.ProvisionallyInspected;
                                break;
                            case '4':
                                controlLevel = ControlLevel.Controlled;
                                break;
                            default:
                                throw new Exception("Invalid control level value " + line[13 - 1]);
                        }
                        Validity validity;
                        switch (line[14 - 1])
                        {
                            case '1':
                                validity = Validity.NoValueYet;
                                break;
                            case '2':
                                validity = Validity.UpTo90Percent;
                                break;
                            case '3':
                                validity = Validity.Valid;
                                break;
                            case '4':
                                validity = Validity.Invalid;
                                break;
                            default:
                                throw new Exception("Invalid validity value " + line[14 - 1]);
                        }
                        string valueText = line.Substring(14);
                        int hour = int.Parse(line.Substring(8, 2));
                        if (!string.IsNullOrWhiteSpace(valueText))
                        {
                            Measurement measurement = new Measurement
                            {
                                Date = new DateTime(
                                    int.Parse(line.Substring(0, 4)),
                                    int.Parse(line.Substring(4, 2)),
                                    int.Parse(line.Substring(6, 2)),
                                    hour == 24 ? 0 : hour,
                                    int.Parse(line.Substring(10, 2)),
                                    0),
                                ControlLevel = controlLevel,
                                Validity = validity,
                                Value = double.Parse(valueText, CultureInfo.InvariantCulture)
                            };
                            measurements.Add(measurement);
                        }
                    }
                    index++;
                }
                if (componentCode.HasValue)
                {
                    station.Measurements = measurements.ToArray();
                    station.ComponentCode = componentCode.Value;
                    stations.Add(station);
                }

                if (lines[index].StartsWith("==ENDE=="))
                {
                    break;
                }
            }
            return stations.ToArray();
        }
        public async Task<string[]> GetDataAsync(CancellationToken ct)
        {
            var request = new RestRequest("http://www.umwelt.ktn.gv.at/luft/online/Daten/AtelierPrusnik/AtelierPrusnik.txt", Method.Get);
            request.AddHeader("Accept", "text/plain");
            var response = await client.GetAsync(request, ct);
            var stringReader = new StringReader(response.Content);
            var lines = new List<string>();
            string line;
            while ((line = stringReader.ReadLine()) != null)
            {
                lines.Add(line);
            }
            return lines.ToArray();
            
        }
    }

    public class Station
    {
        public string MeasurementNetworkIdentifier { get; set; }
        public string StationIdentifier { get; set; }
        public ComponentCode ComponentCode { get; set; }
        public string ComponentSubNumber { get; set; }
        public string TimeSeriesType { get; set; }
        public string MeasuredValueUnit { get; set; }
        public string Description { get; set; }
        public Measurement[] Measurements { get; set; }
    }
    public class Measurement
    {
        public DateTime Date { get; set; }
        public ControlLevel ControlLevel { get; set; }
        public Validity Validity { get; set; }
        public double Value { get; set; }
        public bool IsValid => (Validity == Validity.Valid || Validity == Validity.UpTo90Percent);
    }

    public enum ControlLevel
    {
        Unverified,
        Measured,
        ProvisionallyInspected,
        Controlled
    }

    public enum Validity
    {
        NoValueYet,
        UpTo90Percent,
        Valid,
        Invalid
    }

    public enum ComponentCode
    {
        SO2,
        PM10,
        NO2,
        O3,
    }
}
