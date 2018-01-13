using Cyanometer.AirQuality.Services.Abstract;
using Cyanometer.Core.Core;
using Cyanometer.Core.Services.Abstract;
using Cyanometer.Core.Services.Logging;
using Cyanometer.Imagging.Services.Abstract;
using Cyanometer.Manager.Services.Abstract;
using Exceptionless;
using Righthand.WittyPi;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnixSignalWaiter;

namespace Cyanometer.Manager.Services.Implementation
{
    public class Processor : IProcessor
    {
        private readonly ILogger logger;
        private readonly ISettings settings;
        private readonly IImageProcessor imageProcessor;
        private readonly IWittyPiService wittyPiService;
        private readonly INtpService ntpService;
        private readonly IStopCheckService stopCheckService;
        private readonly IAirQualityProcessor airQualityProcessor;
        private CancellationTokenSource cts;

        public Processor(LoggerFactory loggerFactory, ISettings settings, IImageProcessor imageProcessor, IWittyPiService wittyPiService,
            INtpService ntpService, IStopCheckService stopCheckService, IAirQualityProcessor airQualityProcessor)
        {
            this.logger = loggerFactory(nameof(Processor));
            this.settings = settings;
            this.imageProcessor = imageProcessor;
            this.wittyPiService = wittyPiService;
            this.ntpService = ntpService;
            this.stopCheckService = stopCheckService;
            this.airQualityProcessor = airQualityProcessor;
        }
        public void Process()
        {
            cts = new CancellationTokenSource();
            var ct = cts.Token;

            Console.CancelKeyPress += (s, e) =>
            {
                logger.LogInfo().WithCategory(LogCategory.Manager).WithMessage("Waiting for cancel to finish").Commit();
                cts.Cancel();
                e.Cancel = true;
            };
            if (SignalWaiter.Instance.CanWaitExitSignal())
            {
                logger.LogInfo().WithCategory(LogCategory.Manager).WithMessage("Will wait for SIGTERM as well").Commit();
                Task.Factory.StartNew(a =>
                {
                    SignalWaiter.Instance.WaitExitSignal();
                    cts.Cancel();
                    logger.LogInfo().WithCategory(LogCategory.Manager).WithMessage("Received SIGTERM").Commit();
                }, ct, TaskCreationOptions.LongRunning);
            }
            try
            {
                PrepareForLoopAsync(ct).Wait(ct);
                bool shouldLoop = settings.CycleWaitMinutes > 0;
                do
                {
                    DateTime now = DateTime.Now;
                    Loop(ct);
                    var delay = new TimeSpan(0, settings.CycleWaitMinutes, 0) - (DateTime.Now - now);
                    if (shouldLoop)
                    {
                        logger.LogInfo().WithCategory(LogCategory.Manager).WithMessage($"Will cycle wait for {delay}").Commit();
                        if (delay.TotalSeconds > 0)
                        {
                            Task.Delay(delay, ct).Wait(ct);
                        }
                    }
                } while (shouldLoop);
            }
            catch (OperationCanceledException)
            {
                logger.LogInfo().WithCategory(LogCategory.Manager).WithMessage($"Gracefully exiting loop due to cancellation").Commit();
            }

            if (settings.ExceptionlessEnabled)
            {
                logger.LogInfo().WithCategory(LogCategory.Manager).WithMessage("Flushing Exceptionless").Commit();
                ExceptionlessClient.Default.ProcessQueue();
            }
            logger.LogInfo().WithCategory(LogCategory.Manager).WithMessage("All done, exiting").Commit();
        }

        public async Task PrepareForLoopAsync(CancellationToken ct)
        {
            if (settings.InitialDelay > 0)
            {
                TimeSpan initialDelay = TimeSpan.FromMinutes(settings.InitialDelay);
                logger.LogInfo().WithCategory(LogCategory.Manager).WithMessage($"Safe waiting for {initialDelay}").Commit();
                await Task.Delay(initialDelay, ct);
                logger.LogInfo().WithCategory(LogCategory.Manager).WithMessage($"Safe waiting done").Commit();
            }
            if (settings.SleepMinutes > 0)
            {
                await SetSystemTimeAsync(ct);
            }
        }

