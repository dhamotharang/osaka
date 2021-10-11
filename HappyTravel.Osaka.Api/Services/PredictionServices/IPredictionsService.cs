using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using HappyTravel.Osaka.Api.Models.Response;

namespace HappyTravel.Osaka.Api.Services.PredictionServices
{
    public interface IPredictionsService
    {
        Task<List<Prediction>> Search(string query, CancellationToken cancellationToken = default);
    }
}