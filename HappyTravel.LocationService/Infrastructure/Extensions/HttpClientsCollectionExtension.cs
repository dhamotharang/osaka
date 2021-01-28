using System;
using HappyTravel.LocationService.Services.HttpClients;
using HappyTravel.VaultClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HappyTravel.LocationService.Infrastructure.Extensions
{
    public static class HttpClientsCollectionExtension
    {
        public static IServiceCollection AddHttpClients(this IServiceCollection services, IConfiguration configuration, IVaultClient vaultClient)
        {
            var options = vaultClient.Get(configuration["Elasticsearch:Mapper"]).GetAwaiter().GetResult();
            services.AddHttpClient<MapperHttpClient>(client => client.BaseAddress = new Uri(options["endpoint"]));

            return services;
        }
    }
}