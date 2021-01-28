using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.LocationService.Services.Locations;
using HappyTravel.LocationService.Services.Locations.Mapper;
using Microsoft.AspNetCore.Mvc;

namespace HappyTravel.LocationService.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/{v:apiVersion}/locations")]
    [Produces("application/json")]
    public class LocationsController : BaseController
    {
        public LocationsController(IEnumerable<ILocationsService> locationServices)
        {
            _locationsService = locationServices.FirstOrDefault(s => s.GetType() == typeof(MapperLocationsService));
        }

        
        /// <summary>
        /// Retrieves locations by a query
        /// </summary>
        /// <param name="query"></param>
        /// <param name="skip"></param>
        /// <param name="top"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpGet]
        [ProducesResponseType(typeof(List<Models.Elasticsearch.Location>), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ProblemDetails), (int) HttpStatusCode.BadRequest)]
        public async Task<IActionResult> SearchLocations([FromQuery] string query, [FromQuery] int skip = 0, [FromQuery] int top = 10, CancellationToken cancellationToken = default)
        {
            var (_, isFailure, locations, error) = await _locationsService!.Search(query, skip, top, cancellationToken);
            
            return isFailure 
                ? BadRequestWithProblemDetails(error) 
                : Ok(locations);
        }


        /// <summary>
        /// Retrieves a location by id
        /// </summary>
        /// <param name="htId"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpGet("{htId}")]
        [ProducesResponseType(typeof(Models.Elasticsearch.Location), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ProblemDetails), (int) HttpStatusCode.BadRequest)]
        public async Task<IActionResult> GetLocation([FromRoute] string htId, CancellationToken cancellationToken = default)
        {
            var (_, isFailure, value, error) = await _locationsService!.Get(htId, cancellationToken);
            
            return isFailure
                ? BadRequestWithProblemDetails(error)
                : Ok(value);
        }

        private readonly ILocationsService _locationsService = null!;
    }
}