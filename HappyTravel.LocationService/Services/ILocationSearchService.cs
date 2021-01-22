using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;

namespace HappyTravel.LocationService.Services
{
    public interface ILocationSearchService
    {
        Task<Result<List<Models.Elasticsearch.Location>>> Search(string query, int skip = 0, int top = 10, CancellationToken cancellationToken = default);
        Task<Result<Models.Elasticsearch.Location>> Get(string id, CancellationToken cancellationToken = default);
    }
}