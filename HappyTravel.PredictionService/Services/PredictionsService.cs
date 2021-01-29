using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
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
        
        
        public async Task<Result<List<Prediction>>> Search(string query, int skip = 0, int top = 10, CancellationToken cancellationToken = default)
        {
            var (_, isFailure, locations, error) = await _locationsService.Search(query, skip, top, cancellationToken);
            
            return isFailure
                ? Result.Failure<List<Prediction>>(error)
                : Build(locations);
        }


        private List<Prediction> Build(List<Location> locations) => locations.Select(Build).ToList();
        

        private Prediction Build(Location location) => new(location.HtId, location.PredictionText);

        
        private readonly ILocationsService _locationsService;
    }
}