using System.Collections.Generic;
using HappyTravel.MultiLanguage;
using HappyTravel.Osaka.Api.Options;

namespace HappyTravel.Osaka.Api.Infrastructure
{
    public static class ElasticHelper
    {
        public static bool TryGetCountriesIndex(Dictionary<string, Indexes> indexes, Languages language, out string index)
        {
            index = string.Empty;
            
            if (!GetIndexes(indexes, language, out var languageIndexes))
                return false;

            languageIndexes ??= new Indexes();

            index = languageIndexes.Countries;
            
            return true;
        }

        
        public static bool TryGetLocalitiesIndex(Dictionary<string, Indexes> indexes, Languages language, out string index)
        {
            index = string.Empty;
            
            if (!GetIndexes(indexes, language, out var languageIndexes))
                return false;

            languageIndexes ??= new Indexes();

            index = languageIndexes.Localities;
            
            return true;
        }
        
        
        public static bool TryGetAccommodationsIndex(Dictionary<string, Indexes> indexes, Languages language, out string index)
        {
            index = string.Empty;
            
            if (!GetIndexes(indexes, language, out var languageIndexes))
                return false;

            languageIndexes ??= new Indexes();

            index = languageIndexes.Localities;
            
            return true;
        }
        
        
        private static bool GetIndexes(Dictionary<string, Indexes> indexes, Languages language, out Indexes? languageIndexes)
        {
            languageIndexes = new Indexes();
            
            var languageCode = LanguagesHelper.GetLanguageCode(language).ToLowerInvariant();
            
            return indexes.TryGetValue(languageCode, out languageIndexes);
        }
    }
}