using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HappyTravel.LocationService.Infrastructure;
using HappyTravel.LocationService.Models.Requests;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nest;
using IndexOptions = HappyTravel.LocationService.Options.IndexOptions;
using Result = CSharpFunctionalExtensions.Result;

namespace HappyTravel.LocationService.Services
{
    public class LocationManagementService : ILocationManagementService
    {
        public LocationManagementService(IElasticClient elasticClient, IOptions<IndexOptions> indexOptions, ILogger<ILocationManagementService> logger)
        {
            _elasticClient = elasticClient;
            _logger = logger;
            _indexOptions = indexOptions.Value;
        }
        
        public async Task<Result> UploadLocations(List<Location> locations, string languageCode, CancellationToken cancellationToken = default)
        {
            if (!ElasticsearchHelper.TryGetIndex(_indexOptions.Indexes, languageCode, out var index))
                return Result.Failure($"Index with the language '{languageCode}' doesn't exist");

            var elasticsearchLocations = BuildElasticsearchLocations(locations);
            
            var response = await _elasticClient.IndexManyAsync(elasticsearchLocations, index, cancellationToken);

            if (!response.IsValid)
                return Result.Failure("Failed to upload locations: " + string.Join(", ",
                    response.ItemsWithErrors.Select(item =>$"id '{item.Id}' " + item.Error.RootCause)));
            
            return Result.Success();
        }

        
        private List<Models.Elasticsearch.Location> BuildElasticsearchLocations(List<Location> locations)
            => locations.Select(BuildElasticsearchLocation).ToList();


        private Models.Elasticsearch.Location BuildElasticsearchLocation(Location location)
        {
            return new()
            {
                Id = location.Id,
                Name = location.Name,
                Locality = location.Locality,
                Country = location.Country,
                CountryCode = location.CountryCode,
                PredictionText = CreatePredictionText(location.Name, location.Locality, location.Country),
                Coordinates = new GeoCoordinate(location.Coordinates.Latitude, location.Coordinates.Longitude),
                DistanceInMeters = location.DistanceInMeters,
                Source = location.Source,
                Type = location.Type,
                Suppliers = location.Suppliers
            };
        }
        
        private string CreatePredictionText(string country, string locality, string name)
        {
            var predictionText = string.Empty;

            if (!string.IsNullOrEmpty(name))
                predictionText = name;

            if (!string.IsNullOrEmpty(locality))
                predictionText += $", {locality}";

            if (!string.IsNullOrEmpty(country))
                predictionText += $", {country}";

            return predictionText;
        }
        
        public async Task<Result> RemoveLocations(List<string> ids, string languageCode, CancellationToken cancellationToken = default)
        {
            if (!ElasticsearchHelper.TryGetIndex(_indexOptions.Indexes, languageCode, out var index))
                return Result.Failure($"Index with the language '{languageCode}' doesn't exist");

            var response = await _elasticClient.DeleteManyAsync(ids.Select(id=>new Location{Id = id}), index, cancellationToken);
            
            if (!response.IsValid)
                return Result.Failure("Failed to delete locations: " + string.Join(", ", 
                    response.ItemsWithErrors.Select(item =>$"id '{item.Id}' " + item.Error.RootCause)));

            return Result.Success();
        }
        

        private readonly IElasticClient _elasticClient;
        private readonly IndexOptions _indexOptions;
        private readonly ILogger<ILocationManagementService> _logger;
    }
}