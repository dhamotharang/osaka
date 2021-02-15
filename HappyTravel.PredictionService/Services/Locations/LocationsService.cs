using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.MultiLanguage;
using HappyTravel.PredictionService.Infrastructure;
using HappyTravel.PredictionService.Infrastructure.Logging;
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
        
        
        public async Task<List<Location>> Search(string query, CancellationToken cancellationToken = default)
        {
            _logger.LogPredictionsQuery(query);
            
            ElasticsearchHelper.TryGetIndex(_indexOptions.Indexes!, Languages.English, out var index);
            const string countrySuggester = "countrySuggester";
            const string localitySuggester = "localitySuggester";
            const string accommodationSuggester = "accommodationSuggester";
            const int maxLocationsCount = 10;
            
            var searchResponse = await _elasticClient.SearchAsync<Location>(search => search.Index(index).Suggest(suggest => CreateSuggestionRequests(suggest, query)), cancellationToken);
            
            var result = GetLocations(searchResponse, countrySuggester).ToList();
            if (result.Count == maxLocationsCount)
                return result;

            var locations = GetLocations(searchResponse, localitySuggester, result.Count);
            result.AddRange(locations);
            if (result.Count == maxLocationsCount)
                return result;

            locations = GetLocations(searchResponse, accommodationSuggester, result.Count);
            result.AddRange(locations);
            
            return result;

            
           static IPromise<ISuggestContainer> CreateSuggestionRequests(SuggestContainerDescriptor<Location> suggestContainer, string query)
            {
                return suggestContainer
                    .Completion(countrySuggester,suggester => AddSuggester(suggester, MapperLocationTypes.Country))
                    .Completion(localitySuggester,suggester => AddSuggester(suggester, MapperLocationTypes.Locality))
                    .Completion(accommodationSuggester, suggester => AddSuggester(suggester, MapperLocationTypes.Accommodation));

                
                ICompletionSuggester AddSuggester(CompletionSuggesterDescriptor<Location> suggester, MapperLocationTypes contextType) 
                    => suggester.Field(field => field.Suggestion)
                        .Prefix(query)
                        .Contexts(context => context.Context("type", category => category.Context(GetContextName(contextType))))
                        .Size(maxLocationsCount);
            }


            static string GetContextName(MapperLocationTypes type) => type.ToString("G").ToLowerInvariant();
            
                
            static IEnumerable<Location> GetLocations(ISearchResponse<Location> searchResponse, string suggester, int foundedLocationsCount = 0)
                => searchResponse.Suggest[suggester].SelectMany(c => c.Options)
                    .Select(o => o.Source).Take(maxLocationsCount - foundedLocationsCount);
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
    }
}