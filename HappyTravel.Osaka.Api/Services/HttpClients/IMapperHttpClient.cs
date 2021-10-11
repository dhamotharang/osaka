using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.MapperContracts.Internal.Mappings.Enums;
using HappyTravel.MapperContracts.Public.Locations;
using HappyTravel.Osaka.Api.Models;

namespace HappyTravel.Osaka.Api.Services.HttpClients
{
    public interface IMapperHttpClient
    {
        Task<Result<List<LocationDetailedInfo>>> GetLocations(MapperLocationTypes locationType, string languageCode,
            DateTime fromDate, int skip = 0, int top = 20000, CancellationToken cancellationToken = default);
    }
}