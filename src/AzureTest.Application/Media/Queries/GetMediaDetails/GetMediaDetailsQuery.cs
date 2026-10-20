using System;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Interfaces.Data.NoSql;
using AzureTest.Application.Interfaces.Services;
using AzureTest.Application.Media.Queries.GetWatchList;
using AzureTest.Application.Messaging;
using AzureTest.Contracts.DTOs.Media;
using AzureTest.Domain.Media.Enums;

namespace AzureTest.Application.Media.Queries.GetMediaDetails;

public sealed record GetMediaDetailsQuery(int TmdbId, MediaType Type, Guid UserId) : IQuery<Result<TmdbTitleDetailsDto>>;

public sealed class GetMediaDetailsQueryHandler : IQueryHandler<GetMediaDetailsQuery, Result<TmdbTitleDetailsDto>>
{
    private readonly ITmdbService _tmdbService;
    private readonly INoSqlUnitOfWork _uow;

    public GetMediaDetailsQueryHandler(ITmdbService tmdbService, INoSqlUnitOfWork uow)
    {
        _tmdbService = tmdbService;
        _uow = uow;
    }

    public async Task<Result<TmdbTitleDetailsDto>> HandleAsync(GetMediaDetailsQuery request, CancellationToken ct = default)
    {
        TmdbTitleDetailsDto? details = null;

        if (request.Type == MediaType.Movie)
        {
            details = await _tmdbService.GetMovieDetailsAsync(request.TmdbId, ct)
                      ?? await _tmdbService.GetTvShowDetailsAsync(request.TmdbId, ct);
        }
        else if (request.Type == MediaType.TvSeries || request.Type == MediaType.Anime)
        {
            details = await _tmdbService.GetTvShowDetailsAsync(request.TmdbId, ct)
                      ?? await _tmdbService.GetMovieDetailsAsync(request.TmdbId, ct);
        }
        else
        {
            details = await _tmdbService.GetTvShowDetailsAsync(request.TmdbId, ct)
                      ?? await _tmdbService.GetMovieDetailsAsync(request.TmdbId, ct);
        }

        if (details == null)
        {
            return Result.Failure<TmdbTitleDetailsDto>("Title not found on TMDb.");
        }

        var effectiveType = details.Type;

        // Check if user has this in their watchlist
        var userItem = await _uow.WatchItems.GetByUserAndTmdbAsync(request.UserId, request.TmdbId, effectiveType, ct);
        if (userItem == null && effectiveType == MediaType.Anime)
        {
            // Also check under TvSeries in case user saved as TvSeries
            userItem = await _uow.WatchItems.GetByUserAndTmdbAsync(request.UserId, request.TmdbId, MediaType.TvSeries, ct);
        }
        else if (userItem == null && effectiveType == MediaType.TvSeries)
        {
            // Also check under Anime in case user saved as Anime
            userItem = await _uow.WatchItems.GetByUserAndTmdbAsync(request.UserId, request.TmdbId, MediaType.Anime, ct);
        }

        WatchItemDto? userDto = userItem != null ? GetWatchListQuery.Mapping.ToDto(userItem) : null;

        var result = details with { UserWatchItem = userDto };
        return Result.Success(result);
    }
}
