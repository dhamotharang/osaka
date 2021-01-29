using System;
using HappyTravel.PredictionService.Services.HttpClients;
using HappyTravel.VaultClient;
using IdentityModel.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HappyTravel.PredictionService.Infrastructure.Extensions
{
    public static class HttpClientsCollectionExtension
    {
        public static IServiceCollection AddHttpClients(this IServiceCollection services, IConfiguration configuration, IVaultClient vaultClient)
        {
            var mapperClientOptions = vaultClient.Get(configuration["Elasticsearch:Mapper"]).GetAwaiter().GetResult();
            var authorityOptions = vaultClient.Get(configuration["Authority:Options"]).GetAwaiter().GetResult();
           
            services.AddAccessTokenManagement(options =>
            {
                options.Client.Clients.Add(HttpClientNames.MapperIdentityClient, new ClientCredentialsTokenRequest
                {
                    Address = $"{authorityOptions["authorityUrl"]}connect/token",
                    ClientId = mapperClientOptions["clientId"],
                    ClientSecret = mapperClientOptions["clientSecret"],
                    Scope = mapperClientOptions["scope"]
                });
            });
            services.AddClientAccessTokenClient(HttpClientNames.MapperApi, HttpClientNames.MapperIdentityClient, 
                client =>
            {
                client.BaseAddress = new Uri(mapperClientOptions["endpoint"]);
            });
            
            services.AddHttpClient(HttpClientNames.MapperApi);
            services.AddTransient<IStaticDataMapperHttpClient, StaticDataMapperHttpClient>();
            
            return services;
        }
    }
}