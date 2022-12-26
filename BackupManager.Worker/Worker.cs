using BackupManager.Worker.Jobs;
using Quartz;
using Quartz.Impl;
using System.Collections.Specialized;
using System.Reflection.Metadata;

namespace BackupManager.Worker
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;

        public Worker(ILogger<Worker> logger)
        {
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // Making sure that worker service only functions in linux.
            // Not needed at the moment.
            //if(!OperatingSystem.IsLinux())
            //{
            //    throw new Exception("Only linux distros are supported.");
            //}
            try
            {
                // and last shut down the scheduler when you are ready to close your program
                while (!stoppingToken.IsCancellationRequested)
                {
                    _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);


                    await Task.Delay(1000, stoppingToken);
                }


            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                await Task.FromCanceled(stoppingToken);
            }
            
        }
    }
}