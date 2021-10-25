using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using RestService.BackgroundWorks;

namespace RestService
{
    public class CronJob: IHostedService
    {
        private readonly ILogger<CronJob> logger;
        private readonly ICheckDatabase checkDatabase;

        public CronJob(ILogger<CronJob> logger, ICheckDatabase checkDatabase)
        {
            this.logger = logger;
            this.checkDatabase = checkDatabase;
        }

        

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await checkDatabase.DoWork(cancellationToken);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            logger.LogInformation("lel stoped");
            return Task.CompletedTask;
        }
    }
}
