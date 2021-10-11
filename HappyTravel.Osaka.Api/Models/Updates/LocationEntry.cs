using HappyTravel.MapperContracts.Public.Locations;

namespace HappyTravel.Osaka.Api.Models.Updates
{
    public class LocationEntry
    {
        public UpdateEventTypes Type { get; init; } = UpdateEventTypes.Undefined;
        public LocationDetailedInfo Location { get; init; } = new ();
    }
}