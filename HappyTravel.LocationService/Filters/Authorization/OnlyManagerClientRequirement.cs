using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace HappyTravel.LocationService.Filters.Authorization
{
    public class OnlyManagerClientRequirement : AuthorizationHandler<OnlyManagerClientRequirement>, IAuthorizationRequirement
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, OnlyManagerClientRequirement requirement)
        {
            var clientId = context.User.Claims
                .SingleOrDefault(c => c.Type == "client_id")?.Value;

            switch (clientId)
            {
                case null:
                    context.Fail();
                    break;
                case ManagerClientName:
                    context.Succeed(requirement);
                    break;
                default:
                    context.Fail();
                    break;
            }
            
            return Task.CompletedTask;
        }

        
        private const string ManagerClientName = "location_service_manager_client";
    }
}