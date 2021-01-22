using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.LocationService.Models.Requests;

namespace HappyTravel.LocationService.Services
{
    public interface ILocationManagementService
    {
        Task<Result> UploadLocations(List<Location> locations, string languageCode, CancellationToken cancellationToken = default);
        Task<Result> RemoveLocations(List<string> ids, string languageCode, CancellationToken cancellationToken = default);
    }
}