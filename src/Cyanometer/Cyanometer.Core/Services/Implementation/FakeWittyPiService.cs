//using System;
//using Righthand.WittyPi;
//using Cyanometer.Core.Services.Logging;
//using Cyanometer.Core.Services.Abstract;

//namespace Cyanometer.Core.Services.Implementation
//{
//    public class FakeWittyPiService : IWittyPiService
//    {
//        private readonly ILogger logger;
//        public FakeWittyPiService(LoggerFactory loggerFactory)
//        {
//            logger = loggerFactory(nameof(FakeWittyPiService));
//        }
//        public WakeUpDateTime WakeUp {
//            get => new WakeUpDateTime();
//            set => logger.LogInfo().WithCategory(LogCategory.System).WithMessage($"WakeUpTime set").Commit();
//        }
//        public SleepDateTime Sleep
//        { get => new SleepDateTime();
//            set => logger.LogInfo().WithCategory(LogCategory.System).WithMessage($"SleepDateTime set").Commit();
//        }
//        public DateTime RtcDateTime {
//            get => DateTime.Now;
//            set => logger.LogInfo().WithCategory(LogCategory.System).WithMessage($"RtcDateTime set").Commit(); }
//    }
//}
