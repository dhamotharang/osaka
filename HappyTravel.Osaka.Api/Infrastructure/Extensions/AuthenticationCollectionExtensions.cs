using System;
using System.Net;
using System.Net.Http;
using HappyTravel.Osaka.Api.Services.HttpClients;
using HappyTravel.VaultClient;
using IdentityModel.Client;
using IdentityServer4.AccessTokenValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Extensions.Http;

namespace HappyTravel.Osaka.Api.Infrastructure.Extensions
{
    public static class AuthenticationCollectionExtensions
    {
        public static IServiceCollection ConfigureAuthentication(this IServiceCollection services, IConfiguration configuration, IHostEnvironment environment, IVaultClient vaultClient)
        {
            var (apiName, authorityUrl) = GetApiNameAndAuthority(configuration, environment, vaultClient);

            services.AddAuthentication(IdentityServerAuthenticationDefaults.AuthenticationScheme)
                .AddIdentityServerAuthentication(options =>
                {
                    options.Authority = authorityUrl;
                    options.ApiName = apiName;
                    options.RequireHttpsMetadata = true;
                    options.SupportedTokens = SupportedTokens.Jwt;
                });

            return services;
        }

        
        public static IServiceCollection AddHttpClients(this IServiceCollection services, IConfiguration configuration, IHostEnvironment environment, IVaultClient vaultClient)
        {
            var (_, authorityUrl) = GetApiNameAndAuthority(configuration, environment, vaultClient);
            var mapperClientOptions = vaultClient.Get(configuration["Elasticsearch:Mapper"]).GetAwaiter().GetResult();
           
            services.AddAccessTokenManagement(options =>
            {
                options.Client.Clients.Add(HttpClientNames.Identity, new ClientCredentialsTokenRequest
                    {
                        Address = $"{authorityUrl}connect/token",
                        ClientId = mapperClientOptions["clientId"],
                        ClientSecret = mapperClientOptions["clientSecret"],
                        Scope = mapperClientOptions["scope"]
                    });
            });
            
            //services.AddHttpClient(HttpClientNames.MapperApi, client => { client.BaseAddress = new Uri(mapperClientOptions["endpoint"]); })
            services.AddHttpClient(HttpClientNames.MapperApi, client => { client.BaseAddress = new Uri("http://localhost:5080"); })
                .AddPolicyHandler((serviceProvider, _)
                    => HttpPolicyExtensions
                        .HandleTransientHttpError()
                        .OrResult(result => result.StatusCode == HttpStatusCode.Unauthorized)
                        .WaitAndRetryAsync(new[]
                            {
                                TimeSpan.FromMilliseconds(500),
                                TimeSpan.FromMilliseconds(1000),
                                TimeSpan.FromMilliseconds(3000)
                            },
                            (delegateHandler, timespan, retryAttempt, _) =>
                            {
                                var errorMessage = delegateHandler.Exception?.Message
                                                   ??
                                                   $"{delegateHandler.Result.StatusCode} {delegateHandler.Result.Content.ReadAsStringAsync().Result}";

                                serviceProvider.GetService<ILogger<HttpClient>>()
                                    .LogWarning(
                                        "Delaying client {Client} for {Delay}ms: '{Message}', then making retry {Retry}",
                                        HttpClientNames.MapperApi, timespan.TotalMilliseconds, errorMessage,
                                        retryAttempt);
                            }
                        ))
            .AddClientAccessTokenHandler(HttpClientNames.Identity);
            
            services.AddTransient<IMapperHttpClient, MapperHttpClient>();

            return services;
        }

        
        private static (string apiName, string authorityUrl) GetApiNameAndAuthority(IConfiguration configuration, IHostEnvironment environment, IVaultClient vaultClient)
        {
            var authorityOptions = vaultClient.Get(configuration["Authority:Options"]).GetAwaiter().GetResult();

            var apiName = configuration["Authority:ApiName"];
            var authorityUrl = configuration["Authority:Endpoint"];
            if (environment.IsLocal()) 
                return (apiName, authorityUrl);

            apiName = authorityOptions["apiName"];
            authorityUrl = authorityOptions["authorityUrl"];
            
            return (apiName, authorityUrl);
        }
    }
}