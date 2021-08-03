using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.MapperContracts.Internal.Mappings.Enums;
using HappyTravel.MultiLanguage;
using HappyTravel.Osaka.Api.Infrastructure;
using HappyTravel.Osaka.Api.Infrastructure.Logging;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nest;
using IndexOptions = HappyTravel.Osaka.Api.Options.IndexOptions;
using Location = HappyTravel.Osaka.Api.Models.Elasticsearch.Location;
using Result = CSharpFunctionalExtensions.Result;

namespace HappyTravel.Osaka.Api.Services.Locations
{
    public class LocationsService : ILocationsService
    {
        public LocationsService(IElasticClient elasticClient, IOptions<IndexOptions> indexOptions, ILogger<ILocationsService> logger)
        {
            _indexOptions = indexOptions.Value;
            _elasticClient = elasticClient;
            _logger = logger;
        }
        
        
        public async Task<Result<Location>> Get(string htId, CancellationToken cancellationToken = default)
        {
            ElasticsearchHelper.TryGetIndex(_indexOptions.Indexes, Languages.English, out var index);
            
            var searchResponse = await _elasticClient.GetAsync<Location>(htId, request => request.Index(index), cancellationToken);
            
            return !searchResponse.IsValid 
                ? Result.Failure<Location>($"Failed to retrieve a location by htId '{htId}'") 
                : searchResponse.Source;
        }
        
        
        public async Task<List<Location>> Search(string query, CancellationToken cancellationToken = default)
        {
            _logger.LogPredictionsQuery(query);
            
            ElasticsearchHelper.TryGetIndex(_indexOptions.Indexes!, Languages.English, out var index);
            
            var locations = await SearchSuggestions(index, query, cancellationToken);

            if (locations.Count < MaxLocationCount)
                locations.AddRange(await SearchMatching(index, query, locations.Select(l => l.Id), MaxLocationCount - locations.Count, cancellationToken));

            return locations;
        }

        
        private async Task<List<Location>> SearchSuggestions(string index, string value, CancellationToken cancellationToken = default)
        {
            const string countrySuggester = "countrySuggester";
            const string localitySuggester = "localitySuggester";
            const string accommodationSuggester = "accommodationSuggester";
            
            var response = await _elasticClient.SearchAsync<Location>(search => search.Index(index).Suggest(CreateSuggestionRequests), cancellationToken);
            
            var result = GetLocations(response, countrySuggester).ToList();
            if (result.Count == MaxLocationCount) 
                return result;

            var locations = GetLocations(response, localitySuggester, result.Count);
            result.AddRange(locations);
            
            if (result.Count == MaxLocationCount) 
                return result;

            locations = GetLocations(response, accommodationSuggester, result.Count);
            result.AddRange(locations);

            return result;
            

            IPromise<ISuggestContainer> CreateSuggestionRequests(SuggestContainerDescriptor<Location> suggestContainer)
            {
                return suggestContainer
                    .Completion(countrySuggester, suggester => AddSuggester(suggester, MapperLocationTypes.Country))
                    .Completion(localitySuggester, suggester => AddSuggester(suggester, MapperLocationTypes.Locality))
                    .Completion(accommodationSuggester, suggester => AddSuggester(suggester, MapperLocationTypes.Accommodation));

                ICompletionSuggester AddSuggester(CompletionSuggesterDescriptor<Location> suggester,
                    MapperLocationTypes contextType) 
                    => suggester.Field(field => field.Suggestion)
                        .Prefix(value)
                        .Contexts(context => context.Context("type", category => category.Context(GetContextName(contextType))))
                        .Size(MaxLocationCount);
            }

            
            string GetContextName(MapperLocationTypes type) => type.ToString("G").ToLowerInvariant();

            
            IEnumerable<Location> GetLocations(ISearchResponse<Location> searchResponse, string suggester, int foundedLocationCount = 0)
            {
                if (!searchResponse.Suggest.ContainsKey(suggester))
                    return Enumerable.Empty<Location>();
                
                return searchResponse.Suggest[suggester]
                    .SelectMany(c => c.Options)
                    .Select(o => o.Source)
                    .Take(MaxLocationCount - foundedLocationCount);
            }
        }

        
        private async Task<IEnumerable<Location>> SearchMatching(string index, string value, IEnumerable<string> excludeIds, int size = 10, CancellationToken cancellationToken = default)
        {
            var response = await _elasticClient.SearchAsync<Location>(request 
                => request.Index(index).Query(query
                        => query.Bool(boolQuery => boolQuery.Must(must 
                                => must.Match(match => match.Field(location => location.PredictionText)
                                    .Query(value)
                                    .Analyzer("standard")
                                    .Operator(Operator.And)))
                            .MustNot(mustQuery => mustQuery.Ids(ids => ids.Values(excludeIds))))), cancellationToken);
            
            return response.Documents;
        }
        
        
        private const int MaxLocationCount = 10;
        private readonly IndexOptions _indexOptions;
        private readonly IElasticClient _elasticClient;
        private readonly ILogger<ILocationsService> _logger;
    }
}