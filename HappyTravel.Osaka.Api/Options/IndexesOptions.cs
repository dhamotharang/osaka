namespace HappyTravel.Osaka.Api.Options
{
    public record IndexesOptions
    {
        public Indexes EnglishIndexes { get; set; } = new();
    }
}