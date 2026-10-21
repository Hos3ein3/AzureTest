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

public sealed record GetMediaDetailsQuery(int TmdbId, MediaType? Type = null, Guid UserId = default, string? Slug = null) : IQuery<Result<TmdbTitleDetailsDto>>;

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

        static bool MatchesSlug(string? title, string? originalTitle, string? slug)
        {
            if (string.IsNullOrWhiteSpace(slug)) return false;
            var cleanSlug = System.Text.RegularExpressions.Regex.Replace(slug.ToLowerInvariant(), @"[^a-z0-9]+", "-").Trim('-');
            if (string.IsNullOrWhiteSpace(cleanSlug)) return false;

            bool Check(string? t)
            {
                if (string.IsNullOrWhiteSpace(t)) return false;
                var cleanTitle = System.Text.RegularExpressions.Regex.Replace(t.ToLowerInvariant(), @"[^a-z0-9]+", "-").Trim('-');
                return cleanTitle.Contains(cleanSlug) || cleanSlug.Contains(cleanTitle);
            }

            return Check(title) || Check(originalTitle);
        }

        if (!string.IsNullOrWhiteSpace(request.Slug))
        {
            // Disambiguate when TMDb has both a movie and a TV show with the same ID (e.g. 1429: Attack on Titan TV vs 25th Hour Movie)
            var tvTask = _tmdbService.GetTvShowDetailsAsync(request.TmdbId, ct);
            var movieTask = _tmdbService.GetMovieDetailsAsync(request.TmdbId, ct);
            await Task.WhenAll(tvTask, movieTask);

            var tv = tvTask.Result;
            var movie = movieTask.Result;

            if (tv != null && MatchesSlug(tv.Title, tv.OriginalTitle, request.Slug))
            {
                details = tv;
            }
            else if (movie != null && MatchesSlug(movie.Title, movie.OriginalTitle, request.Slug))
            {
                details = movie;
            }
            else if (request.Type.HasValue)
            {
                details = request.Type.Value == MediaType.Movie
                    ? (movie ?? tv)
                    : (tv ?? movie);
            }
            else
            {
                details = (tv != null && tv.Seasons.Count > 0) ? tv : (movie ?? tv);
            }
        }
        else if (request.Type.HasValue && request.Type.Value == MediaType.Movie)
        {
            details = await _tmdbService.GetMovieDetailsAsync(request.TmdbId, ct)
                      ?? await _tmdbService.GetTvShowDetailsAsync(request.TmdbId, ct);
        }
        else if (request.Type.HasValue && (request.Type.Value == MediaType.TvSeries || request.Type.Value == MediaType.Anime))
        {
            details = await _tmdbService.GetTvShowDetailsAsync(request.TmdbId, ct)
                      ?? await _tmdbService.GetMovieDetailsAsync(request.TmdbId, ct);
        }
        else
        {
            var tv = await _tmdbService.GetTvShowDetailsAsync(request.TmdbId, ct);
            if (tv != null && tv.Seasons.Count > 0)
            {
                details = tv;
            }
            else
            {
                details = await _tmdbService.GetMovieDetailsAsync(request.TmdbId, ct) ?? tv;
            }
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
        else if (userItem == null)
        {
            var allWatchList = await _uow.WatchItems.GetUserWatchListAsync(request.UserId, ct: ct);
            userItem = allWatchList.FirstOrDefault(w => w.TmdbId == request.TmdbId);
        }

        // Preload season episodes in parallel for immediate server-side rendering
        if (details.Seasons.Count > 0)
        {
            var validSeasons = details.Seasons.Where(s => s.SeasonNumber > 0).Take(12).ToList();
            var episodeTasks = validSeasons.Select(async s => new
            {
                s.SeasonNumber,
                Episodes = await _tmdbService.GetTvSeasonEpisodesAsync(details.Id, s.SeasonNumber, ct)
            });

            var episodeResults = await Task.WhenAll(episodeTasks);
            var episodeMap = episodeResults.ToDictionary(r => r.SeasonNumber, r => r.Episodes);

            var updatedSeasons = details.Seasons.Select(s =>
                episodeMap.TryGetValue(s.SeasonNumber, out var eps) && eps != null && eps.Count > 0
                    ? s with { Episodes = System.Linq.Enumerable.ToList(eps) }
                    : s).ToList();

            details = details with { Seasons = updatedSeasons };
        }

        WatchItemDto? userDto = userItem != null ? GetWatchListQuery.Mapping.ToDto(userItem) : null;

        var result = details with { UserWatchItem = userDto };
        return Result.Success(result);
    }
}
