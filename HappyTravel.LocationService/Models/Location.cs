using System.Collections.Generic;
using HappyTravel.EdoContracts.GeoData.Enums;
using Nest;

namespace HappyTravel.LocationService.Models
{
    public class Location
    {
        public string Id { get; init; }
        public string PredictionText { get; init; }
        public string Name { get; set; }
        public string Locality { get; set; }
        public string Country { get; set; }
        public GeoCoordinate Coordinates { get; set; }
        public double DistanceInMeters { get; set; }
        public PredictionSources Source { get; set; }
        public LocationTypes Type { get; set; }
        public List<Suppliers> Suppliers { get; set; }
    }
}