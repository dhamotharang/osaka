using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Elasticsearch.Net;
using HappyTravel.MultiLanguage;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using Nest;

namespace HappyTravel.Osaka.Api.Infrastructure.Extensions
{
    public class ElasticHealthCheck : IHealthCheck
    {
        public ElasticHealthCheck(IElasticClient elasticClient, IOptions<Options.IndexesOptions> indexOptions)
        {
            _elasticClient = elasticClient;
            _indexesOptions = indexOptions.Value;
        }
        
        
        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            var result = await Task.WhenAll(CheckCountriesIndexHealth(cancellationToken),
                CheckLocalitiesIndexHealth(cancellationToken), CheckAccommodationIndexHealth(cancellationToken));

            var unhealthy = result.SingleOrDefault(r => r.Status == HealthStatus.Unhealthy);
            if (unhealthy.Status == HealthStatus.Unhealthy)
                return unhealthy;
            
            var degraded = result.SingleOrDefault(r => r.Status == HealthStatus.Degraded);
            if (degraded.Status == HealthStatus.Degraded)
                return degraded;

            return result.First();
        }

        
        public Task<HealthCheckResult> CheckCountriesIndexHealth(CancellationToken cancellationToken = default)
            => CheckIndexHealth(_indexesOptions.EnglishIndexes.Countries, cancellationToken);


        public Task<HealthCheckResult> CheckLocalitiesIndexHealth(CancellationToken cancellationToken = default)
            => CheckIndexHealth(_indexesOptions.EnglishIndexes.Localities, cancellationToken);
        
        
        public Task<HealthCheckResult> CheckAccommodationIndexHealth(CancellationToken cancellationToken = default)
            => CheckIndexHealth(_indexesOptions.EnglishIndexes.Accommodations, cancellationToken);
        
        
        private async Task<HealthCheckResult> CheckIndexHealth(string indexName, CancellationToken cancellationToken = default)
        {
            var request = new ClusterHealthRequest(indexName);
            var response = await _elasticClient.Cluster.HealthAsync(request, cancellationToken);
            
            return !response.IsValid 
                ? HealthCheckResult.Unhealthy(response.ToString()) 
                : GetHealthStatus(response);
        }
        
        
        private HealthCheckResult GetHealthStatus(ClusterHealthResponse clusterHealth)
        {
            return clusterHealth.Status switch
            {
                Health.Red => HealthCheckResult.Unhealthy(clusterHealth.ToString()),
                Health.Yellow => HealthCheckResult.Degraded(clusterHealth.ToString()),
                Health.Green => HealthCheckResult.Healthy("Healthy"),
                _ => throw new ArgumentOutOfRangeException()
            };
        }
        
        
        private readonly Options.IndexesOptions _indexesOptions;
        private readonly IElasticClient _elasticClient;
    }
}