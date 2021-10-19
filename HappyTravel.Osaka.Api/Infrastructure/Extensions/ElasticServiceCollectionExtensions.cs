using System;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using HappyTravel.Osaka.Api.Models.Elastic;
using HappyTravel.Osaka.Api.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nest;

namespace HappyTravel.Osaka.Api.Infrastructure.Extensions
{
    public static class ElasticServiceCollectionExtensions
    {
        public static IServiceCollection ConfigureElastic(this IServiceCollection services, IConfiguration configuration, VaultClient.VaultClient vaultClient)
        {
            var englishIndexes = GetEnglishIndexes(configuration, vaultClient);
            
            services.Configure<IndexesOptions>(o =>
            {
                o.EnglishIndexes.Countries = englishIndexes.Countries;
                o.EnglishIndexes.Localities = englishIndexes.Localities;
                o.EnglishIndexes.Accommodations = englishIndexes.Accommodations;
            });

            var clientSettings = vaultClient.Get(configuration["Elasticsearch:ClientSettings"]).GetAwaiter().GetResult();
            
            return services.AddSingleton<IElasticClient>(_ =>
            {
                ConnectionSettings connectionSettings;
                if (!EnvironmentVariableHelper.IsLocal())
                {
                    connectionSettings = new ConnectionSettings(new Uri(clientSettings["endpoint"]))
                        .BasicAuthentication(clientSettings["username"], clientSettings["password"])
                        .ServerCertificateValidationCallback((o, certificate, chain, errors) => true)
                        .ClientCertificate(
                            new X509Certificate2(Convert.FromBase64String(clientSettings["certificate"])));
                }
                else
                {
                    connectionSettings =
                        new ConnectionSettings(new Uri(configuration["Elasticsearch:ClientSettings:Endpoint"]))
                            .EnableDebugMode();
                }

                if (clientSettings.TryGetValue("requestTimeoutInSeconds", out var requestTimeoutInSeconds))
                    connectionSettings.RequestTimeout(TimeSpan.FromSeconds(Convert.ToInt32(requestTimeoutInSeconds)));
                
                var client = new ElasticClient(connectionSettings);
            
                client.CreateEnglishIndexes(englishIndexes).GetAwaiter().GetResult();

                return client;
            });
        }

        /*
        public static Task<CreateIndexResponse> CreateIndexes(this IElasticClient client, Dictionary<string, string> indexes)
        {
            ElasticHelper.TryGetIndex(indexes, Languages.English, out var indexEn);
            var synonyms = ElasticSynonymsHelper.GetSynonyms();
            
            return client.Indices.CreateAsync(indexEn,
                index => index
                    .Settings(settings => settings.Analysis(analysis =>
                        analysis.TokenFilters(filter =>
                                filter.SynonymGraph("synonyms_filter", synonymsFilter => synonymsFilter.Tokenizer("standard").Lenient(false).Synonyms(synonyms))
                                    .Stop("stopwords_filter", stopWordsFilter => stopWordsFilter.StopWords(StopWords).IgnoreCase())
                                    .EdgeNGram("edge_ngram_filter", edgeNGramFilter => edgeNGramFilter.MinGram(1).MaxGram(20)))
                            .Analyzers(analyzer 
                                => analyzer.Custom("predictions_analyzer", predictionAnalyzer => predictionAnalyzer.Filters("lowercase", "asciifolding", "synonyms_filter", "stopwords_filter").Tokenizer("standard"))
                                    .Custom("full_text_analyzer", ngramAnalyzer => ngramAnalyzer.Filters("lowercase", "edge_ngram_filter", "asciifolding").Tokenizer("standard")))))
                    .Map<ElasticLocation>(mapping => mapping.Properties(properties =>
                            properties.Keyword(property => property.Name(prediction => prediction.Id))
                                .Keyword(completion => completion.Name(location => location.Country))
                                .Keyword(completion => completion.Name(location => location.Locality))
                                .Keyword(completion => completion.Name(location => location.Name))
                                .Completion(completion => completion.Name(location => location.Suggestion)
                                    .Analyzer("predictions_analyzer")
                                    .SearchAnalyzer("predictions_analyzer")
                                    .PreserveSeparators(false)
                                    .PreservePositionIncrements(false)
                                    .MaxInputLength(80)
                                    .Contexts(context 
                                        => context.Category(category => category.Name(ElasticContextCategoryNames.LocationType).Path(location => location.Type))
                                            .Category(category => category.Name(ElasticContextCategoryNames.IsPromotion).Path(location => location.IsPromotion))
                                            .Category(category => category.Name(ElasticContextCategoryNames.IsDirectContract).Path(location => location.IsDirectContract))
                                            .Category(category => category.Name(ElasticContextCategoryNames.IsConfirmed).Path(location => location.IsConfirmed))
                                            .Category(category => category.Name(ElasticContextCategoryNames.IsInDomesticZone).Path(location => location.IsInDomesticZone))))
                                .Text(property => property.Name(prediction => prediction.PredictionText)
                                    .Analyzer("full_text_analyzer")
                                    .SearchAnalyzer("standard"))
                                .GeoPoint(property => property.Name(prediction => prediction.Coordinates))
                                .Number(property => property.Name(prediction => prediction.NumberOfAccommodations).Type(NumberType.Integer))
                                .Boolean(property => property.Name(predictions => predictions.IsPromotion))
                                .Boolean(property => property.Name(predictions => predictions.IsDirectContract))
                                .Boolean(property => property.Name(predictions => predictions.IsConfirmed))
                                .Boolean(property => property.Name(predictions => predictions.IsInDomesticZone)))
                        .AutoMap()));
        }

        */
        private static Indexes GetEnglishIndexes(IConfiguration configuration, VaultClient.VaultClient vaultClient)
        {
            var indexes = vaultClient.Get(configuration["Elasticsearch:Indexes:En"]).GetAwaiter().GetResult();
            
            return new Indexes
            {
                Countries = indexes["countries"],
                Localities = indexes["localities"],
                Accommodations = indexes["accommodations"]
            };
        }
        

