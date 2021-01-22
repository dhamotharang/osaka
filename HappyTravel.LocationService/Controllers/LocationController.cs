using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.LocationService.Models.Requests;
using HappyTravel.LocationService.Services;
using Microsoft.AspNetCore.Mvc;

namespace HappyTravel.LocationService.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/{v:apiVersion}/locations")]
    [Produces("application/json")]
    public class LocationController : BaseController
    {
        public LocationController(ILocationManagementService locationManagementService, ILocationSearchService locationSearchService)
        {
            _locationManagementService = locationManagementService;
            _locationSearchService = locationSearchService;
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
            var (_, isFailure, locations, error) = await _locationSearchService.Search(query, skip, top, cancellationToken);
            
            return isFailure 
                ? BadRequestWithProblemDetails(error) 
                : Ok(locations);
        }


        /// <summary>
        /// Retrieves a location by id
        /// </summary>
        /// <param name="id"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(Models.Elasticsearch.Location), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ProblemDetails), (int) HttpStatusCode.BadRequest)]
        public async Task<IActionResult> GetLocation([FromRoute] string id, CancellationToken cancellationToken = default)
        {
            var (_, isFailure, value, error) = await _locationSearchService.Get(id, cancellationToken);
            
            return isFailure
                ? BadRequestWithProblemDetails(error)
                : Ok(value);
        }
        

        /// <summary>
        /// Uploads locations
        /// </summary>
        /// <param name="locations"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType((int) HttpStatusCode.NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), (int) HttpStatusCode.BadRequest)]
        public async Task<IActionResult> UploadLocations([FromBody] [MaxLength(MaxItemsToProcess)]
            List<Location> locations, CancellationToken cancellationToken = default)
        {
            var (_, isFailure, error) = await _locationManagementService.UploadLocations(locations, LanguageCode, cancellationToken);
            
            return isFailure
                ? BadRequestWithProblemDetails(error)
                : NoContent();
        }
        
        
        /// <summary>
        /// Removes locations by id
        /// </summary>
        /// <param name="ids"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpDelete]
        [ProducesResponseType((int) HttpStatusCode.NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), (int) HttpStatusCode.BadRequest)]
        public async Task<IActionResult> RemoveLocations([FromBody] [MaxLength(MaxItemsToProcess)] List<string> ids, CancellationToken cancellationToken = default)
        {
            var (_, isFailure, error) = await _locationManagementService.RemoveLocations(ids, LanguageCode, cancellationToken);
            
            return isFailure
                ? BadRequestWithProblemDetails(error)
                : NoContent();
        }


        private readonly ILocationManagementService _locationManagementService;
        private readonly ILocationSearchService _locationSearchService;
        
        private const int MaxItemsToProcess = 50000;
    }
}