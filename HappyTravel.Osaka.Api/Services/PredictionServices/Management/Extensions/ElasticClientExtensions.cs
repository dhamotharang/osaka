using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using HappyTravel.Osaka.Api.Models.Elastic;
using Nest;

namespace HappyTravel.Osaka.Api.Services.PredictionServices.Management.Extensions
{
    public static class ElasticClientExtensions
    {
        public static Task<BulkResponse> Add<T>(this IElasticClient elasticClient, List<T> items, string index, CancellationToken cancellationToken = default) where T : class, IElasticModel 
            => elasticClient.BulkAsync(b 
                => b.IndexMany(items), cancellationToken);

        
        public static Task<BulkResponse> Update<T>(this IElasticClient elasticClient, List<T> items, string index,
            CancellationToken cancellationToken = default) where T : class, IElasticModel 
            => elasticClient.BulkAsync(b => b.Index(index)
                    .UpdateMany(items, (bd, l) => bd.Id(l.Id).Doc(l)),
                cancellationToken);

        
        public static Task<BulkResponse> Delete<T>(this IElasticClient elasticClient, List<T> items,
            string index, CancellationToken cancellationToken = default) where T : class, IElasticModel 
            => elasticClient.BulkAsync(b => b.Index(index)
                    .DeleteMany(items, (bd, l) => bd.Document(l)),
                cancellationToken);
    }
}