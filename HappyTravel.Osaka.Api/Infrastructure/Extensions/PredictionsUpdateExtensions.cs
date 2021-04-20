using System.Linq;
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
        public static IServiceCollection AddPredictionsUpdate(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment environment)
        {
            var role = configuration["PredictionsUpdate:Role"];
            using var vaultClient = VaultHelper.CreateVaultClient(configuration, role);
            var token = configuration[configuration["Vault:Token"]];
            vaultClient.Login(token).GetAwaiter().GetResult();
            var redisOptions = vaultClient.Get(configuration["PredictionsUpdate:Redis"]).GetAwaiter().GetResult();
            string endpoint;
            string port;
            string streamName;
            if (environment.IsLocal())
            {
                endpoint = configuration["PredictionsUpdate:Redis:Endpoint"];
                port = configuration["PredictionsUpdate:Redis:Port"];
                streamName = configuration["PredictionsUpdate:Redis:Stream"];
            }
            else
            {
                endpoint = redisOptions["endpoint"];
                port = configuration["port"];
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