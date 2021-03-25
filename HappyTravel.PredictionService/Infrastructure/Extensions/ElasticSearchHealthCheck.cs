using System;
using System.Threading;
using System.Threading.Tasks;
using Elasticsearch.Net;
using HappyTravel.MultiLanguage;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using Nest;

namespace HappyTravel.Osaka.Api.Infrastructure.Extensions
{
    public class ElasticSearchHealthCheck : IHealthCheck
    {
        public ElasticSearchHealthCheck(IElasticClient elasticClient, IOptions<Options.IndexOptions> indexOptions)
        {
            _elasticClient = elasticClient;
            _indexOptions = indexOptions.Value;
        }
        
        
        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            var languageCode = LanguagesHelper.GetLanguageCode(Languages.English).ToLowerInvariant();
            ElasticsearchHelper.TryGetIndex(_indexOptions.Indexes, languageCode, out var index);
            var request = new ClusterHealthRequest(index);
            var response = await _elasticClient.Cluster.HealthAsync(request, cancellationToken);

            if (!response.IsValid)
                return HealthCheckResult.Unhealthy(response.ToString());

            return response.Status switch
            {
                Health.Red => HealthCheckResult.Unhealthy(response.ToString()),
                Health.Yellow => HealthCheckResult.Degraded(response.ToString()),
                Health.Green => HealthCheckResult.Healthy("Healthy"),
                _ => throw new ArgumentOutOfRangeException()
            };
        }


        private readonly Options.IndexOptions _indexOptions;
        private readonly IElasticClient _elasticClient;
    }
}