using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Osaka.Api.Infrastructure;
using HappyTravel.Osaka.Api.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;

namespace HappyTravel.Osaka.Api.Services.HttpClients
{
    public class MapperHttpClient : IMapperHttpClient
    {
        public MapperHttpClient(IHttpClientFactory httpClientFactory, IOptions<JsonOptions> jsonOptions, IHttpContextAccessor httpContextAccessor)
        {
            _jsonSerializerOptions = jsonOptions.Value.JsonSerializerOptions;
            _httpClient = httpClientFactory.CreateClient(HttpClientNames.MapperApi);

            if (_httpClient.DefaultRequestHeaders.Contains(HeaderNames.Authorization)) return;
            
            var authToken = httpContextAccessor.HttpContext!.Request.Headers[HeaderNames.Authorization];
            _httpClient.DefaultRequestHeaders.Add(HeaderNames.Authorization, authToken.Single());
        }


        public Task<Result<List<Location>>> GetLocations(MapperLocationTypes locationType, string languageCode, DateTime fromDate, int skip = 0, int top = 20000, CancellationToken cancellationToken = default)
            => Execute<List<Location>>(new HttpRequestMessage(HttpMethod.Get, $"/api/1.0/location-mappings/locations/?locationType={locationType}&modified={fromDate:s}&skip={skip}&top={top}"), languageCode, cancellationToken);

        
        private async Task<Result<TResponse>> Execute<TResponse>(HttpRequestMessage requestMessage, string languageCode = "", CancellationToken cancellationToken = default)
        {
            if (!_httpClient.DefaultRequestHeaders.Contains(HeaderNames.AcceptLanguage))
                _httpClient.DefaultRequestHeaders.Add(HeaderNames.AcceptLanguage, languageCode);
            
            var responseMessage = await _httpClient.SendAsync(requestMessage, cancellationToken);
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


        private readonly HttpClient _httpClient;
        private readonly JsonSerializerOptions _jsonSerializerOptions;
    }
}