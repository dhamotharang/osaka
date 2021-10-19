using HappyTravel.Osaka.Api.Infrastructure.StackExchange.Redis;
using HappyTravel.Osaka.Api.Services;
using HappyTravel.Osaka.Api.Services.Workers.RedisStreams;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis.Extensions.Core.Configuration;

namespace HappyTravel.Osaka.Api.Infrastructure.Extensions
{
    public static class PredictionsUpdateExtensions
    {
        public static IServiceCollection AddPredictionsUpdate(this IServiceCollection services, VaultClient.VaultClient vaultClient, IConfiguration configuration, IWebHostEnvironment environment)
        {
            string endpoint;
            string port;
            string syncTimeout;
            string streamName;
            
            if (environment.IsLocal())
            {
                endpoint = configuration["PredictionsUpdate:Redis:Endpoint"];
                port = configuration["PredictionsUpdate:Redis:Port"];
                streamName = configuration["PredictionsUpdate:Redis:StreamName"];
                syncTimeout = configuration["PredictionsUpdate:Redis:SyncTimeout"];
            }
            else
            {
                var redisOptions = vaultClient.Get(configuration["PredictionsUpdate:Redis"]).GetAwaiter().GetResult();
                endpoint = redisOptions["endpoint"];
                port = redisOptions["port"];
                streamName = redisOptions["streamName"];
                syncTimeout = redisOptions["syncTimeout"];
            }
            
            services.AddStackExchangeRedisExtensions<DefaultSerializer>(s 
                => new ()
                {
                    Hosts = new []
                    {
                        new RedisHost
                        {
                            Host = endpoint,
                            Port = int.Parse(port)
                        }
                    },
                    SyncTimeout = int.Parse(syncTimeout)
                });
            services.Configure<PredictionUpdateOptions>(o =>
            {
                o.StreamName = streamName;
            });
            
            services.AddHostedService<CountryStreamWorker>();
            services.AddHostedService<LocalityStreamWorker>();
            services.AddHostedService<AccommodationStreamWorker>();
            
            return services;
        }
    }
}