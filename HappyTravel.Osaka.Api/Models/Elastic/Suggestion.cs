using System.Collections.Generic;

namespace HappyTravel.Osaka.Api.Models.Elastic
{
    public class Suggestion
    {
        public IEnumerable<string> Input { get; set; } = new List<string>();
        public int Weight { get; set; } = 1;
    }
}