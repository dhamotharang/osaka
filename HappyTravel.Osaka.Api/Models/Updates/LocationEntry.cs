namespace HappyTravel.Osaka.Api.Models.Updates
{
    public class LocationEntry
    {
        public UpdateEventTypes Type { get; init; } = UpdateEventTypes.Undefined;
        public Location Location { get; init; } = new ();
    }
}