using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.PredictionService.Models;

namespace HappyTravel.PredictionService.Services.HttpClients
{
    public interface IStaticDataMapperHttpClient
    {
        Task<Result<List<Location>>> GetLocations(AccommodationMapperLocationTypes locationType, string languageCode,
            DateTime fromDate, int skip = 0, int top = 20000, CancellationToken cancellationToken = default);
    }
}