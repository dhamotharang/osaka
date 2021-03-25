using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using HappyTravel.Osaka.Api.Models.Response;
using HappyTravel.Osaka.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace HappyTravel.Osaka.Api.Controllers
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