        public static async Task CreateEnglishIndexes(this IElasticClient client, Indexes indexes)
        {
            var countriesIndex = indexes.Countries;
            var localitiesIndex = indexes.Localities;
            var accommodationsIndex = indexes.Accommodations;

            await client.CreateCountriesIndex(countriesIndex);
            await client.CreateLocalitiesIndex(localitiesIndex);
            await client.CreateAccommodationsIndex(accommodationsIndex);
        }

        
        public static Task<CreateIndexResponse> CreateCountriesIndex(this IElasticClient client, string indexName)
        {
            var synonyms = ElasticSynonymsHelper.GetCountrySynonyms();
            
            return client.Indices.CreateAsync(indexName,
                index => index
                    .Settings(settings => settings.Analysis(analysis =>
                        analysis.TokenFilters(filter =>
                                filter.SynonymGraph("synonyms_filter", synonymsFilter => synonymsFilter.Tokenizer("standard").Lenient(false).Synonyms(synonyms))
                                    .Stop("stopwords_filter", stopWordsFilter => stopWordsFilter.StopWords(StopWords).IgnoreCase())
                                    .EdgeNGram("edge_ngram_filter", edgeNGramFilter => edgeNGramFilter.MinGram(1).MaxGram(15)))
                            .Analyzers(analyzer 
                                => analyzer.Custom("predictions_analyzer", predictionAnalyzer => predictionAnalyzer.Filters("lowercase", "asciifolding", "synonyms_filter", "stopwords_filter").Tokenizer("standard"))
                                    .Custom("full_text_analyzer", ngramAnalyzer => ngramAnalyzer.Filters("lowercase", "edge_ngram_filter", "asciifolding").Tokenizer("standard")))))
                    .Map<ElasticCountry>(mapping => mapping.Properties(properties =>
                            properties.Keyword(property => property.Name(country => country.Id))
                                .Keyword(property => property.Name(country => country.Name))
                                .Keyword(property => property.Name(country => country.Code))
                                .Date(property => property.Name(country => country.Modified))
                                .Number(property => property.Name(country => country.NumberOfAccommodations).Type(NumberType.Integer))
                                .Completion(countryDescriptor => countryDescriptor.Name(country => country.Suggestion)
                                    .Analyzer("predictions_analyzer")
                                    .SearchAnalyzer("predictions_analyzer")
                                    .PreserveSeparators(false)
                                    .PreservePositionIncrements(false)
                                    .MaxInputLength(80))
                                .Text(property => property.Name(country => country.PredictionText)
                                    .Analyzer("full_text_analyzer")
                                    .SearchAnalyzer("standard"))
                                .Boolean(property => property.Name(country => country.Enabled))
                                )
                        
                        .AutoMap()));
        }

        
        public static Task<CreateIndexResponse> CreateLocalitiesIndex(this IElasticClient client, string indexName)
        {
            var synonyms = ElasticSynonymsHelper.GetLocalitySynonyms();
            
            return client.Indices.CreateAsync(indexName,
                index => index
                    .Settings(settings => settings.Analysis(analysis =>
                        analysis.TokenFilters(filter =>
                                filter.SynonymGraph("synonyms_filter", synonymsFilter => synonymsFilter.Tokenizer("standard").Lenient(false).Synonyms(synonyms))
                                .Stop("stopwords_filter", stopWordsFilter => stopWordsFilter.StopWords(StopWords).IgnoreCase())
                                .EdgeNGram("edge_ngram_filter", edgeNGramFilter => edgeNGramFilter.MinGram(1).MaxGram(20)))
                            .Analyzers(analyzer => analyzer.Custom("predictions_analyzer", predictionAnalyzer => predictionAnalyzer.Filters("lowercase", "asciifolding", "synonyms_filter", "stopwords_filter").Tokenizer("standard"))
                                    .Custom("full_text_analyzer", ngramAnalyzer => ngramAnalyzer.Filters("lowercase", "edge_ngram_filter", "asciifolding").Tokenizer("standard")))))
                    .Map<ElasticLocality>(mapping => mapping.Properties(properties =>
                            properties.Keyword(property => property.Name(locality => locality.Id))
                                .Keyword(property => property.Name(locality => locality.Name))
                                .Keyword(property => property.Name(locality => locality.Country))
                                .Completion(completion => completion.Name(locality => locality.Suggestion)
                                    .Analyzer("predictions_analyzer")
                                    .SearchAnalyzer("predictions_analyzer")
                                    .PreserveSeparators(false)
                                    .PreservePositionIncrements(false)
                                    .MaxInputLength(80))
                                .Text(property => property.Name(locality => locality.PredictionText)
                                    .Analyzer("full_text_analyzer")
                                    .SearchAnalyzer("standard"))
                                .Boolean(property => property.Name(locality => locality.Enabled)))
                        .AutoMap()));
        }


