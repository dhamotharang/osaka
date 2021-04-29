using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using HappyTravel.Osaka.Api.Infrastructure;
using HappyTravel.Osaka.Api.Infrastructure.Logging;
using HappyTravel.Osaka.Api.Models;
using HappyTravel.Osaka.Api.Models.Updates;
using HappyTravel.Osaka.Api.Options;
using HappyTravel.Osaka.Api.Services.Locations;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Sentry;
using StackExchange.Redis.Extensions.Core.Abstractions;

namespace HappyTravel.Osaka.Api.Services
{
    public class UpdateFromStreamWorker : BackgroundService
    {
        public UpdateFromStreamWorker(IRedisCacheClient redisCacheClient, IPredictionsManagementService predictionsManagementService, IOptions<PredictionUpdateOptions> updateOptions, IOptions<IndexOptions> indexOptions, ILogger<UpdateFromStreamWorker> logger)
        {
            _updateOptions = updateOptions.Value;
            _redisCacheClient = redisCacheClient;
            _predictionsManagementService = predictionsManagementService;
            _logger = logger;
            Init(indexOptions.Value, out _index);
        }


        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            var database = _redisCacheClient.GetDbFromConfiguration().Database;
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
            
            async Task ProcessEntry(StackExchange.Redis.StreamEntry entry)
            {
                var locations = GetLocations(entry);
                await UpdateIndex(locations, cancellationToken);
            }
            
            
            Task<StackExchange.Redis.StreamEntry[]> GetEntries() => database!.StreamReadAsync(_updateOptions.StreamName, "0-0", batchSize);
            
            
            Task DeleteEntries(StackExchange.Redis.StreamEntry[] entries) => database!.StreamDeleteAsync(_updateOptions.StreamName, entries.Select(e=> e.Id).ToArray());
        }

        
        private void Init(IndexOptions indexOptions, out string index)
        {
            const string enLanguage = "en";
            if (!ElasticsearchHelper.TryGetIndex(indexOptions.Indexes!, enLanguage, out index))
                throw new ArgumentException($"Failed to get an index name by the language code '{enLanguage}");
        }
        
        
        private Dictionary<UpdateEventTypes, List<Location>> GetLocations(StackExchange.Redis.StreamEntry streamEntry)
        {
            var locations = new Dictionary<UpdateEventTypes, List<Location>>();

            foreach (var nameValueEntry in streamEntry.Values)
            {
                var entry = JsonSerializer.Deserialize<LocationEntry>(nameValueEntry.Value);

                if (!locations.ContainsKey(entry!.UpdateEventType)) 
                    locations[entry.UpdateEventType] = new List<Location>();

                locations[entry.UpdateEventType].Add(entry.Location);
            }

            return locations;
        }
        

        private async Task UpdateIndex(Dictionary<UpdateEventTypes, List<Location>> locations, CancellationToken cancellationToken)
        {
            foreach (var locationKeyValue in locations)
                await UpdateIndex(locationKeyValue.Key, locationKeyValue.Value, cancellationToken);
        }


        private Task UpdateIndex(UpdateEventTypes updateEventType, List<Location> locations, CancellationToken cancellationToken)
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
        private readonly IRedisCacheClient _redisCacheClient;
        private readonly IPredictionsManagementService _predictionsManagementService;
    }
}