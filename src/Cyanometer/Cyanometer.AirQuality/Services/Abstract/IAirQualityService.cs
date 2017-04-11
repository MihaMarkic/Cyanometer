using Cyanometer.AirQuality.Services.Implementation;
using Cyanometer.AirQuality.Services.Implementation.Arso;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Cyanometer.AirQuality.Services.Abstract
{
    public interface IAirQualityService
    {
        Task<AirQualityData> GetIndexAsync(CancellationToken ct);
        XElement PersistedToXElement(AirQualityPersisted data);
        AirQualityPersisted XElementToPersisted(XElement root);
        void UpdatePersisted(AirQualityData data, AirQualityPersisted persisted);
    }
}