﻿using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HappyTravel.MapperContracts.Internal.Mappings.Enums;
using HappyTravel.MultiLanguage;
using HappyTravel.Osaka.Api.Infrastructure;
using HappyTravel.Osaka.Api.Infrastructure.Logging;
using HappyTravel.Osaka.Api.Models.Elastic;
using HappyTravel.Osaka.Api.Models.Response;
using HappyTravel.Osaka.Api.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nest;

namespace HappyTravel.Osaka.Api.Services.PredictionServices
{
    public class PredictionsService : IPredictionsService
    {
        public PredictionsService(IElasticClient elasticClient, IOptions<IndexesOptions> indexOptions, ILogger<IPredictionsService> logger)
        {
            _indexesOptions = indexOptions.Value;
            _elasticClient = elasticClient;
            _logger = logger;
        }
        
        
        public async Task<List<Prediction>> Search(string query, CancellationToken cancellationToken = default)
        {
            _logger.LogPredictionsQuery(query);
            
            ElasticHelper.TryGetIndex(_indexesOptions.Indexes!, Languages.English, out var index);
            
            var locations = await SearchSuggestions(index, query, cancellationToken);

            if (locations.Count < MaxLocationCount)
                locations.AddRange(await SearchMatching(index, query, locations.Select(l => l.Id), MaxLocationCount - locations.Count, cancellationToken));

            return locations.Select(Build).ToList();
        }

        
        private async Task<List<ElasticLocation>> SearchSuggestions(string index, string value, CancellationToken cancellationToken = default)
        {
            const string countrySuggester = "countrySuggester";
            const string localitySuggester = "localitySuggester";
            const string accommodationSuggester = "accommodationSuggester";
            
            var response = await _elasticClient.SearchAsync<ElasticLocation>(search => search.Index(index).Suggest(CreateSuggestionRequests), cancellationToken);
            
            var result = GetLocations(response, localitySuggester).ToList();
            if (result.Count == MaxLocationCount)
                return result;

            var locations = GetLocations(response, accommodationSuggester, result.Count);
            result.AddRange(locations);
            
            if (result.Count == MaxLocationCount)
                return result;

            locations = GetLocations(response, countrySuggester, result.Count);
            result.AddRange(locations);

            return result;
            

            IPromise<ISuggestContainer> CreateSuggestionRequests(SuggestContainerDescriptor<ElasticLocation> suggestContainer)
            {
                return suggestContainer
                    .Completion(countrySuggester, suggester => AddSuggester(suggester, MapperLocationTypes.Country))
                    .Completion(localitySuggester, suggester => AddSuggester(suggester, MapperLocationTypes.Locality))
                    .Completion(accommodationSuggester, AddAccommodationSuggester);

                ICompletionSuggester AddSuggester(CompletionSuggesterDescriptor<ElasticLocation> suggester,
                    MapperLocationTypes locationType) 
                    => suggester.Field(field => field.Suggestion)
                        .Prefix(value)
                        .Contexts(queriesDescriptor => queriesDescriptor.Context(AccommodationContextCategoryNames.LocationType, category
                            => category.Context(GetContextName(locationType))))
                        .Size(MaxLocationCount)
                        .SkipDuplicates();

                ICompletionSuggester AddAccommodationSuggester(CompletionSuggesterDescriptor<ElasticLocation> suggester)
                    => suggester.Field(field => field.Suggestion)
                        .Prefix(value)
                        .Contexts(queriesDescriptor => queriesDescriptor
                                .Context(AccommodationContextCategoryNames.LocationType, category 
                                    => category.Context(GetContextName(MapperLocationTypes.Accommodation)))
                                .Context(AccommodationContextCategoryNames.LocationType, category 
                                    => category.Context(GetContextName(MapperLocationTypes.Accommodation))))
                        
                           /* category => category.Context(ElasticContextCategoryNames.IsPromotion).Boost(5),
                            category => category.Context(ElasticContextCategoryNames.IsDirectContract).Boost(4),
                            category => category.Context(ElasticContextCategoryNames.IsConfirmed).Boost(3),
                            category => category.Context($"{nameof(ElasticLocation.IsInDomesticZone)}").Boost(2)).Context()*/
                        .Size(MaxLocationCount)
                        .SkipDuplicates();
            }

            
            string GetContextName(MapperLocationTypes type) => type.ToString("G").ToLowerInvariant();


            IEnumerable<ElasticLocation> GetLocations(ISearchResponse<ElasticLocation> searchResponse, string suggester, int foundedLocationCount = 0)
            {
                if (!searchResponse.Suggest.ContainsKey(suggester))
                    return Enumerable.Empty<ElasticLocation>();
                
                return searchResponse.Suggest[suggester]
                    .SelectMany(c => c.Options)
                    .Select(o => o.Source)
                    .Take(MaxLocationCount - foundedLocationCount);
            }
        }

        
        private async Task<IEnumerable<ElasticLocation>> SearchMatching(string index, string value, IEnumerable<string> excludeIds, int size = 10, CancellationToken cancellationToken = default)
        {
            var response = await _elasticClient.SearchAsync<ElasticLocation>(request 
                => request.Index(index).Query(query
                        => query.Bool(boolQuery => boolQuery.Must(must 
                                => must.Match(match => match.Field(location => location.PredictionText)
                                    .Query(value)
                                    .Analyzer("standard")
                                    .Operator(Operator.And)))
                            .MustNot(mustQuery => mustQuery.Ids(ids => ids.Values(excludeIds))))), cancellationToken);
            
            return response.Documents;
        }
        
        
        private Prediction Build(ElasticLocation location) => new(location.Id, location.PredictionText, string.Empty);

        private Dictionary<string, double> AccommodationBoostValues = new()
        {
            {AccommodationContextCategoryNames }
        };
        
        private const int MaxLocationCount = 10;
        private readonly IndexesOptions _indexesOptions;
        private readonly IElasticClient _elasticClient;
        private readonly ILogger<IPredictionsService> _logger;
    }
}