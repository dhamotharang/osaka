using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.LocationService.Infrastructure;
using HappyTravel.LocationService.Models;
using HappyTravel.MultiLanguage;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nest;
using IndexOptions = HappyTravel.LocationService.Options.IndexOptions;
using Location = HappyTravel.LocationService.Models.Elasticsearch.Location;
using Result = CSharpFunctionalExtensions.Result;

namespace HappyTravel.LocationService.Services.Locations.Mapper
{
    public class MapperLocationsService : ILocationsService
    {
        public MapperLocationsService(IElasticClient elasticClient, IOptions<IndexOptions> indexOptions, ILogger<ILocationsService> logger)
        {
            _indexOptions = indexOptions.Value;
            _elasticClient = elasticClient;
            _logger = logger;
        }


        public async Task<Result<List<Location>>> Search(string query, int skip = 0, int top = 10, CancellationToken cancellationToken = default)
        {
            ElasticsearchHelper.TryGetIndex(_indexOptions.Indexes, Languages.English, out var index);
           
            var multiSearchResponse = await _elasticClient.MultiSearchAsync(index,
                request => request
                    .Search<Location>(AccommodationMapperLocationTypes.Country.ToString(), CreateSearchCountryRequest)
                    .Search<Location>(AccommodationMapperLocationTypes.Locality.ToString(), CreateSearchLocalityRequest)
                    .Search<Location>(AccommodationMapperLocationTypes.Accommodation.ToString(), CreateSearchRequest), cancellationToken);

            if (!multiSearchResponse.IsValid || multiSearchResponse.ServerError != null)
                return Result.Failure<List<Location>>(multiSearchResponse.ToString());
            
            return ProcessResponse();
            
            
            ISearchRequest CreateSearchCountryRequest(SearchDescriptor<Location> search) 
                => search.Query(searchQuery => searchQuery.Bool(boolQuery 
                    => boolQuery.Must(mustQuery => mustQuery.Match(matchQuery => matchQuery.Field(location => location.Country).Query(query).Operator(Operator.And)), 
                        mustQuery => mustQuery.Term(termQuery => termQuery.Field(location => location.LocationType).Value(AccommodationMapperLocationTypes.Country))))).From(0).Size(MaxLocationsNumber);
            
            
            ISearchRequest CreateSearchLocalityRequest(SearchDescriptor<Location> search) 
                => search.Query(searchQuery => searchQuery.Bool(boolQuery 
                    => boolQuery.Must(mustQuery => mustQuery.Match(matchQuery => matchQuery.Field(location => location.PredictionText).Query(query).Operator(Operator.And)), 
                        mustQuery => mustQuery.Term(termQuery => termQuery.Field(location => location.LocationType).Value(AccommodationMapperLocationTypes.Locality))))).From(0).Size(MaxLocationsNumber);
            
            
            ISearchRequest CreateSearchRequest(SearchDescriptor<Location> search) 
                => search.Query(searchQuery => searchQuery.Bool(boolQuery 
                    => boolQuery.Must(mustQuery => mustQuery.Match(matchQuery => matchQuery.Field(location => location.PredictionText).Query(query).Operator(Operator.And))))).From(0).Size(MaxLocationsNumber);

            
            List<Location> ProcessResponse() =>
                multiSearchResponse.GetResponses<Location>()
                    .SelectMany(searchResponse => searchResponse.Documents).Skip(skip).Take(top)
                    .GroupBy(location => location.Id).Select(group => group.First()).ToList();
        }

        
        public async Task<Result<Location>> Get(string htId, CancellationToken cancellationToken = default)
        {
            ElasticsearchHelper.TryGetIndex(_indexOptions.Indexes, Languages.English, out var index);
            
            var searchResponse = await _elasticClient.GetAsync<Location>(htId, request => request.Index(index), cancellationToken);
            
            return !searchResponse.IsValid 
                ? Result.Failure<Location>($"Failed to retrieve a location by htId '{htId}'") 
                : searchResponse.Source;
        }

        
        private static readonly List<Location> EmptyLocations = new(0);
        private readonly IndexOptions _indexOptions;
        private readonly IElasticClient _elasticClient;
        private readonly ILogger<ILocationsService> _logger;

        private const int MaxLocationsNumber = 10;
    }
}