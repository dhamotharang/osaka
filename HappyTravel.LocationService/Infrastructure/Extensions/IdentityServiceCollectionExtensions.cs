using HappyTravel.VaultClient;
using IdentityServer4.AccessTokenValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace HappyTravel.LocationService.Infrastructure.Extensions
{
    public static class IdentityServiceCollectionExtensions
    {
        public static IServiceCollection ConfigureAuthentication(this IServiceCollection services, IConfiguration configuration,
            IHostEnvironment environment, IVaultClient vaultClient)
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
        
        
        private static (string apiName, string authorityUrl) GetApiNameAndAuthority(IConfiguration configuration, IHostEnvironment environment,
            IVaultClient vaultClient)
        {
            var authorityOptions = vaultClient.Get(configuration["Authority:Options"]).GetAwaiter().GetResult();

            var apiName = configuration["Authority:ApiName"];
            var authorityUrl = configuration["Authority:Endpoint"];
            if (environment.IsDevelopment() || environment.IsLocal())
                return (apiName, authorityUrl);

            apiName = authorityOptions["apiName"];
            authorityUrl = authorityOptions["authorityUrl"];

            return (apiName, authorityUrl);
        }
    }
}