        public async Task SetSystemTimeAsync(CancellationToken ct)
        {
            DateTime now = DateTime.UtcNow;
            await Retrier.RetryAsync(() =>
            {
                now = ntpService.GetNetworkTime();
                logger.LogInfo().WithCategory(LogCategory.Manager).WithMessage($"NTP time: {now} UTC: {now.ToUniversalTime()}").Commit();
                now = now.ToUniversalTime();
            }, logger, 5, "Read NTP", false, ct);
            SetSystemTime(now);
            await Retrier.RetryAsync(() =>
            {
                wittyPiService.RtcDateTime = now;
                logger.LogInfo().WithCategory(LogCategory.Manager).WithMessage($"RTC set to : {wittyPiService.RtcDateTime.ToLocalTime()}").Commit();
            }, logger, 5, "Read RTC", true, ct);
        }

        public void Loop(CancellationToken ct)
        {
            var loops = new List<Task>(2);
            DateTime start = DateTime.Now;
            Task<bool> canShutdownTask = settings.SleepMinutes > 0 ? stopCheckService.CanShutdownAsync(ct) : null;
            if (settings.ProcessImages)
            {
                var imageProcessorTask = imageProcessor.LoopAsync(ct);
                loops.Add(imageProcessorTask);
                logger.LogInfo().WithCategory(LogCategory.Manager).WithMessage("Image processing is enabled").Commit();
            }
            if (settings.ProcessAirQuality)
            {
                var airQualityProcessorTask = airQualityProcessor.LoopAsync(ct);
                loops.Add(airQualityProcessorTask);
                logger.LogInfo().WithCategory(LogCategory.Manager).WithMessage("AirQuality processing is enabled").Commit();
            }
            try
            {
                Task.WhenAll(loops).Wait(ct);
            }
            catch (OperationCanceledException)
            {
                logger.LogInfo().WithCategory(LogCategory.Manager).WithMessage("Processor canceled, will exit").Commit();
                return;
            }
            catch (Exception ex)
            {
                logger.LogError().WithCategory(LogCategory.Manager).WithMessage("General failure").WithException(ex).Commit();
            }
            bool canShutdown = canShutdownTask?.Result ?? false;
            if (canShutdown && settings.SleepMinutes > 0)
            {
                SystemSleepAsync(start, ct).Wait(ct);
            }
            logger.LogInfo().WithCategory(LogCategory.Manager).WithMessage("Exiting").Commit();
        }

        public async Task SystemSleepAsync(DateTime start, CancellationToken ct)
        {
            DateTime wakeUpTime = start.AddMinutes(settings.SleepMinutes);
            DateTime now = DateTime.UtcNow;
            logger.LogInfo().WithCategory(LogCategory.Manager).WithMessage($"RTC: {now.ToLocalTime()}, wakeup at {wakeUpTime.ToLocalTime()}").Commit();
            await Retrier.RetryAsync(() =>
            {
                wittyPiService.WakeUp = WakeUpDateTime.Hourly((byte)wakeUpTime.Minute, (byte)wakeUpTime.Second);
            }, logger, 5, "Setting wakeup", true, ct);
            logger.LogInfo().WithCategory(LogCategory.Manager).WithMessage("Shutting down").Commit();
            const int sleepDelaySeconds = 100;
            var sleep = now.AddSeconds(sleepDelaySeconds);
            logger.LogInfo().WithCategory(LogCategory.Manager).WithMessage($"Going to sleep in {sleepDelaySeconds} secs at {sleep.ToLocalTime()}").Commit();
            await Retrier.RetryAsync(() =>
            {
                wittyPiService.Sleep = new SleepDateTime((byte)sleep.Day, (byte)sleep.Hour, (byte)sleep.Minute);
            }, logger, 5, "Setting sleep", true, ct);
        }

        public void SetSystemTime(DateTime date)
        {
            //logger.Info("Getting system time");
            //Timespec sr = new Timespec();
            //int r1 = Interop.clock_gettime(Interop.CLOCK_REALTIME, sr);
            //logger.Info($"Read {r1} {sr.tv_sec} {sr.tv_nsec}");
            var diff = (date - new DateTime(1970, 1, 1));
            Timespec ts = new Timespec
            {
                tv_sec = (int)Math.Abs(diff.TotalSeconds),
                tv_nsec = (int)diff.Milliseconds * 1000000
            };
            logger.LogInfo().WithCategory(LogCategory.Manager).WithMessage($"Settings {ts.tv_sec} {ts.tv_nsec}").Commit();
            int result = Interop.clock_settime(Interop.CLOCK_REALTIME, ts);
            logger.LogInfo().WithCategory(LogCategory.Manager).WithMessage($"Date after system set is {DateTime.Now} with result {result}").Commit();
        }
    }
}
