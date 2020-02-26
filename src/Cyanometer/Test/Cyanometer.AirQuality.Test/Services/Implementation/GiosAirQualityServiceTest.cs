using Cyanometer.AirQuality.Services.Abstract;
using Cyanometer.AirQuality.Services.Implementation.Specific;
using Cyanometer.Core.Services.Abstract;
using Moq;
using NUnit.Framework;
using RestSharp;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Cyanometer.SkyCalculator.Test.Services.Implementation
{
    public class GiosAirQualityServiceTest
    {
        static readonly string Root = Path.Combine("Samples", "Gios");
        internal static string GetSampleContent(string file) => File.ReadAllText(Path.Combine(Root, file));
        protected GiosAirQualityService target;
        protected Mock<IRestClient> restClientMock;
        [SetUp]
        public void SetUp()
        {
            var loggerMock = new Mock<ILogger>();
            var settingsMock = new Mock<IAirQualitySettings>();
            restClientMock = new Mock<IRestClient>();
            target = new GiosAirQualityService(s => loggerMock.Object, settingsMock.Object, restClientMock.Object);
        }
        [TestFixture]
        public class GetSensorIdsAsync: GiosAirQualityServiceTest
        {
            [Test]
            public async Task WhenRealSample_ParsesResultCorrectly()
            {
                string source = GetSampleContent("sensors.json");
                var responseMock = new Mock<IRestResponse>();
                responseMock.SetupGet(mq => mq.StatusCode).Returns(System.Net.HttpStatusCode.OK);
                responseMock.SetupGet(mq => mq.Content).Returns(source);
                restClientMock.Setup(mq => mq.ExecuteGetTaskAsync(It.IsAny<IRestRequest>(), It.IsAny<CancellationToken>()))
                    .Returns(Task.FromResult(responseMock.Object));

                var actual = await target.GetSensorIdsAsync(default);

                Assert.That(actual[GiosAirQualityService.ParamId.SO2], Is.EqualTo(672));
                Assert.That(actual[GiosAirQualityService.ParamId.CO], Is.EqualTo(660));
                Assert.That(actual[GiosAirQualityService.ParamId.NO2], Is.EqualTo(665));
                Assert.That(actual[GiosAirQualityService.ParamId.O3], Is.EqualTo(667));
                Assert.That(actual[GiosAirQualityService.ParamId.PM10], Is.EqualTo(14395));
            }
        }
    }
}
