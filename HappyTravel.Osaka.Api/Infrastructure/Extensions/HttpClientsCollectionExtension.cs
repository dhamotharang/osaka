using System;
using System.Net.Http;
using HappyTravel.Osaka.Api.Services.HttpClients;
using HappyTravel.VaultClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Extensions.Http;

namespace HappyTravel.Osaka.Api.Infrastructure.Extensions
{
    public static class HttpClientsCollectionExtension
    {
        public static IServiceCollection AddHttpClients(this IServiceCollection services, IConfiguration configuration, IVaultClient vaultClient)
        {
            var mapperClientOptions = vaultClient.Get(configuration["Elasticsearch:Mapper"]).GetAwaiter().GetResult();

            services.AddHttpClient(HttpClientNames.MapperApi, client =>
            {
                client.BaseAddress = new Uri(mapperClientOptions["endpoint"]);
            }).AddPolicyHandler(GetDefaultRetryPolicy());
            
            services.AddTransient<IMapperHttpClient, MapperHttpClient>();
            
            return services;
        }
        
        
        private static IAsyncPolicy<HttpResponseMessage> GetDefaultRetryPolicy()
        {
            var jitter = new Random();

            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .WaitAndRetryAsync(3, attempt
                    => TimeSpan.FromSeconds(Math.Pow(1.5, attempt)) + TimeSpan.FromMilliseconds(jitter.Next(0, 100)));
        }
    }
}