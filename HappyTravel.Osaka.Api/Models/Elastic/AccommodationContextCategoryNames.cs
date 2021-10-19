namespace HappyTravel.Osaka.Api.Models.Elastic
{
    public static class AccommodationContextCategoryNames
    {
        public static readonly string IsPromotion = nameof(ElasticAccommodation.IsPromotion).ToLowerInvariant();
        public static readonly string IsDirectContract = nameof(ElasticAccommodation.IsDirectContract).ToLowerInvariant();
        public static readonly string IsInDomesticZone = nameof(ElasticAccommodation.IsInDomesticZone).ToLowerInvariant();
        public static readonly string IsConfirmed = nameof(ElasticAccommodation.IsMappingConfirmed).ToLowerInvariant();
    }
}