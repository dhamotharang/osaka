using System;
using System.Collections.Generic;
using HappyTravel.VaultClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace HappyTravel.Osaka.Api.Infrastructure
{
    public static class VaultHelper
    {
        public static VaultClient.VaultClient CreateVaultClient(IConfiguration configuration, ILoggerFactory loggerFactory = null!)
        {
            var vaultOptions = new VaultOptions
            {
                BaseUrl = new Uri(configuration[configuration["Vault:Endpoint"]]),
                Engine = configuration["Vault:Engine"],
                Role = configuration["Vault:Role"]
            };

            return new VaultClient.VaultClient(vaultOptions, loggerFactory);
        }
        
        
        public static Dictionary<string, string> GetOptions(IVaultClient vaultClient, string path, IConfiguration configuration) 
            => vaultClient.Get(configuration[path]).Result;
    }
}