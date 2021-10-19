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
    public class ElasticLocalityManagementService : BasePredictionsManagementService
    {
        public ElasticLocalityManagementService(IElasticClient elasticClient, 
            IOptions<IndexesOptions> indexOptions, 
            ILogger<ElasticLocalityManagementService> logger)
        {
            _elasticClient = elasticClient;
            _indexName = indexOptions.Value.EnglishIndexes.Countries;
            _logger = logger;
        }
        
        
        public async Task<Result> Add(IEnumerable<Locality> localities, CancellationToken cancellationToken = default)
        {
            var elasticLocalities = localities.Select(Build).ToList();
            var response = await _elasticClient.Add(elasticLocalities, _indexName, cancellationToken);

            return HandleResponse(response, errors => _logger.LogElasticLocalityErrors(errors),
                numberOfLocalities => _logger.LogElasticLocalityAdded(numberOfLocalities));
        }

        
        public async Task<Result> Update(IEnumerable<Locality> localities, CancellationToken cancellationToken = default)
        {
            var elasticLocalities = localities.Select(Build).ToList();
            var response = await _elasticClient.Update(elasticLocalities, _indexName, cancellationToken);

            return HandleResponse(response, errors => _logger.LogElasticLocalityErrors(errors),
                numberOfLocalities => _logger.LogElasticLocalityUpdated(numberOfLocalities));
        }

        
        public async Task<Result> Delete(IEnumerable<Locality> localities, CancellationToken cancellationToken = default)
        {
            var elasticLocalities = localities.Select(Build).ToList();
            var response = await _elasticClient.Delete(elasticLocalities, _indexName, cancellationToken);

            return HandleResponse(response, errors => _logger.LogElasticLocalityErrors(errors),
                numberOfLocalities => _logger.LogElasticLocalityDeleted(numberOfLocalities));
        }
        
        
        private ElasticLocality Build(Locality locality)
        {
            var uploadedDate = DateTime.UtcNow;
            return new()
            {
                Id = locality.HtId,
                Name = locality.Name,
                Country = locality.Country,
                Suggestion = BuildSuggestion(locality),
                PredictionText = BuildPredictionText(locality),
                NumberOfAccommodations = locality.NumberOfAccommodations,
                Modified = uploadedDate
            };
        }
        
        
        private Suggestion BuildSuggestion(Locality locality) 
            => new()
            {
                Input = new List<string>
                {
                    $"{locality.Name} {locality.Country}",
                    $"{locality.Country} {locality.Name}"
                }, 
                Weight = locality.NumberOfAccommodations
            };


        private string BuildPredictionText(Locality locality)
            => $"{locality.Name}, {locality.Country}";
        
        
        private readonly string _indexName;
        private readonly IElasticClient _elasticClient;
        private readonly ILogger<ElasticLocalityManagementService> _logger;
    }
}