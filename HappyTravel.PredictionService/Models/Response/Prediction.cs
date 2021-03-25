
namespace HappyTravel.Osaka.Api.Models.Response
{
    public readonly struct Prediction
    {
        public Prediction(string htId, string predictionText, string image)
        {
            HtId = htId;
            PredictionText = predictionText;
            Image = image;
        }
        
        /// <summary>
        /// HappyTravel identifier
        /// </summary>
        public string HtId { get; }
        
        /// <summary>
        /// Full prediction text with delimiters
        /// </summary>
        public string PredictionText { get; }
        
        /// <summary>
        /// Image path
        /// </summary>
        public string Image { get; }
    }
}