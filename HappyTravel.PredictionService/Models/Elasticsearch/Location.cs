using System;
using HappyTravel.EdoContracts.GeoData.Enums;
using Nest;

namespace HappyTravel.PredictionService.Models.Elasticsearch
{
    public class Location
    {
        public string Id { get; init; } = string.Empty;
        public string HtId => Id;
        public string Name { get; init; } = string.Empty;
        public string Locality { get; init; } = string.Empty;
        public string Country { get; init; } = string.Empty;
        public string CountryCode { get; init; } = string.Empty;
        public GeoCoordinate Coordinates { get; init; } = new(0, 0);
        public double DistanceInMeters { get; init; }
        public LocationTypes Type { get; init; } = LocationTypes.Unknown;
        public string LocationType { get; init; } = string.Empty;
        public DateTime Modified { get; init; }
        public Suggestion Suggestion { get; init; } = new();
        public string PredictionText { get; init; } = string.Empty;
    }
}