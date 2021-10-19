using System;
using Nest;

namespace HappyTravel.Osaka.Api.Models.Elastic
{
    public class ElasticAccommodation : IElasticModel
    {
        public string Id { get; init; } = string.Empty;
        public string Name { get; init; } = string.Empty;
        public string Locality { get; init; } = string.Empty;
        public string Country { get; init; } = string.Empty;
        public GeoCoordinate Coordinates { get; init; } = new(0, 0);
        public bool IsPromotion { get; init; } = false;
        public bool IsDirectContract { get; init; } = false;
        public bool IsMappingConfirmed { get; init; } = false;
        public bool IsInDomesticZone { get; init; } = false;
        public int NumberOfAccommodationsInLocality { get; init; }
        public DateTime Modified { get; init; }
        public string PredictionText { get; init; } = string.Empty;
        public Suggestion Suggestion { get; init; } = new();
        public bool Enabled { get; init; } = true;
    }
}