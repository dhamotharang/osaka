using System.Collections.Generic;

namespace HappyTravel.PredictionService.Models.Elasticsearch
{
    public class Suggestion
    {
        public IEnumerable<string> Input { get; set; } = new List<string>();
        public int Weight { get; set; } = 1;
    }
}