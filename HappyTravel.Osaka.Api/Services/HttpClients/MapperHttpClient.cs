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
using HappyTravel.MapperContracts.Public.StaticDataPublications;
using HappyTravel.Osaka.Api.Infrastructure;
using HappyTravel.Osaka.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Country = HappyTravel.MapperContracts.Public.StaticDataPublications.Country;
using Locality = HappyTravel.MapperContracts.Public.StaticDataPublications.Locality;

namespace HappyTravel.Osaka.Api.Services.HttpClients
{
    public class MapperHttpClient : IMapperHttpClient
    {
        public MapperHttpClient(IHttpClientFactory httpClientFactory, IOptions<JsonOptions> jsonOptions)
        {
            _jsonSerializerOptions = jsonOptions.Value.JsonSerializerOptions;
            _httpClient = httpClientFactory.CreateClient(HttpClientNames.MapperApi);
        }


        public Task<Result<List<Country>>> GetCountries(DateTime fromDate, int skip = 0, int top = 2000, CancellationToken cancellationToken = default)
            => Send<List<Country>>(new HttpRequestMessage(HttpMethod.Get, $"/api/1.0/publications/countries?modified={fromDate:s}&skip={skip}&top={top}"), cancellationToken);

        
        public Task<Result<List<Locality>>> GetLocalities(DateTime fromDate, int skip = 0, int top = 2000, CancellationToken cancellationToken = default)
            => Send<List<Locality>>(new HttpRequestMessage(HttpMethod.Get, $"/api/1.0/publications/localities?modified={fromDate:s}&skip={skip}&top={top}"), cancellationToken);
        
        
        public Task<Result<List<Accommodation>>> GetAccommodations(DateTime fromDate, int skip = 0, int top = 2000, CancellationToken cancellationToken = default)
            => Send<List<Accommodation>>(new HttpRequestMessage(HttpMethod.Get, $"/api/1.0/publications/accommodations?modified={fromDate:s}&skip={skip}&top={top}"), cancellationToken);

        
        private async Task<Result<TResponse>> Send<TResponse>(HttpRequestMessage requestMessage, CancellationToken cancellationToken = default)
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