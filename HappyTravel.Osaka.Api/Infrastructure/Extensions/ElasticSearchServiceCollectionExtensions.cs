using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using HappyTravel.MultiLanguage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nest;

namespace HappyTravel.Osaka.Api.Infrastructure.Extensions
{
    public static class ElasticSearchServiceCollectionExtensions
    {
        public static IServiceCollection AddElasticsearchClient(this IServiceCollection services,
            IConfiguration configuration, VaultClient.VaultClient vaultClient, Dictionary<string, string> indexes)
        {
            var clientSettings = vaultClient.Get(configuration["Elasticsearch:ClientSettings"]).GetAwaiter().GetResult();
            
            return services.AddSingleton<IElasticClient>(_ =>
            {
                ElasticsearchHelper.TryGetIndex(indexes, Languages.English, out var indexEn);
                ConnectionSettings connectionSettings;
                if (!EnvironmentVariableHelper.IsLocal())
                {
                    connectionSettings = new ConnectionSettings(new Uri(clientSettings["endpoint"]))
                        .BasicAuthentication(clientSettings["username"], clientSettings["password"])
                        .ServerCertificateValidationCallback((o, certificate, chain, errors) => true)
                        .ClientCertificate(
                            new X509Certificate2(Convert.FromBase64String(clientSettings["certificate"])))
                        .DefaultIndex(indexEn);
                }
                else
                {
                    connectionSettings =
                        new ConnectionSettings(new Uri(configuration["Elasticsearch:ClientSettings:Endpoint"]))
                            .DefaultIndex(configuration["Elasticsearch:Indexes:En"])
                            .EnableDebugMode();
                }

                if (clientSettings.TryGetValue("requestTimeoutInSeconds", out var requestTimeoutInSeconds))
                    connectionSettings.RequestTimeout(TimeSpan.FromSeconds(Convert.ToInt32(requestTimeoutInSeconds)));
                
                var client = new ElasticClient(connectionSettings);

                client.CreateIndexes(indexes).GetAwaiter().GetResult();

                return client;
            });
        }

        
        public static Task<CreateIndexResponse> CreateIndexes(this IElasticClient client, Dictionary<string, string> indexes)
        {
            ElasticsearchHelper.TryGetIndex(indexes, Languages.English, out var indexEn);
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
                    .Map<Models.Elasticsearch.Location>(mapping => mapping.Properties(properties =>
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
                                    .Contexts(context =>
                                        context.Category(category => category.Name("type").Path(location => location.LocationType))
                                            .Category(category => category.Name("country").Path(location => location.Country))
                                            .Category(category => category.Name("locality").Path(location => location.Locality))))
                                .Text(property => property.Name(prediction => prediction.PredictionText)
                                    .Analyzer("full_text_analyzer")
                                    .SearchAnalyzer("standard"))
                                .Keyword(property => property.Name(prediction => prediction.CountryCode))
                                .GeoPoint(property => property.Name(prediction => prediction.Coordinates))
                                .Number(property => property.Name(prediction => prediction.DistanceInMeters).Type(NumberType.Double))
                                .Keyword(property => property.Name(prediction => prediction.LocationType)))
                        .AutoMap()));
        }

        
        private static readonly string[] StopWords = {"the"};
    }
}