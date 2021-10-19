using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace HappyTravel.Osaka.Api.Services.Workers.RedisStreams
{
    public abstract class BaseStreamWorker : BackgroundService
    {
        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            throw new System.NotImplementedException();
        }
    }
}