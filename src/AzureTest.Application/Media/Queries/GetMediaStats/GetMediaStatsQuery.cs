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
        int movieMinutes = 0;
        int seriesMinutes = 0;
        int animeMinutes = 0;

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
            var status = item.Status;
            if (!statusCounts.ContainsKey(status) || (int)status == 0)
            {
                status = WatchStatus.PlanToWatch;
            }
            statusCounts[status]++;

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
                var itemMinutes = item.GetEstimatedWatchedMinutes();
                totalWatchedMinutes += itemMinutes;

                var fullRunEpisodes = item.TotalEpisodes > 0 ? item.TotalEpisodes : item.CurrentEpisode;
                var episodesWatched = (fullRunEpisodes * item.RewatchCount) +
                    ((item.Status == WatchStatus.Completed && item.TotalEpisodes > 0)
                        ? Math.Max(item.CurrentEpisode, item.TotalEpisodes)
                        : item.CurrentEpisode);

                var isMovie = item.Type == MediaType.Movie || (item.Type == MediaType.Anime && item.TotalEpisodes <= 1 && item.TotalSeasons <= 1);

                if (item.Type == MediaType.Movie)
                {
                    if (item.Status == WatchStatus.Completed) totalMovies++;
                    movieMinutes += itemMinutes;
                }
                else if (item.Type == MediaType.Anime)
                {
                    totalAnime++;
                    if (isMovie)
                    {
                        if (item.Status == WatchStatus.Completed) totalMovies++;
                    }
                    else
                    {
                        totalEpisodes += episodesWatched;
                    }
                    animeMinutes += itemMinutes;
                }
                else
                {
                    totalSeries++;
                    totalEpisodes += episodesWatched;
                    seriesMinutes += itemMinutes;
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
            new("Watching", statusCounts.GetValueOrDefault(WatchStatus.Watching, 0), "#3B82F6"),
            new("Finished", statusCounts.GetValueOrDefault(WatchStatus.Completed, 0), "#10B981"),
            new("Wishlist", statusCounts.GetValueOrDefault(WatchStatus.PlanToWatch, 0), "#F59E0B"),
            new("Stopped", statusCounts.GetValueOrDefault(WatchStatus.Dropped, 0), "#EF4444")
        };

        var typeBreakdown = new List<TypeCountDto>
        {
            new("Movies", totalMovies, movieMinutes / 60),
            new("Series", totalSeries, seriesMinutes / 60),
            new("Anime", totalAnime, animeMinutes / 60)
        };

        // Rewatch Analytics
        var rewatchedItems = allItems.Where(i => i.RewatchCount > 0).ToList();
        var totalRewatchedTitles = rewatchedItems.Count;
        var totalRewatchCount = rewatchedItems.Sum(i => i.RewatchCount);
        int totalRewatchMinutes = 0;

        foreach (var item in rewatchedItems)
        {
            var isMovie = item.Type == MediaType.Movie || (item.Type == MediaType.Anime && item.TotalEpisodes <= 1 && item.TotalSeasons <= 1);
            if (isMovie)
            {
                var duration = (item.RuntimeMinutes.HasValue && item.RuntimeMinutes.Value > 0)
                    ? item.RuntimeMinutes.Value
                    : AzureTest.Domain.Documents.Media.WatchItemDocument.Rules.DefaultMovieDurationMinutes;
                totalRewatchMinutes += duration * item.RewatchCount;
            }
            else
            {
                var epDuration = (item.RuntimeMinutes.HasValue && item.RuntimeMinutes.Value > 0)
                    ? item.RuntimeMinutes.Value
                    : (item.Type == MediaType.Anime
                        ? AzureTest.Domain.Documents.Media.WatchItemDocument.Rules.DefaultAnimeEpisodeDurationMinutes
                        : AzureTest.Domain.Documents.Media.WatchItemDocument.Rules.DefaultEpisodeDurationMinutes);
                var fullEpisodes = item.TotalEpisodes > 0 ? item.TotalEpisodes : item.CurrentEpisode;
                totalRewatchMinutes += (fullEpisodes * item.RewatchCount) * epDuration;
            }
        }

        var mostRewatchedTitles = rewatchedItems
            .OrderByDescending(i => i.RewatchCount)
            .Take(8)
            .Select(i => {
                var estMinutes = i.GetEstimatedWatchedMinutes();
                return new RewatchedTitleDto(
                    i.TmdbId,
                    i.Type,
                    i.Title,
                    i.PosterPath,
                    i.RewatchCount,
                    i.RewatchCount + 1,
                    estMinutes / 60
                );
            })
            .ToList();

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
            ratingDistribution,
            totalRewatchCount,
            totalRewatchedTitles,
            totalRewatchMinutes / 60,
            mostRewatchedTitles
        );

        return Result.Success(stats);
    }
}
