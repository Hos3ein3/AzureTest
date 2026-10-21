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

namespace AzureTest.Application.Media.Queries.GetSmartRecommendations;

public sealed record GetSmartRecommendationsQuery(Guid UserId) : IQuery<Result<MediaRecommendationsDto>>;

public sealed class GetSmartRecommendationsQueryHandler : IQueryHandler<GetSmartRecommendationsQuery, Result<MediaRecommendationsDto>>
{
    private readonly INoSqlUnitOfWork _uow;
    private readonly ITmdbService _tmdbService;

    public GetSmartRecommendationsQueryHandler(INoSqlUnitOfWork uow, ITmdbService tmdbService)
    {
        _uow = uow;
        _tmdbService = tmdbService;
    }

    public async Task<Result<MediaRecommendationsDto>> HandleAsync(GetSmartRecommendationsQuery request, CancellationToken ct = default)
    {
        var allWatchItems = await _uow.WatchItems.GetUserWatchListAsync(request.UserId, ct: ct);
        var followedPeople = await _uow.FollowedPeople.GetUserFollowedPeopleAsync(request.UserId, ct);

        var completedOrDropped = allWatchItems
            .Where(w => w.Status == WatchStatus.Completed || w.Status == WatchStatus.Dropped)
            .Select(w => (w.TmdbId, w.Type))
            .ToHashSet();

        var becauseRewatched = new List<RecommendedGroupDto>();
        var starringFollowed = new List<RecommendedGroupDto>();
        var similarWatched = new List<RecommendedGroupDto>();
        var topPicks = new List<TmdbSearchResultDto>();
        var seenPickIds = new HashSet<(int, MediaType)>();

        // 1. Recommendations based on Rewatched Titles
        var rewatched = allWatchItems.Where(w => w.RewatchCount > 0)
            .OrderByDescending(w => w.RewatchCount)
            .ThenByDescending(w => w.LastWatchedAt ?? w.WatchedDate ?? w.CreatedAt)
            .Take(3)
            .ToList();

        if (!rewatched.Any())
        {
            // Fallback to top-rated completed favorites
            rewatched = allWatchItems.Where(w => w.Status == WatchStatus.Completed && (w.UserRating ?? 0) >= 8)
                .OrderByDescending(w => w.UserRating)
                .Take(2)
                .ToList();
        }

        foreach (var item in rewatched)
        {
            try
            {
                var similar = await _tmdbService.GetSimilarTitlesAsync(item.TmdbId, item.Type, ct);
                var filtered = similar
                    .Where(s => !completedOrDropped.Contains((s.Id, s.Type)))
                    .Take(8)
                    .ToList();

                if (filtered.Any())
                {
                    var subtitle = item.RewatchCount > 0 
                        ? $"Because you rewatched \"{item.Title}\" ({item.RewatchCount + 1}x)"
                        : $"Because you loved \"{item.Title}\" (Rated {item.UserRating}/10)";

                    becauseRewatched.Add(new RecommendedGroupDto(
                        item.Title,
                        subtitle,
                        item.RewatchCount > 0 ? $"{item.RewatchCount}x Rewatched" : "Favorite",
                        filtered
                    ));

                    foreach (var f in filtered)
                    {
                        if (seenPickIds.Add((f.Id, f.Type)))
                            topPicks.Add(f);
                    }
                }
            }
            catch
            {
                // Ignore individual TMDb call issues
            }
        }

        // 2. Recommendations based on Followed Actors / Directors
        foreach (var person in followedPeople.Take(4))
        {
            try
            {
                var details = await _tmdbService.GetPersonDetailsAsync(person.TmdbPersonId, ct);
                if (details?.Filmography != null && details.Filmography.Any())
                {
                    var mapped = details.Filmography
                        .Where(k => !completedOrDropped.Contains((k.Id, k.Type)))
                        .OrderByDescending(k => k.VoteAverage ?? 0)
                        .Take(8)
                        .Select(k => new TmdbSearchResultDto(
                            Id: k.Id,
                            Title: k.Title,
                            Type: k.Type,
                            PosterPath: k.PosterPath,
                            BackdropPath: null,
                            ReleaseDate: k.ReleaseDate,
                            VoteAverage: k.VoteAverage,
                            Overview: null
                        ))
                        .ToList();

                    if (mapped.Any())
                    {
                        starringFollowed.Add(new RecommendedGroupDto(
                            person.Name,
                            $"Starring or directed by followed talent {person.Name} ({person.KnownForDepartment})",
                            person.KnownForDepartment,
                            mapped
                        ));

                        foreach (var f in mapped)
                        {
                            if (seenPickIds.Add((f.Id, f.Type)))
                                topPicks.Add(f);
                        }
                    }
                }
            }
            catch
            {
                // Ignore
            }
        }

        // 3. Recommendations based on Recently Watched or Currently Watching
        var recentlyWatched = allWatchItems
            .Where(w => w.Status == WatchStatus.Watching || w.Status == WatchStatus.Completed)
            .OrderByDescending(w => w.LastWatchedAt ?? w.WatchedDate ?? w.CreatedAt)
            .Take(3)
            .ToList();

        foreach (var item in recentlyWatched)
        {
            // Avoid duplicate seed title from rewatched section
            if (rewatched.Any(r => r.TmdbId == item.TmdbId && r.Type == item.Type))
                continue;

            try
            {
                var similar = await _tmdbService.GetSimilarTitlesAsync(item.TmdbId, item.Type, ct);
                var filtered = similar
                    .Where(s => !completedOrDropped.Contains((s.Id, s.Type)))
                    .Take(8)
                    .ToList();

                if (filtered.Any())
                {
                    similarWatched.Add(new RecommendedGroupDto(
                        item.Title,
                        $"Similar to your recent watch \"{item.Title}\"",
                        item.Status == WatchStatus.Watching ? "Currently Watching" : "Recently Watched",
                        filtered
                    ));

                    foreach (var f in filtered)
                    {
                        if (seenPickIds.Add((f.Id, f.Type)))
                            topPicks.Add(f);
                    }
                }
            }
            catch
            {
                // Ignore
            }
        }

        // 4. If top picks are sparse, pull top recommendations from TMDb Discover
        if (topPicks.Count < 12)
        {
            try
            {
                var discover = await _tmdbService.GetDiscoverAsync(ct);
                var extra = discover.Recommendations.Concat(discover.TrendingToday)
                    .Where(d => !completedOrDropped.Contains((d.Id, d.Type)))
                    .ToList();

                foreach (var ex in extra)
                {
                    if (seenPickIds.Add((ex.Id, ex.Type)))
                        topPicks.Add(ex);

                    if (topPicks.Count >= 18)
                        break;
                }
            }
            catch
            {
                // Ignore
            }
        }

        var result = new MediaRecommendationsDto(
            BecauseYouRewatched: becauseRewatched,
            StarringFollowedTalent: starringFollowed,
            SimilarToWatched: similarWatched,
            TopPicksForYou: topPicks.Take(18).ToList()
        );

        return Result.Success(result);
    }
}
