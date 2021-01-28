using System.Net;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.LocationService.Services.Locations.Mapper;
using Microsoft.AspNetCore.Mvc;

namespace HappyTravel.LocationService.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/{v:apiVersion}/locations")]
    [Produces("application/json")]
    public class LocationsManagementController : BaseController
    {
        public LocationsManagementController(IMapperLocationsManagementService locationsManagementService)
        {
            _locationsManagementService = locationsManagementService;
        }

        
        /// <summary>
        /// Re-uploads locations from the mapper
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns>Number of locations</returns>
        [HttpPost("re-upload")]
        [ProducesResponseType(typeof(int), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ProblemDetails), (int) HttpStatusCode.BadRequest)]
        public async Task<IActionResult> ReUpload(CancellationToken cancellationToken = default)
        {
            var (_, isFailure, uploaded, error) = await _locationsManagementService.ReUpload(cancellationToken);

            return !isFailure
                ? Ok($"Locations uploaded '{uploaded}'")
                : BadRequestWithProblemDetails(error);
        }

        
        private readonly IMapperLocationsManagementService _locationsManagementService;
    }
}