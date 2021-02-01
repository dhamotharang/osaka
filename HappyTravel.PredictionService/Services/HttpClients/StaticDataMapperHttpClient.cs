using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.PredictionService.Infrastructure;
using HappyTravel.PredictionService.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace HappyTravel.PredictionService.Services.HttpClients
{
    public class StaticDataMapperHttpClient : IStaticDataMapperHttpClient
    {
        public StaticDataMapperHttpClient(IHttpClientFactory httpClientFactory, IOptions<JsonOptions> jsonOptions,
            ILogger<StaticDataMapperHttpClient> logger)
        {
            _httpClientFactory = httpClientFactory;
            _jsonSerializerOptions = jsonOptions.Value.JsonSerializerOptions;
            _logger = logger;
        }


        public Task<Result<List<Location>>> GetLocations(MapperLocationTypes locationType, string languageCode, DateTime fromDate, int skip = 0, int top = 20000, CancellationToken cancellationToken = default)
            => Execute<List<Location>>(new HttpRequestMessage(HttpMethod.Get, $"/api/1.0/location-mappings/locations/?locationType={locationType}&modified={fromDate:s}&skip={skip}&top={top}"), languageCode, cancellationToken);


        private async Task<Result<TResponse>> Execute<TResponse>(HttpRequestMessage requestMessage, string languageCode = "", CancellationToken cancellationToken = default)
        {
            var httpClient = _httpClientFactory.CreateClient(HttpClientNames.MapperApi);
            httpClient.DefaultRequestHeaders.Add("Accept-Language", languageCode);
            var responseMessage = await httpClient.SendAsync(requestMessage, cancellationToken);
            var stream = await responseMessage.Content.ReadAsStreamAsync(cancellationToken);

            if (responseMessage.IsSuccessStatusCode)
                return (await JsonSerializer.DeserializeAsync<TResponse>(stream, _jsonSerializerOptions, cancellationToken))!;

            string? error;
            try
            {
                error = (await JsonSerializer.DeserializeAsync<ProblemDetails>(stream, _jsonSerializerOptions, cancellationToken))?.Detail;
            }
            catch
            {
                error = $"Reason {responseMessage.ReasonPhrase}, Code: {responseMessage.StatusCode}";
            }
            
            return Result.Failure<TResponse>(error);
        }


        private readonly IHttpClientFactory _httpClientFactory;
        private readonly JsonSerializerOptions _jsonSerializerOptions;
        private readonly ILogger<StaticDataMapperHttpClient> _logger;
    }
}