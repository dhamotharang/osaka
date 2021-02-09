﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.MultiLanguage;
using HappyTravel.PredictionService.Infrastructure;
using HappyTravel.PredictionService.Infrastructure.Logging;
using HappyTravel.PredictionService.Models;
using HappyTravel.PredictionService.Services.HttpClients;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nest;
using IndexOptions = HappyTravel.PredictionService.Options.IndexOptions;
using Result = CSharpFunctionalExtensions.Result;

namespace HappyTravel.PredictionService.Services.Locations
{
    public class LocationsManagementService : ILocationsManagementService
    {
        public LocationsManagementService(IElasticClient elasticClient, IMapperHttpClient mapperHttpClient, IOptions<IndexOptions> indexOptions, ILogger<LocationsManagementService> logger)
        {
            _elasticClient = elasticClient;
            _logger = logger;
            _indexOptions = indexOptions.Value;
            _mapperHttpClient = mapperHttpClient;
        }

        
        public async Task<Result<int>> ReUpload(CancellationToken cancellationToken = default)
        {
            _logger.LogStartUploadingLocations("Start locations upload");
            
            var languageCode = LanguagesHelper.GetLanguageCode(Languages.English).ToLowerInvariant();
            if (!ElasticsearchHelper.TryGetIndex(_indexOptions.Indexes!, languageCode, out var index))
            {
                var error = $"Index with the language '{languageCode}' doesn't exist";
                _logger.LogUploadingError(error);
                return Result.Failure<int>(error);
            }

            _logger.LogRemoveLocationsFromIndex($"Remove all locations from the Elasticsearch index '{index}'");
            
            var (_, removeFailure, removeError) = await RemoveAllFromIndex(index!, cancellationToken);
            if (removeFailure)
            {
                _logger.LogUploadingError(removeError);
                return Result.Failure<int>(removeError);
            }

            var locationsUploaded = 0;
            
            foreach (var locationType in Enum.GetValues<MapperLocationTypes>())
            {
                const int batchSize = 20000;
                await foreach (var (_, isFailure, locations, error) in GetFromMapper(locationType, languageCode, batchSize, cancellationToken))
                {
                    if (isFailure)
                    {
                        _logger.LogUploadingError(error);
                        return Result.Failure<int>(error);
                    }

                    _logger.LogLocationsReceivedFromMapper($"'{locations.Count}' locations received from the mapper ");
                    
                    if (!locations.Any())
                        continue;
                    
                    var (_, uploadFailure, uploadError) = await UploadToElasticsearch(index, locations, cancellationToken);
                    if (uploadFailure)
                    {
                        _logger.LogUploadingError(error);
                        return Result.Failure<int>(uploadError);
                    }

                    _logger.LogLocationsUploadedToIndex($"'{locations.Count}' locations uploaded to the the Elasticsearch index '{index}'");
                    
                    locationsUploaded += locations.Count;
                }
            }

            _logger.LogCompleteUploadingLocations($"Uploading to the Elasticsearch index '{index}' has been completed. The total number of uploaded locations is '{locationsUploaded}'");
            
            return Result.Success(locationsUploaded);
        }

        
        private async IAsyncEnumerable<Result<List<Location>>> GetFromMapper(MapperLocationTypes locationType, string languageCode, int batchSize, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            var skip = 0;
            List<Location> locations;
            do
            {
                cancellationToken.ThrowIfCancellationRequested();
                bool isFailure;
                string error;
                (_, isFailure, locations, error) = await _mapperHttpClient.GetLocations(locationType, languageCode, default, skip, batchSize, cancellationToken);
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

            if (!response.IsValid) 
                return Result.Failure(response.ToString());
            
            return Result.Success();
        }


        private async Task<Result> RemoveAllFromIndex(string index, CancellationToken cancellationToken = default)
        {
            var response = await _elasticClient.DeleteByQueryAsync<Models.Elasticsearch.Location>(request => request.Index(index).MatchAll(), cancellationToken);
            
            if (response.OriginalException is not null)
                throw response.OriginalException;
            
            return !response.IsValid
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
            var result = location.LocationType == MapperLocationTypes.Accommodation
                ? location.Name
                : string.Empty;
            
            if (!string.IsNullOrEmpty(location.Locality))
                result += string.IsNullOrEmpty(result) ? location.Locality : ", " + location.Locality;
                
            if (!string.IsNullOrEmpty(location.Country))
                result += string.IsNullOrEmpty(result)? location.Country : ", " + location.Country;

            return result;
        }
        

        private readonly IMapperHttpClient _mapperHttpClient;
        private readonly IElasticClient _elasticClient;
        private readonly IndexOptions _indexOptions;
        private readonly ILogger<LocationsManagementService> _logger;
    }
}