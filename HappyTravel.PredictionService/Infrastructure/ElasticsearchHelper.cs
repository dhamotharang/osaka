using System.Collections.Generic;
using HappyTravel.MultiLanguage;

namespace HappyTravel.PredictionService.Infrastructure
{
    public static class ElasticsearchHelper
    {
        public static bool TryGetIndex(Dictionary<string, string> indexes, Languages language, out string index)
        {
            index = string.Empty;
            
            return indexes.TryGetValue(LanguagesHelper.GetLanguageCode(language).ToLowerInvariant(), out index!);
        }

        
        public static bool TryGetIndex(Dictionary<string, string> indexes, string languageCode, out string index)
        {
            index = string.Empty;
            if (!LanguagesHelper.TryGetLanguage(languageCode, out var language))
                return false;

            return TryGetIndex(indexes, language, out index);
        }
    }
}