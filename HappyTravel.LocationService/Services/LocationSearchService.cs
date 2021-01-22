using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.LocationService.Infrastructure;
using HappyTravel.MultiLanguage;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nest;
using IndexOptions = HappyTravel.LocationService.Options.IndexOptions;
using Result = CSharpFunctionalExtensions.Result;

namespace HappyTravel.LocationService.Services
{
    public class LocationSearchService : ILocationSearchService
    {
        public LocationSearchService(IElasticClient elasticClient, IOptions<IndexOptions> indexOptions, ILogger<ILocationSearchService> logger)
        {
            _indexOptions = indexOptions.Value;
            _elasticClient = elasticClient;
            _logger = logger;
        }


        public async Task<Result<List<Models.Elasticsearch.Location>>> Search(string query, int skip = 0, int top = 10, CancellationToken cancellationToken = default)
        {
            ElasticsearchHelper.TryGetIndex(_indexOptions.Indexes, Languages.English, out var index);

            var searchResponse = await _elasticClient.SearchAsync<Models.Elasticsearch.Location>(
                search => search.Index(index)
                    .From(skip)
                    .Size(top)
                    .Query(queryContainer => queryContainer.Match(matchQuery =>
                        matchQuery.Field(field => field.PredictionText).Query(query)))
                    .Sort(sort => sort.Descending(location => location.Type)), cancellationToken);

            if (!searchResponse.IsValid)
            {
                return searchResponse.ServerError is null 
                    ? EmptyLocations
                    : Result.Failure<List<Models.Elasticsearch.Location>>(searchResponse.ServerError.ToString());
            }

            return searchResponse.Documents.ToList();
        }

        
        public async Task<Result<Models.Elasticsearch.Location>> Get(string id, CancellationToken cancellationToken = default)
        {
            ElasticsearchHelper.TryGetIndex(_indexOptions.Indexes, Languages.English, out var index);
            
            var searchResponse = await _elasticClient.GetAsync<Models.Elasticsearch.Location>(id, request => request.Index(index), cancellationToken);
            
            return !searchResponse.IsValid 
                ? Result.Failure<Models.Elasticsearch.Location>($"Failed to retrieve a location by id '{id}'") 
                : searchResponse.Source;
        }

        
        private static readonly List<Models.Elasticsearch.Location> EmptyLocations = new(0);
        private readonly IndexOptions _indexOptions;
        private readonly IElasticClient _elasticClient;
        private readonly ILogger<ILocationSearchService> _logger;
    }
}