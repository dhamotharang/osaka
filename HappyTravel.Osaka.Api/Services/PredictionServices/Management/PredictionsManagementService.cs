using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.MapperContracts.Internal.Mappings.Enums;
using HappyTravel.MultiLanguage;
using HappyTravel.Osaka.Api.Infrastructure;
using HappyTravel.Osaka.Api.Infrastructure.Logging;
using HappyTravel.Osaka.Api.Models.Elastic;
using HappyTravel.Osaka.Api.Options;
using HappyTravel.Osaka.Api.Services.HttpClients;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nest;
using Result = CSharpFunctionalExtensions.Result;

namespace HappyTravel.Osaka.Api.Services.PredictionServices.Management
{
    public class PredictionsManagementService
    {
        public PredictionsManagementService(IElasticClient elasticClient, IMapperHttpClient mapperHttpClient, IOptions<IndexesOptions> indexOptions, ILogger<PredictionsManagementService> logger)
        {
            _elasticClient = elasticClient;
            _logger = logger;
            _indexesOptions = indexOptions.Value;
            _mapperHttpClient = mapperHttpClient;
        }

        /*
        public async Task<Result<int>> ReuploadAllPredictionsFromMapper(CancellationToken cancellationToken = default)
        {
            _logger.LogStartUploadingLocations("Start locations upload");

            var languageCode = LanguagesHelper.GetLanguageCode(Languages.English).ToLowerInvariant();
            if (!ElasticHelper.TryGetIndex(_indexesOptions.Indexes!, languageCode, out var index))
            {
                var error = $"Index with the language '{languageCode}' doesn't exist";
                _logger.LogUploadingError(error);
                return Result.Failure<int>(error);
            }

            _logger.LogRemoveLocationsFromIndex($"Remove all locations from the Elasticsearch index '{index}'");
            
            var (_, removeFailure, removeError) = await ReCreateIndex(index!, cancellationToken);
            if (removeFailure)
            {
                _logger.LogUploadingError(removeError);
                return Result.Failure<int>(removeError);
            }

            var locationsUploaded = 0;
            
            foreach (var locationType in Enum.GetValues<MapperLocationTypes>().Where(t => t != MapperLocationTypes.Undefined))
            {
                const int batchSize = 2000;
                await foreach (var (_, isFailure, locations, error) in GetFromMapper(locationType, languageCode, batchSize, cancellationToken))
                {
                    if (isFailure)
                        _logger.LogUploadingError(error);

                    _logger.LogGetLocationsFromMapper($"'{locations.Count}' locations received from the mapper");

                    bool FilterLocations(LocationDetailedInfo l) => l.NumberOfAccommodations > 0;

                    if (!locations.Any())
                        continue;
                    
                    var (_, uploadFailure, uploadError) = await Add(locations.Where(FilterLocations).ToList(), index, cancellationToken);
                    if (uploadFailure)
                        _logger.LogUploadingError(uploadError);
                    
                    locationsUploaded += locations.Count;
                }
            }

            _logger.LogCompleteUploadingLocations($"Uploading to the Elasticsearch index '{index}' has been completed. The total number of uploaded locations is '{locationsUploaded}'");
            
            return Result.Success(locationsUploaded);
        }
        private void LogErrorsIfNeeded(BulkResponse response)
        {
            if (!response.Errors) 
                return;
            
            var sb = new StringBuilder();
            foreach (var itemWithError in response.ItemsWithErrors)
                sb.AppendLine($"Failed to index location {itemWithError.Id}: {itemWithError.Error}");
            
            _logger.LogUploadingError($"{sb} {nameof(response.DebugInformation)}: {response.DebugInformation}");
        }
        
        
        private async IAsyncEnumerable<Result<List<LocationDetailedInfo>>> GetFromMapper(MapperLocationTypes locationType, string languageCode, int batchSize, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            var skip = 0;
            List<LocationDetailedInfo> locations;
            do
            {
                cancellationToken.ThrowIfCancellationRequested();
                bool isFailure;
                string error;
                (_, isFailure, locations, error) = await _mapperHttpClient.GetLocations(locationType, languageCode, default, skip, batchSize, cancellationToken);
                if (isFailure)
                    yield return Result.Failure<List<LocationDetailedInfo>>(error);
                
                yield return locations;
                
                skip += batchSize;
            } while (locations.Count == batchSize);
        }
        
       
        private async Task<Result> ReCreateIndex(string index, CancellationToken cancellationToken = default)
        {
            await _elasticClient.Indices.DeleteAsync(index, ct: cancellationToken);
            var response = await _elasticClient.CreateIndexes(_indexesOptions.Indexes);
            
            if (response.OriginalException is not null)
                throw response.OriginalException;
            
            return !response.IsValid
                ? Result.Failure(response.ToString()) 
                : Result.Success();
        }
        
        */
        private readonly IMapperHttpClient _mapperHttpClient;
        private readonly IElasticClient _elasticClient;
        private readonly IndexesOptions _indexesOptions;
        private readonly ILogger<PredictionsManagementService> _logger;
    }
}