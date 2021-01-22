using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace HappyTravel.LocationService.Infrastructure
{
    public static class EnvironmentVariableHelper
    {
        public static string Get(string key, IConfiguration configuration)
        {
            var environmentVariable = configuration[key];
            if (environmentVariable is null)
                throw new Exception($"Couldn't obtain a value for '{key}' configuration key.");

            return Environment.GetEnvironmentVariable(environmentVariable);
        }


        public static bool IsLocal(this IHostEnvironment hostingEnvironment) 
            => hostingEnvironment.IsEnvironment(LocalEnvironment);


        public static bool IsLocal()
        {
            var environmentVariable = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            
            return !string.IsNullOrEmpty(environmentVariable) && environmentVariable.ToUpperInvariant().Equals("LOCAL");
        }
        

        private const string LocalEnvironment = "Local";
    }
}