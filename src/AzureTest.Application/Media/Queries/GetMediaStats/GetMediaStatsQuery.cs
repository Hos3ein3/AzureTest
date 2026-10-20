using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Interfaces.Data.NoSql;
using AzureTest.Application.Messaging;
using AzureTest.Contracts.DTOs.Media;
using AzureTest.Domain.Media.Enums;

namespace AzureTest.Application.Media.Queries.GetMediaStats;

public sealed record GetMediaStatsQuery(Guid UserId) : IQuery<Result<MediaStatsDto>>;

public sealed class GetMediaStatsQueryHandler : IQueryHandler<GetMediaStatsQuery, Result<MediaStatsDto>>
{
    private readonly INoSqlUnitOfWork _uow;

    public GetMediaStatsQueryHandler(INoSqlUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<Result<MediaStatsDto>> HandleAsync(GetMediaStatsQuery request, CancellationToken ct = default)
    {
        var allItems = await _uow.WatchItems.GetUserWatchListAsync(request.UserId, ct: ct);
        var followed = await _uow.FollowedPeople.GetUserFollowedPeopleAsync(request.UserId, ct);

        int totalEpisodes = 0;
        int totalMovies = 0;
        int totalSeries = 0;
        int totalAnime = 0;
        int totalWatchedMinutes = 0;

        var genreCounts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        var ratings = new List<int>();
        var statusCounts = new Dictionary<WatchStatus, int>
        {
            [WatchStatus.PlanToWatch] = 0,
            [WatchStatus.Watching] = 0,
            [WatchStatus.Completed] = 0,
            [WatchStatus.Dropped] = 0
        };

        var ratingDistribution = new Dictionary<int, int>();
        for (int i = 1; i <= 10; i++)
        {
            ratingDistribution[i] = 0;
        }

        foreach (var item in allItems)
        {
            statusCounts[item.Status]++;

            if (item.UserRating.HasValue)
            {
                ratings.Add(item.UserRating.Value);
                if (ratingDistribution.ContainsKey(item.UserRating.Value))
                {
                    ratingDistribution[item.UserRating.Value]++;
                }
            }

            if (item.Status == WatchStatus.Completed || item.Status == WatchStatus.Watching)
            {
                totalWatchedMinutes += item.GetEstimatedWatchedMinutes();

                if (item.Type == MediaType.Movie)
                {
                    if (item.Status == WatchStatus.Completed) totalMovies++;
                }
                else if (item.Type == MediaType.Anime)
                {
                    totalAnime++;
                    totalEpisodes += item.CurrentEpisode;
                }
                else
                {
                    totalSeries++;
                    totalEpisodes += item.CurrentEpisode;
                }

                foreach (var g in item.Genres)
                {
                    genreCounts[g] = genreCounts.GetValueOrDefault(g, 0) + 1;
                }
            }
        }

        var totalItemsWatched = totalMovies + totalSeries + totalAnime;
        var totalHours = totalWatchedMinutes / 60;
        var avgRating = ratings.Count > 0 ? Math.Round(ratings.Average(), 1) : 0.0;

        var totalGenreHits = genreCounts.Values.Sum();
        var topGenres = genreCounts
            .OrderByDescending(kv => kv.Value)
            .Take(8)
            .Select(kv => new GenreStatDto(
                kv.Key,
                kv.Value,
                totalGenreHits > 0 ? Math.Round((kv.Value / (double)totalGenreHits) * 100.0, 1) : 0.0
            ))
            .ToList();

        var topActors = followed
            .Take(6)
            .Select(p => new PersonStatDto(p.Name, p.KnownForDepartment, 0))
            .ToList();

        var statusBreakdown = new List<StatusCountDto>
        {
            new("Watching", statusCounts[WatchStatus.Watching], "#3B82F6"),
            new("Finished", statusCounts[WatchStatus.Completed], "#10B981"),
            new("Wishlist", statusCounts[WatchStatus.PlanToWatch], "#F59E0B"),
            new("Stopped", statusCounts[WatchStatus.Dropped], "#EF4444")
        };

        var typeBreakdown = new List<TypeCountDto>
        {
            new("Movies", totalMovies, totalMovies * 110 / 60),
            new("Series", totalSeries, (totalEpisodes * 45) / 60),
            new("Anime", totalAnime, (totalAnime * 24) / 60)
        };

        var stats = new MediaStatsDto(
            totalItemsWatched,
            totalWatchedMinutes,
            totalHours,
            totalEpisodes,
            totalMovies,
            totalSeries,
            totalAnime,
            avgRating,
            topGenres,
            topActors,
            statusBreakdown,
            typeBreakdown,
            ratingDistribution
        );

        return Result.Success(stats);
    }
}
