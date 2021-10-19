using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.MapperContracts.Public.StaticDataPublications;
using Country = HappyTravel.MapperContracts.Public.StaticDataPublications.Country;
using Locality = HappyTravel.MapperContracts.Public.StaticDataPublications.Locality;

namespace HappyTravel.Osaka.Api.Services.HttpClients
{
    public interface IMapperHttpClient
    {
        Task<Result<List<Country>>> GetCountries(DateTime fromDate, int skip = 0, int top = 2000, CancellationToken cancellationToken = default);
        Task<Result<List<Locality>>> GetLocalities(DateTime fromDate, int skip = 0, int top = 2000, CancellationToken cancellationToken = default);
        Task<Result<List<Accommodation>>> GetAccommodations(DateTime fromDate, int skip = 0, int top = 2000, CancellationToken cancellationToken = default);
    }
}