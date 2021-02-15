using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.PredictionService.Models.Response;
using HappyTravel.PredictionService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HappyTravel.PredictionService.Controllers
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
        public async Task<IActionResult> SearchPredictions([FromQuery] string query, CancellationToken cancellationToken = default)
            => Ok(await _predictionsService.Search(query, cancellationToken));


        private readonly IPredictionsService _predictionsService;
    }
}