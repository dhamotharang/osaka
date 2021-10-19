namespace HappyTravel.Osaka.Api.Options
{
    public record Indexes
    {
        public string Countries { get; set; } = string.Empty;
        public string Localities { get; set; } = string.Empty;
        public string Accommodations { get; set; } = string.Empty;
    }
}