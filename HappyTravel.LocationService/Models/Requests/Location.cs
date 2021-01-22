using System.Collections.Generic;
using HappyTravel.EdoContracts.GeoData.Enums;
using HappyTravel.Geography;

namespace HappyTravel.LocationService.Models.Requests
{
    public class Location
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Locality { get; set; }
        public string Country { get; set; }
        public string CountryCode { get; set; }
        public GeoPoint Coordinates { get; set; }
        public double DistanceInMeters { get; set; }
        public PredictionSources Source { get; set; }
        public LocationTypes Type { get; set; }
        public List<Suppliers> Suppliers { get; set; }
    }
}