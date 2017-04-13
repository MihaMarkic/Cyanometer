using Cyanometer.AirQuality.Services.Abstract;
using Cyanometer.Core.Services.Abstract;
using Cyanometer.Core.Services.Logging;

namespace Cyanometer.AirQuality.Services.Implementation
{
    public class FakeShiftRegister : IShiftRegister
    {
        private readonly ILogger logger;
        public FakeShiftRegister(LoggerFactory loggerFactory)
        {
            logger = loggerFactory(nameof(FakeShiftRegister));
        }
        public void EnableLight(Lights light)
        {
            logger.LogInfo().WithCategory(LogCategory.AirQuality).WithMessage($"Shift register invoked with {light}").Commit();
        }
    }
}
