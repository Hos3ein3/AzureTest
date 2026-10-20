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

namespace AzureTest.Application.Media.Queries.GetFollowedPeople;

public sealed record GetFollowedPeopleQuery(Guid UserId) : IQuery<Result<IReadOnlyList<FollowedPersonDto>>>;

public sealed class GetFollowedPeopleQueryHandler : IQueryHandler<GetFollowedPeopleQuery, Result<IReadOnlyList<FollowedPersonDto>>>
{
    private readonly INoSqlUnitOfWork _uow;
    private readonly ITmdbService _tmdbService;

    public GetFollowedPeopleQueryHandler(INoSqlUnitOfWork uow, ITmdbService tmdbService)
    {
        _uow = uow;
        _tmdbService = tmdbService;
    }

    public async Task<Result<IReadOnlyList<FollowedPersonDto>>> HandleAsync(GetFollowedPeopleQuery request, CancellationToken ct = default)
    {
        var followed = await _uow.FollowedPeople.GetUserFollowedPeopleAsync(request.UserId, ct);
        if (followed.Count == 0)
        {
            return Result.Success<IReadOnlyList<FollowedPersonDto>>(Array.Empty<FollowedPersonDto>());
        }

        // Get user's completed watch items to compute "X of Y watched"
        var userWatchList = await _uow.WatchItems.GetUserWatchListAsync(request.UserId, WatchStatus.Completed, ct: ct);
        var watchedTmdbIds = userWatchList.Select(w => (w.TmdbId, w.Type)).ToHashSet();

        var list = new List<FollowedPersonDto>();

        foreach (var person in followed)
        {
            var details = await _tmdbService.GetPersonDetailsAsync(person.TmdbPersonId, ct);
            if (details == null)
            {
                list.Add(new FollowedPersonDto(
                    person.Id,
                    person.TmdbPersonId,
                    person.Name,
                    person.KnownForDepartment,
                    person.ProfilePath,
                    null,
                    null,
                    null,
                    person.FollowedAt,
                    person.Notes,
                    0,
                    0,
                    0,
                    new List<PersonCreditDto>(),
                    new List<PersonCreditDto>()
                ));
                continue;
            }

            // Mark watched status on filmography
            var enrichedFilmography = details.Filmography.Select(credit =>
            {
                var isWatched = watchedTmdbIds.Contains((credit.Id, credit.Type)) ||
                                (credit.Type == MediaType.TvSeries && watchedTmdbIds.Contains((credit.Id, MediaType.Anime)));
                return credit with { IsWatched = isWatched };
            }).ToList();

            var watchedCount = enrichedFilmography.Count(c => c.IsWatched);
            var totalCount = enrichedFilmography.Count;
            var progress = totalCount > 0 ? Math.Round((watchedCount / (double)totalCount) * 100.0, 1) : 0.0;

            list.Add(details with
            {
                Id = person.Id,
                FollowedAt = person.FollowedAt,
                Notes = person.Notes,
                WatchedCount = watchedCount,
                TotalCreditsCount = totalCount,
                ProgressPercentage = progress,
                Filmography = enrichedFilmography
            });
        }

        return Result.Success<IReadOnlyList<FollowedPersonDto>>(list);
    }
}
