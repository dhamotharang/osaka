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
            string redisEndpoint;
            string stream;
            if (environment.IsLocal())
            {
                redisEndpoint = configuration["PredictionsUpdate:Redis:Endpoint"];
                stream = configuration["PredictionsUpdate:Redis:Stream"];
            }
            else
            {
                redisEndpoint = redisOptions["endpoint"];
                stream = redisOptions["stream"];
            }
            
            services.AddStackExchangeRedisExtensions<DefaultSerializer>(s 
                => new ()
                {
                    Hosts = new []
                    {
                        new RedisHost
                        {
                            Host = redisEndpoint,
                            Port = 6379
                        }
                    }
                });
            services.Configure<PredictionUpdateOptions>(o =>
            {
                o.Stream = stream;
            });
            
            services.AddHostedService<UpdateFromStreamWorker>();
            
            return services;
        }
    }
}