using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.LocationService.Models.Elasticsearch;
using HappyTravel.LocationService.Models.Response;
using HappyTravel.LocationService.Services.Locations;

namespace HappyTravel.LocationService.Services
{
    public class PredictionsService : IPredictionsService
    {
        public PredictionsService(ILocationsService locationsServices)
        {
            _mapperLocationsService = locationsServices;
        }
        
        
        public async Task<Result<List<Prediction>>> Search(string query, int skip = 0, int top = 10, CancellationToken cancellationToken = default)
        {
            var (_, isFailure, locations, error) = await _mapperLocationsService.Search(query, skip, top, cancellationToken);
            
            return isFailure
                ? Result.Failure<List<Prediction>>(error)
                : Build(locations);
        }


        private List<Prediction> Build(List<Location> locations) => locations.Select(Build).ToList();
        

        private Prediction Build(Location location) => new(location.HtId, location.PredictionText);

        
        private readonly ILocationsService _mapperLocationsService;
    }
}