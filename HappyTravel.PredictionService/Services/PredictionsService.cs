using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HappyTravel.PredictionService.Models.Elasticsearch;
using HappyTravel.PredictionService.Models.Response;
using HappyTravel.PredictionService.Services.Locations;

namespace HappyTravel.PredictionService.Services
{
    public class PredictionsService : IPredictionsService
    {
        public PredictionsService(ILocationsService locationsServices)
        {
            _locationsService = locationsServices;
        }

        public async Task<List<Prediction>> Search(string query, CancellationToken cancellationToken = default) =>
            (await _locationsService.Search(query, cancellationToken)).Select(Build).ToList();

        
        private Prediction Build(Location location) => new(location.HtId, location.PredictionText, string.Empty);

        
        private readonly ILocationsService _locationsService;
    }
}