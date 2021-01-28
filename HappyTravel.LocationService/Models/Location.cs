using HappyTravel.EdoContracts.GeoData.Enums;
using HappyTravel.Geography;

namespace HappyTravel.LocationService.Models
{
    public class Location
    {
        public string HtId { get; init; } = string.Empty;
        public string Name { get; init; }  = string.Empty;
        public string Locality { get; init; } = string.Empty;
        public string Country { get; init; } = string.Empty;
        public string CountryCode { get; init; } = string.Empty;
        public GeoPoint Coordinates { get; init; } = new(0, 0);
        public double DistanceInMeters { get; init; }
        public AccommodationMapperLocationTypes LocationType { get; init; } = 0;
        public LocationTypes Type { get; init; } = LocationTypes.Unknown;
    }
}