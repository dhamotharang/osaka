using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using HappyTravel.PredictionService.Filters.Authorization;
using HappyTravel.PredictionService.Services.Locations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace HappyTravel.PredictionService.Controllers
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
        /// <param name="cancellationToken"></param>
        [HttpPost("re-upload")]
        [ProducesResponseType((int) HttpStatusCode.Accepted)]
        public IActionResult ReUpload(CancellationToken cancellationToken = default)
        {
            if (_locationsUploadTokenSource.Token.CanBeCanceled)
                _locationsUploadTokenSource.Cancel();

            _locationsUploadTokenSource = new CancellationTokenSource(TimeSpan.FromHours(8));

            Task.Run(async () =>
            {
                using var scope = _serviceProvider.CreateScope();

                var locationsManagementService = scope.ServiceProvider.GetRequiredService<ILocationsManagementService>();
                await locationsManagementService.ReUpload(cancellationToken);
            }, _locationsUploadTokenSource.Token);

            return Accepted();
        }

        
        private readonly IServiceProvider _serviceProvider;
        private static CancellationTokenSource _locationsUploadTokenSource = new (TimeSpan.FromHours(8));
    }
}