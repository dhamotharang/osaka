using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using HappyTravel.MultiLanguage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nest;

namespace HappyTravel.LocationService.Infrastructure.Extensions
{
    public static class ElasticSearchServiceCollectionExtensions
    {
        public static IServiceCollection AddElasticsearchClient(this IServiceCollection services,
            IConfiguration configuration, VaultClient.VaultClient vaultClient, Dictionary<string, string> indexes)
        {
            var clientSettings = vaultClient.Get(configuration["Elasticsearch:ClientSettings"]).GetAwaiter().GetResult();
            
            return services.AddSingleton<IElasticClient>(p =>
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
                            .DefaultIndex(configuration["Elasticsearch:Indexes:En"]);
                }

                var client = new ElasticClient(connectionSettings);
                
                ConfigurePredictions(client, indexes);
                
                return client;
            });
        }

        
        private static void ConfigurePredictions(IElasticClient client, Dictionary<string, string> indexes)
        {
            InitializeEnglishIndex();

            void InitializeEnglishIndex()
            {
                ElasticsearchHelper.TryGetIndex(indexes, Languages.English, out var indexEn);

                client.Indices.Create(indexEn,
                    index => index
                        .Settings(settings => settings.Analysis(analysis =>
                            analysis
                                .TokenFilters(filter => filter.EdgeNGram("predictions_filter",
                                    tokenFilter => tokenFilter.MinGram(1).MaxGram(20)))
                                .Analyzers(analyzer => analyzer.Custom("predictions_analyzer",
                                    predictionsAnalyzer =>
                                        predictionsAnalyzer.Filters("lowercase", "asciifolding", "predictions_filter")
                                            .Tokenizer("standard")))))
                        .Map<Models.Elasticsearch.Location>(mapping => mapping.Properties(properties =>
                                properties.Keyword(property => property.Name(prediction => prediction.Id))
                                    .Keyword(property => property.Name(prediction => prediction.Name))
                                    .Keyword(property => property.Name(prediction => prediction.Locality))
                                    .Keyword(property => property.Name(prediction => prediction.Country))
                                    .Text(property => property.Name(prediction => prediction.PredictionText))
                                    .GeoPoint(property => property.Name(prediction => prediction.Coordinates))
                                    .Number(property =>
                                        property.Name(prediction => prediction.DistanceInMeters)
                                            .Type(NumberType.Double)))
                            .AutoMap()));
            }
        }
    }
}
