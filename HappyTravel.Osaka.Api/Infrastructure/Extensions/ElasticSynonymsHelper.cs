using System.Collections.Generic;
using System.Linq;

namespace HappyTravel.Osaka.Api.Infrastructure.Extensions
{
    public static class ElasticSynonymsHelper
    {
        public static IEnumerable<string> GetSynonyms()
        {
            var locationNameRetriever = new LocationNameNormalizer.FileLocationNameRetriever();
            var countries = locationNameRetriever.RetrieveCountries();
            
            var countrySynonyms = countries.Select(c => FilterSynonyms(c.Name.Variants))
                .Where(IsNotOneWordList)
                .Select(CreateSynonym);

            var localitySynonyms = countries
                .Where(c => c.Localities != null)
                .SelectMany(c => c.Localities)
                .Select(l => FilterSynonyms(l.Name.Variants))
                .Where(IsNotOneWordList)
                .Select(CreateSynonym);

            return countrySynonyms.Concat(localitySynonyms);
        }

        
        private static IEnumerable<string> FilterSynonyms(IEnumerable<string> names)
            => names.Select(RemoveArticles).Distinct();

        
        private static bool IsNotOneWordList(IEnumerable<string> synonyms) => synonyms.Count() > 1;
        
        
        private static string RemoveArticles(string origin)
        {
            var articles = new []{"the", "a", "an"};
            
            foreach (var article in articles)
            {
                if (origin.Length <= article.Length)
                    return origin;
            
                var originArticle = origin.Substring(0, article.Length).ToLowerInvariant();
            
                if (originArticle.Equals(article))
                    return origin.Substring(article.Length + 1);
            }

            return origin;
        }
        
        
        private static string CreateSynonym(IEnumerable<string> names) 
            => $"{string.Join(", ", names.Skip(1))} => {names.First()}".ToLowerInvariant();
    }
}