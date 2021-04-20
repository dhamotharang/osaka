using HappyTravel.Osaka.Api.Infrastructure.StackExchange.Redis;
using HappyTravel.Osaka.Api.Services;
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
            var redisOptions = vaultClient.Get(configuration["PredictionsUpdate:Redis"]).GetAwaiter().GetResult();
            string endpoint;
            string port;
            string streamName;
            if (environment.IsLocal())
            {
                endpoint = configuration["PredictionsUpdate:Redis:Endpoint"];
                port = configuration["PredictionsUpdate:Redis:Port"];
                streamName = configuration["PredictionsUpdate:Redis:StreamName"];
            }
            else
            {
                endpoint = redisOptions["endpoint"];
                port = redisOptions["port"];
                streamName = redisOptions["streamName"];
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
                    }
                });
            services.Configure<PredictionUpdateOptions>(o =>
            {
                o.StreamName = streamName;
            });
            
            services.AddHostedService<UpdateFromStreamWorker>();
            
            return services;
        }
    }
}