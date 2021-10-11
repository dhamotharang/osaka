using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using HappyTravel.Osaka.Api.Filters.Authorization;
using HappyTravel.Osaka.Api.Services.PredictionServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace HappyTravel.Osaka.Api.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/{v:apiVersion}/predictions")]
    [Produces("application/json")]
    //[Authorize(Policy = Policies.OnlyManagerClient)]
    public class PredictionsManagementController : BaseController
    {
        public PredictionsManagementController(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        
        /// <summary>
        /// Re-uploads locations from the mapper
        /// </summary>
        [AllowAnonymous]
        [HttpPost("re-upload")]
        [ProducesResponseType((int) HttpStatusCode.Accepted)]
        public IActionResult ReUpload()
        {
            if (_predictionsUploadTokenSource.Token.CanBeCanceled)
                _predictionsUploadTokenSource.Cancel();

            _predictionsUploadTokenSource = new CancellationTokenSource(TimeSpan.FromHours(4));

            Task.Run(async () =>
            {
                using var scope = _serviceProvider.CreateScope();
                var predictionsManagementService = scope.ServiceProvider.GetRequiredService<IPredictionsManagementService>();
                await predictionsManagementService.ReuploadAllPredictionsFromMapper(_predictionsUploadTokenSource.Token);
            }, _predictionsUploadTokenSource.Token);
            // Wait for the task run
            Task.Delay(1000);
            
            return Accepted();
        }

        
        private readonly IServiceProvider _serviceProvider;
        private static CancellationTokenSource _predictionsUploadTokenSource = new (TimeSpan.FromHours(4));
    }
}