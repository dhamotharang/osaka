using System.Collections.Generic;
using HappyTravel.EdoContracts.GeoData.Enums;
using Nest;

namespace HappyTravel.LocationService.Models.Elasticsearch
{
    public class Location
    {
        public string Id { get; init; }
        public string PredictionText { get; init; }
        public string Name { get; init; }
        public string Locality { get; init; }
        public string Country { get; init; }
        public string CountryCode { get; init; }
        public GeoCoordinate Coordinates { get; init; }
        public double DistanceInMeters { get; init; }
        public PredictionSources Source { get; init; }
        public LocationTypes Type { get; init; }
        public List<Suppliers> Suppliers { get; init; }
    }
}