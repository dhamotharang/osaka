using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;

namespace HappyTravel.LocationService.Services.Locations.Mapper
{
    public interface IMapperLocationsManagementService
    {
        Task<Result<int>> ReUploadLocations(CancellationToken cancellationToken = default);
    }
}