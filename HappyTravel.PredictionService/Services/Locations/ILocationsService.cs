using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;

namespace HappyTravel.Osaka.Api.Services.Locations
{
    public interface ILocationsService
    {
        Task<List<Models.Elasticsearch.Location>> Search(string query, CancellationToken cancellationToken = default);
        Task<Result<Models.Elasticsearch.Location>> Get(string htId, CancellationToken cancellationToken = default);
    }
}