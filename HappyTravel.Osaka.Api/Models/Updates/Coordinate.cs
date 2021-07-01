namespace HappyTravel.Osaka.Api.Models.Updates
{
    public struct Coordinate
    {
        public Coordinate(double latitude, double longitude)
        {
            Latitude = latitude;
            Longitude = longitude;
        }
        
        public double Latitude { get; init; }
        public double Longitude { get; init; }
    }
}