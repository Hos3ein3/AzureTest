using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Interfaces.Data.NoSql;
using AzureTest.Application.Interfaces.Services;
using AzureTest.Application.Messaging;
using AzureTest.Contracts.DTOs.Media;
using AzureTest.Domain.Media.Enums;

namespace AzureTest.Application.Media.Queries.GetPersonDetails;

public sealed record GetPersonDetailsQuery(int TmdbPersonId, Guid UserId) : IQuery<Result<FollowedPersonDto>>;

public sealed class GetPersonDetailsQueryHandler : IQueryHandler<GetPersonDetailsQuery, Result<FollowedPersonDto>>
{
    private readonly ITmdbService _tmdbService;
    private readonly INoSqlUnitOfWork _uow;

    public GetPersonDetailsQueryHandler(ITmdbService tmdbService, INoSqlUnitOfWork uow)
    {
        _tmdbService = tmdbService;
        _uow = uow;
    }

    public async Task<Result<FollowedPersonDto>> HandleAsync(GetPersonDetailsQuery request, CancellationToken ct = default)
    {
        var details = await _tmdbService.GetPersonDetailsAsync(request.TmdbPersonId, ct);
        if (details == null)
        {
            return Result.Failure<FollowedPersonDto>("Person not found on TMDb.");
        }

        var followDoc = await _uow.FollowedPeople.GetByUserAndPersonAsync(request.UserId, request.TmdbPersonId, ct);

        var userWatchList = await _uow.WatchItems.GetUserWatchListAsync(request.UserId, WatchStatus.Completed, ct: ct);
        var watchedTmdbIds = userWatchList.Select(w => (w.TmdbId, w.Type)).ToHashSet();

        var enrichedFilmography = details.Filmography.Select(credit =>
        {
            var isWatched = watchedTmdbIds.Contains((credit.Id, credit.Type)) ||
                            (credit.Type == MediaType.TvSeries && watchedTmdbIds.Contains((credit.Id, MediaType.Anime)));
            return credit with { IsWatched = isWatched };
        }).ToList();

        var watchedCount = enrichedFilmography.Count(c => c.IsWatched);
        var totalCount = enrichedFilmography.Count;
        var progress = totalCount > 0 ? Math.Round((watchedCount / (double)totalCount) * 100.0, 1) : 0.0;

        var result = details with
        {
            Id = followDoc?.Id ?? Guid.Empty,
            FollowedAt = followDoc?.FollowedAt ?? DateTime.MinValue,
            Notes = followDoc?.Notes,
            WatchedCount = watchedCount,
            TotalCreditsCount = totalCount,
            ProgressPercentage = progress,
            Filmography = enrichedFilmography
        };

        return Result.Success(result);
    }
}
