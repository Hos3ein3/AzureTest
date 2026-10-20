using System;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Interfaces.Services;
using AzureTest.Application.Messaging;
using AzureTest.Contracts.DTOs.Media;

namespace AzureTest.Application.Media.Queries.GetDiscoverMedia;

public sealed record GetDiscoverMediaQuery(Guid UserId) : IQuery<Result<TmdbDiscoverDto>>;

public sealed class GetDiscoverMediaQueryHandler : IQueryHandler<GetDiscoverMediaQuery, Result<TmdbDiscoverDto>>
{
    private readonly ITmdbService _tmdbService;

    public GetDiscoverMediaQueryHandler(ITmdbService tmdbService)
    {
        _tmdbService = tmdbService;
    }

    public async Task<Result<TmdbDiscoverDto>> HandleAsync(GetDiscoverMediaQuery request, CancellationToken ct = default)
    {
        var discover = await _tmdbService.GetDiscoverAsync(ct);
        return Result.Success(discover);
    }
}
