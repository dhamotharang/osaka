using System;
using Nest;

namespace HappyTravel.Osaka.Api.Models.Elasticsearch
{
    public class ElasticLocation
    {
        public string Id { get; init; } = string.Empty;
        public string Name { get; init; } = string.Empty;
        public string? Locality { get; init; } = string.Empty;
        public string Country { get; init; } = string.Empty;
        public GeoCoordinate Coordinates { get; init; } = new(0, 0);
        public string Type { get; init; } = string.Empty;
        public DateTime Modified { get; init; }
        public Suggestion Suggestion { get; init; } = new();
        public string PredictionText { get; init; } = string.Empty;
        public int NumberOfAccommodations { get; init; } = 0;
        public bool IsDirectContract { get; init; } = false;
        public bool IsConfirmed { get; init; } = false;
        public bool IsInDomesticZone { get; init; } = false;
    }
}