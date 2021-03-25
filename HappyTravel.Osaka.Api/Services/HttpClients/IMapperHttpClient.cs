using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Osaka.Api.Models;

namespace HappyTravel.Osaka.Api.Services.HttpClients
{
    public interface IMapperHttpClient
    {
        Task<Result<List<Location>>> GetLocations(MapperLocationTypes locationType, string languageCode,
            DateTime fromDate, int skip = 0, int top = 20000, CancellationToken cancellationToken = default);
    }
}