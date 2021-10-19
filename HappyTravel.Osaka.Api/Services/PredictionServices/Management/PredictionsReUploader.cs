using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.MapperContracts.Public.StaticDataPublications;
using HappyTravel.Osaka.Api.Infrastructure.Extensions;
using HappyTravel.Osaka.Api.Options;
using HappyTravel.Osaka.Api.Services.HttpClients;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nest;
using Result = CSharpFunctionalExtensions.Result;

namespace HappyTravel.Osaka.Api.Services.PredictionServices.Management
{
    public class PredictionsReUploader
    {
        public PredictionsReUploader(IElasticClient elasticClient,
            ElasticAccommodationsManagementService accommodationsManagementService,
            ElasticCountryManagementService countryManagementService,
            ElasticLocalityManagementService localityManagementService,
            IMapperHttpClient mapperHttpClient, 
            IOptions<IndexesOptions> indexOptions, 
            ILogger<PredictionsManagementService> logger)
        {
            _elasticClient = elasticClient;
            _logger = logger;
            _countriesIndex = indexOptions.Value.EnglishIndexes.Countries;
            _localitiesIndex = indexOptions.Value.EnglishIndexes.Localities;
            _accommodationsIndex = indexOptions.Value.EnglishIndexes.Accommodations;
            _mapperHttpClient = mapperHttpClient;
            _countryManagementService = countryManagementService;
            _localityManagementService = localityManagementService;
            _accommodationsManagementService = accommodationsManagementService;
        }

        public async Task<Result> ReUpload(CancellationToken cancellationToken = default)
        {
            var reUploadCountriesResult = await ReUploadCountries(cancellationToken);
            var reUploadLocalitiesResult = await ReUploadLocalities(cancellationToken);
            var reUploadAccommodationsResult = await ReUploadAccommodations(cancellationToken);

            return Result.Combine(reUploadCountriesResult, reUploadLocalitiesResult, reUploadAccommodationsResult);
        }
        

        public async Task<Result> ReUploadCountries(CancellationToken cancellationToken = default)
        {
            await DeleteIndex(_countriesIndex, cancellationToken);
            await CreateCountriesIndex();
            
            return await GetAndUploadData(DownloadCountries, _countryManagementService.Add, cancellationToken);
        }


        public async Task<Result> ReUploadLocalities(CancellationToken cancellationToken = default)
        {
            await DeleteIndex(_localitiesIndex, cancellationToken);
            await CreateLocalitiesIndex();
            
            return await GetAndUploadData(DownloadLocalities, _localityManagementService.Add, cancellationToken);
        }
        
        
        public async Task<Result> ReUploadAccommodations(CancellationToken cancellationToken = default)
        {
            await DeleteIndex(_accommodationsIndex, cancellationToken);
            await CreateAccommodationsIndex();
            
            return await GetAndUploadData(DownloadAccommodations, _accommodationsManagementService.Add, cancellationToken);
        }
        
        
        private Task DeleteIndex(string indexName, CancellationToken cancellationToken = default)
            => _elasticClient.Indices.DeleteAsync(indexName, ct: cancellationToken);


        private Task CreateCountriesIndex()
            => _elasticClient.CreateCountriesIndex(_countriesIndex);


        private Task CreateLocalitiesIndex()
            => _elasticClient.CreateLocalitiesIndex(_localitiesIndex);


        private Task CreateAccommodationsIndex()
            => _elasticClient.CreateAccommodationsIndex(_accommodationsIndex);

        
        private async Task<Result> GetAndUploadData<T>(Func<CancellationToken, IAsyncEnumerable<Result<List<T>>>> downloadFunc, Func<IEnumerable<T>, CancellationToken, Task<Result>> uploadFunc, CancellationToken cancellationToken = default)
        {
            await foreach (var (_, isDownloadFailure, items, downloadError) in downloadFunc(cancellationToken))
            {
                if (isDownloadFailure)
                    return Result.Failure(downloadError);

                var (_, isUploadFailure, uploadError) = await uploadFunc(items, cancellationToken);
                if (isUploadFailure)
                    return Result.Failure(uploadError);
            }

            return Result.Success();
        }
        
        
        private IAsyncEnumerable<Result<List<Locality>>> DownloadLocalities(CancellationToken cancellationToken = default)
            => Download(BatchSize, (skip, batchSize, ct) => _mapperHttpClient.GetLocalities(default, skip, batchSize, ct), cancellationToken);
        
        
        private IAsyncEnumerable<Result<List<Country>>> DownloadCountries(CancellationToken cancellationToken = default)
            => Download(BatchSize, (skip, batchSize, ct) => _mapperHttpClient.GetCountries(default, skip, batchSize, ct), cancellationToken);
        
        
        private IAsyncEnumerable<Result<List<Accommodation>>> DownloadAccommodations(CancellationToken cancellationToken = default)
            => Download(BatchSize, (skip, batchSize, ct) => _mapperHttpClient.GetAccommodations(default, skip, batchSize, ct), cancellationToken);


        private async IAsyncEnumerable<Result<List<T>>> Download<T>(int batchSize, Func<int, int, CancellationToken, Task<Result<List<T>>>> funcToDownload, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            var skip = 0;
            List<T> items;
            do
            {
                cancellationToken.ThrowIfCancellationRequested();
                bool isFailure;
                string error;
                (_, isFailure, items, error) = await funcToDownload(skip, batchSize, cancellationToken);
                if (isFailure)
                    yield return Result.Failure<List<T>>(error);
                
                yield return items;
                
                skip += batchSize;
            } while (items.Count == batchSize);
        }

        private const int BatchSize = 2000;


        private readonly ElasticCountryManagementService _countryManagementService;
        private readonly ElasticLocalityManagementService _localityManagementService;
        private readonly ElasticAccommodationsManagementService _accommodationsManagementService;
        private readonly IMapperHttpClient _mapperHttpClient;
        private readonly IElasticClient _elasticClient;
        private readonly string _countriesIndex;
        private readonly string _localitiesIndex;
        private readonly string _accommodationsIndex;
        private readonly ILogger<PredictionsManagementService> _logger;
    }
}