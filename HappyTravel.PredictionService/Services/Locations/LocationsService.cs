﻿using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.MultiLanguage;
using HappyTravel.PredictionService.Infrastructure;
using HappyTravel.PredictionService.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nest;
using IndexOptions = HappyTravel.PredictionService.Options.IndexOptions;
using Location = HappyTravel.PredictionService.Models.Elasticsearch.Location;
using Result = CSharpFunctionalExtensions.Result;

namespace HappyTravel.PredictionService.Services.Locations
{
    public class LocationsService : ILocationsService
    {
        public LocationsService(IElasticClient elasticClient, IOptions<IndexOptions> indexOptions, ILogger<ILocationsService> logger)
        {
            _indexOptions = indexOptions.Value;
            _elasticClient = elasticClient;
            _logger = logger;
        }


        public async Task<Result<List<Location>>> Search(string query, int skip = 0, int top = 10, CancellationToken cancellationToken = default)
        {
            ElasticsearchHelper.TryGetIndex(_indexOptions.Indexes!, Languages.English, out var index);

            var response = await _elasticClient.SearchAsync<Location>(
                search => search.Index(index)
                    .Query(searchQuery => searchQuery.Bool(boolQuery =>
                        boolQuery
                            .Must(mustQuery => mustQuery.Match(matchQuery =>
                                matchQuery.Field(location => location.PredictionText)
                                    .Query(query)
                                    .Operator(Operator.And)))
                            .Should(shouldQuery => shouldQuery.Term(termQuery =>
                                    termQuery.Field(location => location.LocationType)
                                        .Value(MapperLocationTypes.Country)),
                                shouldQuery => shouldQuery.Term(termQuery =>
                                    termQuery.Field(location => location.LocationType)
                                        .Value(MapperLocationTypes.Locality)))))
                    .From(0)
                    .Size(MaxLocationsNumber),
                cancellationToken);

            return response.Documents.ToList();
        }
        
        
        public async Task<Result<Location>> Get(string htId, CancellationToken cancellationToken = default)
        {
            ElasticsearchHelper.TryGetIndex(_indexOptions.Indexes, Languages.English, out var index);
            
            var searchResponse = await _elasticClient.GetAsync<Location>(htId, request => request.Index(index), cancellationToken);
            
            return !searchResponse.IsValid 
                ? Result.Failure<Location>($"Failed to retrieve a location by htId '{htId}'") 
                : searchResponse.Source;
        }
        
        
        private readonly IndexOptions _indexOptions;
        private readonly IElasticClient _elasticClient;
        private readonly ILogger<ILocationsService> _logger;

        private const int MaxLocationsNumber = 10;
    }
}