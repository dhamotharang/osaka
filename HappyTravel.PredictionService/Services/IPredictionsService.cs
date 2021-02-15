using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using HappyTravel.PredictionService.Models.Response;

namespace HappyTravel.PredictionService.Services
{
    public interface IPredictionsService
    {
        Task<List<Prediction>> Search(string query, CancellationToken cancellationToken = default);
    }
}