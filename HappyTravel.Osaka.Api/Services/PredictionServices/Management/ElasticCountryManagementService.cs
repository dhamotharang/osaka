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
    public class ElasticCountryManagementService : BasePredictionsManagementService
    {
        public ElasticCountryManagementService(IElasticClient elasticClient, IOptions<IndexesOptions> indexOptions,
            ILogger<ElasticCountryManagementService> logger)
        {
            _elasticClient = elasticClient;
            _indexName = indexOptions.Value.EnglishIndexes.Countries;
            _logger = logger;
        }

        
        public async Task<Result> Add(IEnumerable<Country> countries, CancellationToken cancellationToken = default)
        {
            var elasticCountries = countries.Select(Build).ToList();
            var response = await _elasticClient.Add(elasticCountries, _indexName, cancellationToken);

            return HandleResponse(response, errors => _logger.LogElasticCountryErrors(errors),
                numberOfCountries => _logger.LogElasticCountryAdded(numberOfCountries));
        }

        
        public async Task<Result> Update(IEnumerable<Country> countries, CancellationToken cancellationToken = default)
        {
            var elasticAccommodations = countries.Select(Build).ToList();
            var response = await _elasticClient.Update(elasticAccommodations, _indexName, cancellationToken);

            return HandleResponse(response, errors => _logger.LogElasticCountryErrors(errors),
                numberOfCountries => _logger.LogElasticCountryUpdated(numberOfCountries));
        }

        
        public async Task<Result> Delete(IEnumerable<Country> countries, CancellationToken cancellationToken = default)
        {
            var elasticAccommodations = countries.Select(Build).ToList();
            var response = await _elasticClient.Delete(elasticAccommodations, _indexName, cancellationToken);

            return HandleResponse(response, errors => _logger.LogElasticCountryErrors(errors),
                numberOfCountries => _logger.LogElasticCountryDeleted(numberOfCountries));
        }

        
        private ElasticCountry Build(Country country)
        {
            var uploadedDate = DateTime.UtcNow;

            return new()
            {
                Id = country.HtId,
                Name = country.Name,
                Code = country.Code,
                Suggestion = BuildSuggestion(country),
                PredictionText = BuildPredictionText(country),
                NumberOfAccommodations = country.NumberOfAccommodations,
                Modified = uploadedDate
            };
        }

        
        private Suggestion BuildSuggestion(Country country) 
            => new()
            {
                Input = new List<string> {country.Name}, 
                Weight = country.NumberOfAccommodations
            };

        
        private string BuildPredictionText(Country country) 
            => country.Name;

        
        private readonly IElasticClient _elasticClient;
        private readonly string _indexName;
        private readonly ILogger<ElasticCountryManagementService> _logger;
    }
}