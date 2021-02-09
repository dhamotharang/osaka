using System;
using HappyTravel.PredictionService.Services.HttpClients;
using HappyTravel.VaultClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HappyTravel.PredictionService.Infrastructure.Extensions
{
    public static class HttpClientsCollectionExtension
    {
        public static IServiceCollection AddHttpClients(this IServiceCollection services, IConfiguration configuration, IVaultClient vaultClient)
        {
            var mapperClientOptions = vaultClient.Get(configuration["Elasticsearch:Mapper"]).GetAwaiter().GetResult();
           
            services.AddHttpClient(HttpClientNames.MapperApi, client =>
            {
                client.BaseAddress = new Uri(mapperClientOptions["endpoint"]);
            });
            
            services.AddTransient<IMapperHttpClient, MapperHttpClient>();
            
            return services;
        }
    }
}