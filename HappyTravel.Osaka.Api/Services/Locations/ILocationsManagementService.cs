﻿using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Osaka.Api.Models;

namespace HappyTravel.Osaka.Api.Services.Locations
{
    public interface ILocationsManagementService
    {
        Task<Result<int>> ReUpload(CancellationToken cancellationToken = default);
        Task<Result> Add(List<Location> locations, string index, CancellationToken cancellationToken = default);
        Task<Result> Add(Location location, CancellationToken cancellationToken = default);
        Task<Result> Update(List<Location> locations, string index, CancellationToken cancellationToken = default);
        Task<Result> Update(Location location, string index, CancellationToken cancellationToken = default);
        Task<Result> Remove(List<Location> locations, string index, CancellationToken cancellationToken = default);
        Task<Result> Remove(Location location, string index, CancellationToken cancellationToken = default);
    }
}