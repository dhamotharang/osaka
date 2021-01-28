using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.LocationService.Infrastructure;
using HappyTravel.LocationService.Models;
using HappyTravel.LocationService.Services.HttpClients;
using HappyTravel.MultiLanguage;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nest;
using IndexOptions = HappyTravel.LocationService.Options.IndexOptions;
using Result = CSharpFunctionalExtensions.Result;

namespace HappyTravel.LocationService.Services.Locations.Mapper
{
    public class MapperLocationsManagementService : IMapperLocationsManagementService
    {
        public MapperLocationsManagementService(IElasticClient elasticClient, IAccommodationMapperHttpClient accommodationMapperHttpClient, IOptions<IndexOptions> indexOptions, ILogger<IMapperLocationsManagementService> logger)
        {
            _elasticClient = elasticClient;
            _logger = logger;
            _indexOptions = indexOptions.Value;
            _accommodationMapperHttpClient = accommodationMapperHttpClient;
        }

        
        public async Task<Result<int>> ReUpload(CancellationToken cancellationToken = default)
        {
            var languageCode = LanguagesHelper.GetLanguageCode(Languages.English).ToLowerInvariant();
            if (!ElasticsearchHelper.TryGetIndex(_indexOptions.Indexes!, languageCode, out var index))
                return Result.Failure<int>($"Index with the language '{languageCode}' doesn't exist");

            var (_, removeFailure, removeError) = await RemoveAllFromIndex(index!, cancellationToken);
            if (removeFailure)
                return Result.Failure<int>(removeError);
            
            var locationsUploaded = 0;
            
            foreach (var locationType in Enum.GetValues<AccommodationMapperLocationTypes>())
            {
                const int batchSize = 50000;
                await foreach (var (_, isFailure, locations, error) in GetFromMapper(locationType, languageCode, batchSize, cancellationToken))
                {
                    if (!locations.Any())
                        continue;
                    if (isFailure)
                        return Result.Failure<int>(error);
                    var (_, uploadFailure, uploadError) = await UploadToElasticsearch(index, locations, cancellationToken);
                    if (uploadFailure)
                        return Result.Failure<int>(uploadError);
                    locationsUploaded += locations.Count;
                }
            }
            
            return Result.Success(locationsUploaded);
        }

        
        private async IAsyncEnumerable<Result<List<Location>>> GetFromMapper(AccommodationMapperLocationTypes locationType, string languageCode, int batchSize, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            var skip = 0;
            List<Location> locations;
            do
            {
                cancellationToken.ThrowIfCancellationRequested();
                bool isFailure;
                string error;
                (_, isFailure, locations, error) = await _accommodationMapperHttpClient.GetLocations(locationType, languageCode, default, skip, batchSize, cancellationToken);
                if (isFailure)
                    yield return Result.Failure<List<Location>>(error);
                
                yield return locations;
                
                skip += batchSize;
            } while (locations.Count == batchSize);
        }
        

        private async Task<Result> UploadToElasticsearch(string index, List<Location> locations, CancellationToken cancellationToken = default)
        {
            var elasticsearchLocations = Build(locations);
            
            var response = await _elasticClient.IndexManyAsync(elasticsearchLocations, index, cancellationToken);

            if (!response.IsValid || response.ServerError != null) 
                return Result.Failure(response.ToString());
            
            return Result.Success();
        }


        private async Task<Result> RemoveAllFromIndex(string index, CancellationToken cancellationToken = default)
        {
            var response = await _elasticClient.DeleteByQueryAsync<Models.Elasticsearch.Location>(request => request.Index(index).MatchAll(), cancellationToken);
            
            return !response.IsValid || response.ServerError != null 
                ? Result.Failure(response.ToString()) 
                : Result.Success();
        }
        
        
        private List<Models.Elasticsearch.Location> Build(List<Location> locations)
            => locations.Select(Build).ToList();


        private Models.Elasticsearch.Location Build(Location location)
        {
            var uploadedDate = DateTime.UtcNow;
            return new()
            {
                Id = location.HtId,
                HtId = location.HtId,
                Name = location.Name,
                Locality = location.Locality,
                Country = location.Country,
                CountryCode = location.CountryCode,
                PredictionText = BuildPredictionText(location),
                Coordinates = new GeoCoordinate(location.Coordinates.Latitude, location.Coordinates.Longitude),
                DistanceInMeters = location.DistanceInMeters,
                LocationType = location.LocationType,
                Type = location.Type,
                Modified = uploadedDate
            };
        }
        
        
        private static string BuildPredictionText(Location location)
        {
            var result = location.LocationType == AccommodationMapperLocationTypes.Accommodation
                ? location.Name
                : string.Empty;
            
            if (!string.IsNullOrEmpty(location.Locality))
                result += string.IsNullOrEmpty(result) ? location.Locality : ", " + location.Locality;
                
            if (!string.IsNullOrEmpty(location.Country))
                result += string.IsNullOrEmpty(result)? location.Country : ", " + location.Country;

            return result;
        }
        

        private readonly IAccommodationMapperHttpClient _accommodationMapperHttpClient;
        private readonly IElasticClient _elasticClient;
        private readonly IndexOptions _indexOptions;
        private readonly ILogger<IMapperLocationsManagementService> _logger;
    }
}