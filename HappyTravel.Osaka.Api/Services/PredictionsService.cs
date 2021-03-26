using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HappyTravel.Osaka.Api.Models.Elasticsearch;
using HappyTravel.Osaka.Api.Models.Response;
using HappyTravel.Osaka.Api.Services.Locations;

namespace HappyTravel.Osaka.Api.Services
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