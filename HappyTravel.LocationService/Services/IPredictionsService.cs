using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.LocationService.Models.Response;

namespace HappyTravel.LocationService.Services
{
    public interface IPredictionsService
    {
        Task<Result<List<Prediction>>> Search(string query, int skip = 0, int top = 10, CancellationToken cancellationToken = default);
    }
}