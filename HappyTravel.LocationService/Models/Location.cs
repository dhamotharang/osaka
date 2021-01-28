using HappyTravel.EdoContracts.GeoData.Enums;
using HappyTravel.Geography;

namespace HappyTravel.LocationService.Models
{
    public class Location
    {
        public string Id { get; init; }
        public string HtId { get; init; }
        public string Name { get; init; }
        public string Locality { get; init; }
        public string Country { get; init; }
        public string CountryCode { get; init; }
        public GeoPoint Coordinates { get; init; }
        public double DistanceInMeters { get; init; }
        public AccommodationMapperLocationTypes LocationType { get; init; }
        public LocationTypes Type { get; init; }
    }
}