using System;
using System.Security.Cryptography.X509Certificates;
using HappyTravel.LocationService.Models;
using HappyTravel.MultiLanguage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nest;

namespace HappyTravel.LocationService.Infrastructure.Extensions
{
    public static class ElasticSearchServiceCollectionExtensions
    {
        public static IServiceCollection AddElasticsearchClient(this IServiceCollection services,
            IConfiguration configuration, VaultClient.VaultClient vaultClient)
        {
            var clientSettings = vaultClient.Get(configuration["Elasticsearch:ClientSettings"]).GetAwaiter().GetResult();
            var indexes = vaultClient.Get(configuration["Elasticsearch:Indexes"]).GetAwaiter().GetResult();
            var defaultIndex = indexes[LanguagesHelper.GetLanguageCode(Languages.English)];
            
            return services.AddSingleton<IElasticClient>(p =>
            {
                var connectionSettings = new ConnectionSettings(new Uri(clientSettings["endpoint"]))
                    .BasicAuthentication(clientSettings["username"], clientSettings["password"])
                    .ServerCertificateValidationCallback((o, certificate, chain, errors) => true)
                    .ClientCertificate(new X509Certificate2(Convert.FromBase64String(clientSettings["certificate"])))
                    .DefaultIndex(defaultIndex);
                var client = new ElasticClient(connectionSettings);
                
                ConfigurePredictions(client, defaultIndex);
                
                return client;
            });
        }

        
        private static void ConfigurePredictions(IElasticClient client, string defaultIndex)
        {
            client.Indices.Create(defaultIndex,
                index => index
                    .Settings(settings => settings.Analysis(analysis
                        => analysis.TokenFilters(filter => filter.EdgeNGram("predictions_filter", tokenFilter => tokenFilter.MinGram(1).MaxGram(20)))
                            .Analyzers(analyzer => analyzer.Custom("predictions_analyzer",
                                predictionsAnalyzer => predictionsAnalyzer.Filters("lowercase", "asciifolding", "suggestion_filter").Tokenizer("standard")))))
                    .Map<Location>(mapping => mapping.Properties(properties
                            => properties.Keyword(property => property.Name(prediction => prediction.Id))
                                .Keyword(property => property.Name(prediction => prediction.Name))
                                .Keyword(property => property.Name(prediction => prediction.Locality))
                                .Keyword(property => property.Name(prediction => prediction.Country))
                                .Text(property => property.Name(prediction => prediction.PredictionText))
                                .GeoPoint(property => property.Name(prediction => prediction.Coordinates))
                                .Number(property => property.Name(prediction => prediction.DistanceInMeters).Type(NumberType.Double)))
                        .AutoMap()));
        }
    }
}
