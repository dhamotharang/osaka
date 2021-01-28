using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.LocationService.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace HappyTravel.LocationService.Services.HttpClients
{
    public class MapperHttpClient
    {
        public MapperHttpClient(HttpClient httpClient, IOptions<JsonOptions> jsonOptions)
        {
            _httpClient = httpClient;
            _jsonSerializerOptions = jsonOptions.Value.JsonSerializerOptions;
        }


        public Task<Result<List<Location>>> GetLocations(AccommodationMapperLocationTypes locationType, string languageCode, DateTime fromDate, int skip = 0, int top = 20000, CancellationToken cancellationToken = default)
            => Execute<List<Location>>(new HttpRequestMessage(HttpMethod.Get, $"/api/1.0/location-mappings/locations/?locationType={locationType}&modified={fromDate:s}&skip={skip}&top={top}"), languageCode, cancellationToken);


        private async Task<Result<TResponse>> Execute<TResponse>(HttpRequestMessage requestMessage, string languageCode = "", CancellationToken cancellationToken = default)
        {
            _httpClient.DefaultRequestHeaders.Add("Accept-Language", languageCode);
            var responseMessage = await _httpClient.SendAsync(requestMessage, cancellationToken);
            var stream = await responseMessage.Content.ReadAsStreamAsync(cancellationToken);

            if (responseMessage.IsSuccessStatusCode)
                return await JsonSerializer.DeserializeAsync<TResponse>(stream, _jsonSerializerOptions, cancellationToken) ?? throw new InvalidOperationException();

            var problemDetails = await JsonSerializer.DeserializeAsync<ProblemDetails>(stream, _jsonSerializerOptions, cancellationToken);

            var error = problemDetails is null 
                ? $"Reason {responseMessage.ReasonPhrase}, Code: {responseMessage.StatusCode}" 
                : problemDetails.Detail;

            return Result.Failure<TResponse>(error);
        }


        private readonly HttpClient _httpClient;
        private readonly JsonSerializerOptions _jsonSerializerOptions; 
    }
}