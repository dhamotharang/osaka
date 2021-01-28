
namespace HappyTravel.LocationService.Models.Response
{
    public readonly struct Prediction
    {
        public Prediction(string htId, string predictionText)
        {
            HtId = htId;
            PredictionText = predictionText;
        }
        
        
        public string HtId { get; }
        public string PredictionText { get; }
    }
}