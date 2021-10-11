using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.MapperContracts.Internal.Mappings.Enums;
using HappyTravel.MapperContracts.Public.Locations;
using HappyTravel.Osaka.Api.Infrastructure;
using HappyTravel.Osaka.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace HappyTravel.Osaka.Api.Services.HttpClients
{
    public class MapperHttpClient : IMapperHttpClient
    {
        public MapperHttpClient(IHttpClientFactory httpClientFactory, IOptions<JsonOptions> jsonOptions)
        {
            _jsonSerializerOptions = jsonOptions.Value.JsonSerializerOptions;
            _httpClient = httpClientFactory.CreateClient(HttpClientNames.MapperApi);
        }


        public Task<Result<List<LocationDetailedInfo>>> GetLocations(MapperLocationTypes locationType, string languageCode, DateTime fromDate, int skip = 0, int top = 20000, CancellationToken cancellationToken = default)
            => Post<List<LocationDetailedInfo>>(new HttpRequestMessage(HttpMethod.Get, $"/api/1.0/locations/?type={locationType}&modified={fromDate:s}&skip={skip}&top={top}"), languageCode, cancellationToken);

        
        private async Task<Result<TResponse>> Post<TResponse>(HttpRequestMessage requestMessage, string languageCode = "", CancellationToken cancellationToken = default)
        {
            try
            {
                var response = await _httpClient.SendAsync(requestMessage, cancellationToken);
                
                if (response.IsSuccessStatusCode)
                    return (await response.Content.ReadFromJsonAsync<TResponse>(_jsonSerializerOptions, cancellationToken))!;

                var problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetails>(_jsonSerializerOptions,
                    cancellationToken);
                var error = problemDetails is not null
                    ? $"{problemDetails.Status} {problemDetails.Title} {problemDetails.Detail}"
                    : $"{response.StatusCode} {response.ReasonPhrase}";
                
                return Result.Failure<TResponse>(error);
            }
            catch (Exception ex)
            {
                return Result.Failure<TResponse>(ex.Message);
            }
        }


        private readonly HttpClient _httpClient;
        private readonly JsonSerializerOptions _jsonSerializerOptions;
    }
}