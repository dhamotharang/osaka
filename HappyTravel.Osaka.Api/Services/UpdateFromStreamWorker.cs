using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.MapperContracts.Public.Locations;
using HappyTravel.Osaka.Api.Infrastructure;
using HappyTravel.Osaka.Api.Infrastructure.Logging;
using HappyTravel.Osaka.Api.Models;
using HappyTravel.Osaka.Api.Models.Updates;
using HappyTravel.Osaka.Api.Options;
using HappyTravel.Osaka.Api.Services.PredictionServices;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Sentry;
using StackExchange.Redis;
using StackExchange.Redis.Extensions.Core.Abstractions;

namespace HappyTravel.Osaka.Api.Services
{
    public class UpdateFromStreamWorker : BackgroundService
    {
        public UpdateFromStreamWorker(IRedisCacheClient redisCacheClient, IPredictionsManagementService predictionsManagementService, IOptions<PredictionUpdateOptions> updateOptions, IOptions<IndexOptions> indexOptions, ILogger<UpdateFromStreamWorker> logger)
        {
            _logger = logger;
            _updateOptions = updateOptions.Value;
            _predictionsManagementService = predictionsManagementService;
            _database = redisCacheClient.GetDbFromConfiguration().Database;
            GetIndexName(indexOptions.Value, out _index);
            InitRedisStreamIfNeeded();
        }


        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            const int batchSize = 5;

            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var entries = await GetEntries();
                    if (entries.Any())
                    {
                        while (entries.Any())
                        {
                            _logger.LogGetLocationsFromMapper($"'{entries.Select(e => e.Values.Length).Sum(i => i)}' locations have been got from the mapper");

                            foreach (var entry in entries)
                                await ProcessEntry(entry);

                            await DeleteEntries(entries);
                            entries = await GetEntries();
                        }
                    }
                    else
                    {
                        await Task.Delay(1000, cancellationToken);
                    }
                }
                catch (Exception ex)
                {
                    SentrySdk.CaptureException(ex);
                }
            }
            
            async Task ProcessEntry(StreamEntry entry)
            {
                var locations = GetLocations(entry);
                await UpdateIndex(locations, cancellationToken);
            }
            
            
            Task<StreamEntry[]> GetEntries() => _database!.StreamReadAsync(_updateOptions.StreamName, "0-0", batchSize);
            
            
            Task DeleteEntries(StreamEntry[] entries) => _database!.StreamDeleteAsync(_updateOptions.StreamName, entries.Select(e=> e.Id).ToArray());
        }

        
        private void GetIndexName(IndexOptions indexOptions, out string index)
        {
            const string enLanguage = "en";
            if (!ElasticsearchHelper.TryGetIndex(indexOptions.Indexes!, enLanguage, out index))
                throw new ArgumentException($"Failed to get an index name by the language code '{enLanguage}");
        }
        
        
        private bool InitRedisStreamIfNeeded()
        {
            var streamName = _updateOptions.StreamName;
            try
            {
                // Throws an exceptions if the stream doesn't exist
                _database.StreamInfo(streamName);

                return false;
            }
            catch
            {
                // A value must be added to init the stream
                var initId = _database.StreamAdd(streamName, new[] {new NameValueEntry("init", "init")});
                _database.StreamDelete(streamName, new[] {initId});

                return true;
            }
        }
        
        
        private Dictionary<UpdateEventTypes, List<LocationDetailedInfo>> GetLocations(StackExchange.Redis.StreamEntry streamEntry)
        {
            var locations = new Dictionary<UpdateEventTypes, List<LocationDetailedInfo>>();

            foreach (var nameValueEntry in streamEntry.Values)
            {
                var entry = JsonSerializer.Deserialize<LocationEntry>(nameValueEntry.Value);

                if (!locations.ContainsKey(entry!.Type)) 
                    locations[entry.Type] = new List<LocationDetailedInfo>();

                locations[entry.Type].Add(entry.Location);
            }

            return locations;
        }
        

        private async Task UpdateIndex(Dictionary<UpdateEventTypes, List<LocationDetailedInfo>> locations, CancellationToken cancellationToken)
        {
            foreach (var locationKeyValue in locations)
            {
                var (_, isFailure, error) = await UpdateIndex(locationKeyValue.Key, locationKeyValue.Value, cancellationToken);
                if (isFailure && !string.IsNullOrEmpty(error))
                    _logger.LogUploadingError(error);
            }
        }


        private Task<Result> UpdateIndex(UpdateEventTypes updateEventType, List<LocationDetailedInfo> locations, CancellationToken cancellationToken)
        {
            return updateEventType switch
            {
                UpdateEventTypes.Add => _predictionsManagementService.Add(locations, _index, cancellationToken),
                UpdateEventTypes.Remove => _predictionsManagementService.Remove(locations, _index, cancellationToken),
                UpdateEventTypes.Update => _predictionsManagementService.Update(locations, _index, cancellationToken),
                _ => throw new ArgumentOutOfRangeException(nameof(updateEventType), updateEventType, null)
            };
        }

        
        private readonly string _index;
        private readonly ILogger<UpdateFromStreamWorker> _logger;
        private readonly PredictionUpdateOptions _updateOptions;
        private readonly IPredictionsManagementService _predictionsManagementService;
        private readonly IDatabase _database;
    }
}