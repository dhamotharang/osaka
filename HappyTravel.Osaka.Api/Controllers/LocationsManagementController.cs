using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using HappyTravel.Osaka.Api.Filters.Authorization;
using HappyTravel.Osaka.Api.Services.Locations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace HappyTravel.Osaka.Api.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/{v:apiVersion}/locations")]
    [Produces("application/json")]
    [Authorize(Policy = Policies.OnlyManagerClient)]
    public class LocationsManagementController : BaseController
    {
        public LocationsManagementController(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        
        /// <summary>
        /// Re-uploads locations from the mapper
        /// </summary>
        [HttpPost("re-upload")]
        [ProducesResponseType((int) HttpStatusCode.Accepted)]
        public IActionResult ReUpload()
        {
            if (_locationsUploadTokenSource.Token.CanBeCanceled)
                _locationsUploadTokenSource.Cancel();

            _locationsUploadTokenSource = new CancellationTokenSource(TimeSpan.FromHours(4));

            Task.Run(async () =>
            {
                using var scope = _serviceProvider.CreateScope();
                var locationsManagementService = scope.ServiceProvider.GetRequiredService<IPredictionsManagementService>();
                await locationsManagementService.ReUploadAllPredictionsFromMapper(_locationsUploadTokenSource.Token);
            }, _locationsUploadTokenSource.Token);
            // Wait for the task run
            Task.Delay(1000);
            
            return Accepted();
        }

        
        private readonly IServiceProvider _serviceProvider;
        private static CancellationTokenSource _locationsUploadTokenSource = new (TimeSpan.FromHours(4));
    }
}