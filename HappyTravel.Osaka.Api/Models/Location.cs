using HappyTravel.MapperContracts.Internal.Mappings.Enums;
using HappyTravel.Osaka.Api.Models.Updates;
using Nest;

namespace HappyTravel.Osaka.Api.Models
{
    public class Location
    {
        public string HtId { get; init; } = string.Empty;
        public string Name { get; init; }  = string.Empty;
        public string Locality { get; init; } = string.Empty;
        public string Country { get; init; } = string.Empty;
        public string CountryCode { get; init; } = string.Empty;
        public Coordinate Coordinates { get; init; } = new(0, 0);
        public double DistanceInMeters { get; init; }
        public MapperLocationTypes LocationType { get; init; } = 0;
        public LocationTypes Type { get; init; } = LocationTypes.Unknown;
    }
}