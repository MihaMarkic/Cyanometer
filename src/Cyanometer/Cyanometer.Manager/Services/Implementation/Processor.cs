using Cyanometer.Core.Services.Abstract;
using Cyanometer.Core.Services.Logging;
using Cyanometer.Imagging.Services.Abstract;
using Cyanometer.Manager.Services.Abstract;
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
        private static CancellationTokenSource cts;

        public Processor(LoggerFactory loggerFactory, ISettings settings, IImageProcessor imageProcessor)
        {
            this.logger = loggerFactory(nameof(Processor));
            this.settings = settings;
            this.imageProcessor = imageProcessor;
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
                    logger.LogInfo().WithCategory(LogCategory.Manager).WithMessage("Receivd SIGTERM").Commit();
                }, ct, TaskCreationOptions.LongRunning);
            }
            Loop(ct);
        }

        private void Loop(CancellationToken ct)
        {
            var loops = new List<Task>(2);
            if (settings.ProcessImages)
            {
                var imageProcessorTask = imageProcessor.LoopAsync(TimeSpan.FromMinutes(settings.CycleWaitMinutes), ct);
                loops.Add(imageProcessorTask);
            }
            try
            {
                Task.WhenAll(loops).Wait(ct);
            }
            catch (OperationCanceledException)
            {
                logger.LogInfo().WithCategory(LogCategory.Manager).WithMessage("Processor canceled");
            }
            catch (Exception ex)
            {
                logger.LogError().WithCategory(LogCategory.Manager).WithMessage("General failure").WithException(ex).Commit();
            }
            logger.LogInfo().WithCategory(LogCategory.Manager).WithMessage("Exiting").Commit();
        }
    }
}
