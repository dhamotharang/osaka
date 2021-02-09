using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.PredictionService.Services.Locations;
using Microsoft.AspNetCore.Mvc;

namespace HappyTravel.PredictionService.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/{v:apiVersion}/locations")]
    [Produces("application/json")]
    public class LocationsController : BaseController
    {
        public LocationsController(ILocationsService locationServices)
        {
            _locationsService = locationServices;
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

        
        private readonly ILocationsService _locationsService;
    }
}