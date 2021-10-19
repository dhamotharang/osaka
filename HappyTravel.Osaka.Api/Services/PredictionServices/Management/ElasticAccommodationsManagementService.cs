using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HappyTravel.MapperContracts.Public.StaticDataPublications;
using HappyTravel.Osaka.Api.Infrastructure.Logging;
using HappyTravel.Osaka.Api.Models.Elastic;
using HappyTravel.Osaka.Api.Options;
using HappyTravel.Osaka.Api.Services.PredictionServices.Management.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nest;
using Result = CSharpFunctionalExtensions.Result;

namespace HappyTravel.Osaka.Api.Services.PredictionServices.Management
{
    public class ElasticAccommodationsManagementService : BasePredictionsManagementService
    {
        public ElasticAccommodationsManagementService(IElasticClient elasticClient, 
            IOptions<IndexesOptions> indexOptions, 
            ILogger<ElasticAccommodationsManagementService> logger)
        {
            _elasticClient = elasticClient;
            _indexName = indexOptions.Value.EnglishIndexes.Accommodations;
            _logger = logger;
        }


        public async Task<Result> Add(IEnumerable<Accommodation> accommodations, CancellationToken cancellationToken = default)
        {
            var elasticAccommodations = accommodations.Select(Build).ToList();
            var response = await _elasticClient.Add(elasticAccommodations, _indexName, cancellationToken);
            
            return HandleResponse(response, errors => _logger.LogElasticAccommodationErrors(errors), numberOfAccommodations => _logger.LogElasticAccommodationAdded(numberOfAccommodations));
        }

        
        public async Task<Result> Update(IEnumerable<Accommodation> accommodations, CancellationToken cancellationToken = default)
        {
            var elasticAccommodations = accommodations.Select(Build).ToList();
            var response = await _elasticClient.Update(elasticAccommodations, _indexName, cancellationToken);
            
           return HandleResponse(response, errors => _logger.LogElasticAccommodationErrors(errors), numberOfAccommodations => _logger.LogElasticAccommodationUpdated(numberOfAccommodations));
        }
        
        
        public async Task<Result> Delete(IEnumerable<Accommodation> accommodations, CancellationToken cancellationToken = default)
        {
            var elasticAccommodations = accommodations.Select(Build).ToList();
            var response = await _elasticClient.Delete(elasticAccommodations, _indexName, cancellationToken);
            
            return HandleResponse(response, errors => _logger.LogElasticAccommodationErrors(errors), numberOfAccommodations => _logger.LogElasticAccommodationDeleted(numberOfAccommodations));
        }
        
        
        private ElasticAccommodation Build(Accommodation accommodation)
        {
            var uploadedDate = DateTime.UtcNow;
            return new()
            {
                Id = accommodation.HtId,
                Name = accommodation.Name,
                Locality = accommodation.Locality,
                Country = accommodation.Country,
                Suggestion = BuildSuggestion(accommodation),
                PredictionText = BuildPredictionText(accommodation),
                Coordinates = new GeoCoordinate(accommodation.Coordinates.Latitude, accommodation.Coordinates.Longitude),
                NumberOfAccommodationsInLocality = accommodation.NumberOfAccommodationsInLocality,
                IsPromotion = accommodation.IsPromotion,
                IsMappingConfirmed = accommodation.IsMappingConfirmed,
                IsDirectContract = accommodation.IsDirectContract,
                IsInDomesticZone = accommodation.IsInDomesticZone,
                Modified = uploadedDate
            };
        }


        private Suggestion BuildSuggestion(Accommodation accommodation) 
            => new()
            {
                Input = new List<string>
                {
                    $"{accommodation.Name}",
                    $"{accommodation.Name} {accommodation.Locality} {accommodation.Country}",
                    $"{accommodation.Name} {accommodation.Country} {accommodation.Locality}",
                    $"{accommodation.Locality} {accommodation.Name}",
                    $"{accommodation.Locality} {accommodation.Country} {accommodation.Name}",
                    $"{accommodation.Country} {accommodation.Name}",
                    $"{accommodation.Country} {accommodation.Locality} {accommodation.Name}",
                },
                Weight = accommodation.NumberOfAccommodationsInLocality
            };


        private string BuildPredictionText(Accommodation accommodation)
        {
            if (string.IsNullOrEmpty(accommodation.Locality))
                return $"{accommodation.Name}, {accommodation.Country}";             
            
            return $"{accommodation.Name}, {accommodation.Locality}, {accommodation.Country}";
        }
        

        private readonly IElasticClient _elasticClient;
        private readonly string _indexName;
        private readonly ILogger<ElasticAccommodationsManagementService> _logger;
    }
}