        public static Task<CreateIndexResponse> CreateAccommodationsIndex(this IElasticClient client, string indexName)
        {
            return client.Indices.CreateAsync(indexName,
                index => index
                    .Settings(settings => settings.Analysis(analysis =>
                        analysis.TokenFilters(filter =>
                                filter.EdgeNGram("edge_ngram_filter", edgeNGramFilter => edgeNGramFilter.MinGram(1).MaxGram(20)))
                            .Analyzers(analyzer => analyzer.Custom("predictions_analyzer", predictionAnalyzer => predictionAnalyzer.Filters("lowercase", "asciifolding", "synonyms_filter", "stopwords_filter").Tokenizer("standard"))
                                    .Custom("full_text_analyzer", ngramAnalyzer => ngramAnalyzer.Filters("lowercase", "edge_ngram_filter", "asciifolding").Tokenizer("standard")))))
                    .Map<ElasticAccommodation>(mapping => mapping.Properties(properties =>
                            properties.Keyword(property => property.Name(prediction => prediction.Id))
                                .Keyword(completion => completion.Name(location => location.Country))
                                .Keyword(completion => completion.Name(location => location.Locality))
                                .Keyword(completion => completion.Name(location => location.Name))
                                .Completion(completion => completion.Name(location => location.Suggestion)
                                    .Analyzer("predictions_analyzer")
                                    .SearchAnalyzer("predictions_analyzer")
                                    .PreserveSeparators(false)
                                    .PreservePositionIncrements(false)
                                    .MaxInputLength(80)
                                    .Contexts(context 
                                        => context.Category(category => category.Name(AccommodationContextCategoryNames.IsPromotion).Path(accommodation => accommodation.IsPromotion))
                                            .Category(category => category.Name(AccommodationContextCategoryNames.IsDirectContract).Path(accommodation => accommodation.IsDirectContract))
                                            .Category(category => category.Name(AccommodationContextCategoryNames.IsConfirmed).Path(accommodation => accommodation.IsMappingConfirmed))
                                            .Category(category => category.Name(AccommodationContextCategoryNames.IsInDomesticZone).Path(accommodation => accommodation.IsInDomesticZone))))
                                .Text(property => property.Name(prediction => prediction.PredictionText)
                                    .Analyzer("full_text_analyzer")
                                    .SearchAnalyzer("standard"))
                                .GeoPoint(property => property.Name(accommodation => accommodation.Coordinates))
                                .Number(property => property.Name(accommodation => accommodation.NumberOfAccommodationsInLocality).Type(NumberType.Integer))
                                .Boolean(property => property.Name(accommodation => accommodation.IsPromotion))
                                .Boolean(property => property.Name(accommodation => accommodation.IsDirectContract))
                                .Boolean(property => property.Name(accommodation => accommodation.IsMappingConfirmed))
                                .Boolean(property => property.Name(accommodation => accommodation.IsInDomesticZone))
                                .Boolean(property => property.Name(accommodation => accommodation.Enabled)))
                        .AutoMap()));
        }
        
        
        private static readonly string[] StopWords = {"the"};
    }
}