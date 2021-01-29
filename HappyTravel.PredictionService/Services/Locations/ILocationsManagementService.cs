using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;

namespace HappyTravel.PredictionService.Services.Locations
{
    public interface ILocationsManagementService
    {
        Task<Result<int>> ReUpload(CancellationToken cancellationToken = default);
    }
}