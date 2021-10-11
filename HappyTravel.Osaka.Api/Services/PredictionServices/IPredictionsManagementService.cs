using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.MapperContracts.Public.Locations;

namespace HappyTravel.Osaka.Api.Services.PredictionServices
{
    public interface IPredictionsManagementService
    {
        Task<Result<int>> ReuploadAllPredictionsFromMapper(CancellationToken cancellationToken = default);
        Task<Result> Add(List<LocationDetailedInfo> locations, string index, CancellationToken cancellationToken = default);
        Task<Result> Update(List<LocationDetailedInfo> locations, string index, CancellationToken cancellationToken = default);
        Task<Result> Remove(List<LocationDetailedInfo> locations, string index, CancellationToken cancellationToken = default);
    }
}