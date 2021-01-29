using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.PredictionService.Models.Response;

namespace HappyTravel.PredictionService.Services
{
    public interface IPredictionsService
    {
        Task<Result<List<Prediction>>> Search(string query, int skip = 0, int top = 10, CancellationToken cancellationToken = default);
    }
}