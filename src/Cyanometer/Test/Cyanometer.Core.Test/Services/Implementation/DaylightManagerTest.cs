using Cyanometer.Core.Services.Abstract;
using Cyanometer.Core.Services.Implementation;
using Cyanometer.Core.Services.Logging;
using Moq;
using NUnit.Framework;
using System;
using System.Xml.Linq;

namespace Cyanometer.Core.Test.Services.Implementation
{
    public class DaylightManagerTest
    {
        private const string Data = @"<?xml version=""1.0"" encoding=""utf-8"" ?>
<DaylightCalendar>
  <Daylight Day = ""1"" Month=""6"" Sunrise=""05:15:00"" Sunset=""20:44:00"" />
  <Daylight Day = ""2"" Month=""6"" Sunrise=""05:15:00"" Sunset=""20:45:00"" />
  <Daylight Day = ""3"" Month=""6"" Sunrise=""05:14:00"" Sunset=""20:46:00"" />
</DaylightCalendar>";
        protected DaylightManager target;
        protected Mock<ILogger> logger;
        protected XDocument dataDocument;

        [SetUp]
        public  void SetUp()
        {
            logger = new Mock<ILogger>();
            LoggerFactory loggerFactoy = (className) => logger.Object;
            target = new DaylightManager(loggerFactoy);
            dataDocument = XDocument.Parse(Data);
        }

        [TestFixture]
        public class IsDay: DaylightManagerTest
        {
            [SetUp]
            public new void SetUp()
            {
                target.LoadFrom(dataDocument);
            }
            [Test]
            public void WhenNight_ReturnsFalse()
            {
                bool actual = target.IsDay(new DateTime(2017, 6, 2, 3, 15, 0));

                Assert.That(actual, Is.False);
            }

            [Test]
            public void WhenDay_ReturnsTrue()
            {
                bool actual = target.IsDay(new DateTime(2017, 6, 2, 13, 15, 0));

                Assert.That(actual, Is.True);
            }
        }
    }
}
