using System;

namespace HappyTravel.Osaka.Api.Models.Elastic
{
    public class ElasticLocality : IElasticModel
    {
        public string Id { get; init; } = string.Empty;
        public string Name { get; init; } = string.Empty;
        public string Country { get; init; } = string.Empty;
        public int NumberOfAccommodations { get; init; }
        public DateTime Modified { get; init; }
        public Suggestion Suggestion { get; init; } = new();
        public string PredictionText { get; init; } = string.Empty;
        public bool Enabled { get; init; } = true;
    }
}