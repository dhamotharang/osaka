using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.LocationService.Models.Response;
using HappyTravel.LocationService.Services;
using Microsoft.AspNetCore.Mvc;

namespace HappyTravel.LocationService.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/{v:apiVersion}/predictions")]
    [Produces("application/json")]
    public class PredictionsController : BaseController
    {
        public PredictionsController(IPredictionsService predictionsService)
        {
            _predictionsService = predictionsService;
        }

        
        /// <summary>
        /// Retrieves prediction by a query string 
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [ProducesResponseType(typeof(List<Prediction>), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ProblemDetails), (int) HttpStatusCode.BadRequest)]
        public async Task<IActionResult> SearchPredictions([FromQuery] string query, [FromQuery] int skip = 0, [FromQuery] int top = 10, CancellationToken cancellationToken = default)
        {
            var (_, isFailure, predictions, error) = await _predictionsService.Search(query, skip, top, cancellationToken);
            
            return isFailure 
                ? BadRequestWithProblemDetails(error) 
                : Ok(predictions);
        }


        private readonly IPredictionsService _predictionsService;
    }
}