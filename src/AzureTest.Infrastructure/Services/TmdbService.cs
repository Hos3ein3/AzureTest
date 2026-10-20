using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Interfaces.Services;
using AzureTest.Contracts.DTOs.Media;
using AzureTest.Domain.Media.Enums;
using AzureTest.Infrastructure.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TMDbLib.Client;
using TMDbLib.Objects.Movies;
using TMDbLib.Objects.People;
using TMDbLib.Objects.Search;
using TMDbLib.Objects.Trending;
using TMDbLib.Objects.TvShows;
using TMDbGeneral = TMDbLib.Objects.General;
using MediaType = AzureTest.Domain.Media.Enums.MediaType;

namespace AzureTest.Infrastructure.Services;

public class TmdbService : ITmdbService
{
    private readonly TMDbClient _client;
    private readonly ILogger<TmdbService> _logger;
    private const int AnimationGenreId = 16;
    private const string JapaneseLanguage = "ja";
    private const string JapanCountry = "JP";

    public TmdbService(IOptions<TmdbSettings> settings, ILogger<TmdbService> logger)
    {
        _logger = logger;
        var apiKey = settings.Value?.ApiKey;
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            apiKey = "9d76c7f33e27966e3ecf072ca8efd73d"; // Fallback to provided key
        }

        _client = new TMDbClient(apiKey);
    }

    public async Task<IReadOnlyList<TmdbSearchResultDto>> SearchTitlesAsync(string query, int page = 1, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return Array.Empty<TmdbSearchResultDto>();
        }

        try
        {
            var movieTask = _client.SearchMovieAsync(query, page: page, cancellationToken: ct);
            var tvTask = _client.SearchTvShowAsync(query, page: page, cancellationToken: ct);

            await Task.WhenAll(movieTask, tvTask);

            var list = new List<TmdbSearchResultDto>();

            if (movieTask.Result?.Results != null)
            {
                foreach (var m in movieTask.Result.Results)
                {
                    var isAnime = (m.GenreIds != null && m.GenreIds.Contains(AnimationGenreId)) &&
                                  (string.Equals(m.OriginalLanguage, JapaneseLanguage, StringComparison.OrdinalIgnoreCase));

                    list.Add(new TmdbSearchResultDto(
                        m.Id,
                        m.Title ?? "Untitled Movie",
                        isAnime ? MediaType.Anime : MediaType.Movie,
                        m.PosterPath,
                        m.BackdropPath,
                        m.ReleaseDate,
                        m.VoteAverage,
                        m.Overview
                    ));
                }
            }

            if (tvTask.Result?.Results != null)
            {
                foreach (var t in tvTask.Result.Results)
                {
                    var isAnime = (t.GenreIds != null && t.GenreIds.Contains(AnimationGenreId)) &&
                                  (string.Equals(t.OriginalLanguage, JapaneseLanguage, StringComparison.OrdinalIgnoreCase) ||
                                   (t.OriginCountry != null && t.OriginCountry.Contains(JapanCountry)));

                    list.Add(new TmdbSearchResultDto(
                        t.Id,
                        t.Name ?? "Untitled Series",
                        isAnime ? MediaType.Anime : MediaType.TvSeries,
                        t.PosterPath,
                        t.BackdropPath,
                        t.FirstAirDate,
                        t.VoteAverage,
                        t.Overview
                    ));
                }
            }

            return list.OrderByDescending(x => x.VoteAverage ?? 0).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to search TMDb for '{Query}'", query);
            return Array.Empty<TmdbSearchResultDto>();
        }
    }

    public async Task<IReadOnlyList<TmdbSearchResultDto>> SearchPeopleAsync(string query, int page = 1, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return Array.Empty<TmdbSearchResultDto>();
        }

        try
        {
            var response = await _client.SearchPersonAsync(query, page: page, cancellationToken: ct);
            if (response?.Results == null) return Array.Empty<TmdbSearchResultDto>();

            var list = new List<TmdbSearchResultDto>();
            foreach (var p in response.Results)
            {
                list.Add(new TmdbSearchResultDto(
                    p.Id,
                    p.Name ?? "Unknown Person",
                    MediaType.Movie,
                    p.ProfilePath,
                    null,
                    null,
                    p.Popularity,
                    null,
                    "Acting",
                    IsPerson: true
                ));
            }

            return list;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to search people in TMDb for '{Query}'", query);
            return Array.Empty<TmdbSearchResultDto>();
        }
    }

    public async Task<TmdbTitleDetailsDto?> GetMovieDetailsAsync(int movieId, CancellationToken ct = default)
    {
        try
        {
            var movie = await _client.GetMovieAsync(movieId,
                MovieMethods.Credits | MovieMethods.Videos | MovieMethods.Similar | MovieMethods.WatchProviders,
                cancellationToken: ct);

            if (movie == null) return null;

            var isAnime = movie.Genres != null &&
                          movie.Genres.Any(g => g.Id == AnimationGenreId) &&
                          string.Equals(movie.OriginalLanguage, JapaneseLanguage, StringComparison.OrdinalIgnoreCase);

            var trailer = movie.Videos?.Results?
                .FirstOrDefault(v => string.Equals(v.Site, "YouTube", StringComparison.OrdinalIgnoreCase) &&
                                     (string.Equals(v.Type, "Trailer", StringComparison.OrdinalIgnoreCase) ||
                                      string.Equals(v.Type, "Teaser", StringComparison.OrdinalIgnoreCase)))?.Key;

            var cast = movie.Credits?.Cast?
                .Take(15)
                .Select(c => new CastMemberDto(c.Id, c.Name, c.Character, c.ProfilePath, c.Order))
                .ToList() ?? new List<CastMemberDto>();

            var directors = movie.Credits?.Crew?
                .Where(c => string.Equals(c.Job, "Director", StringComparison.OrdinalIgnoreCase))
                .Select(c => new CrewMemberDto(c.Id, c.Name, c.Job, c.Department, c.ProfilePath))
                .ToList() ?? new List<CrewMemberDto>();

            var watchProviders = ExtractWatchProviders(movie.WatchProviders);

            var similar = movie.Similar?.Results?
                .Take(6)
                .Select(m => new TmdbSearchResultDto(
                    m.Id,
                    m.Title ?? "Untitled",
                    MediaType.Movie,
                    m.PosterPath,
                    m.BackdropPath,
                    m.ReleaseDate,
                    m.VoteAverage,
                    m.Overview))
                .ToList() ?? new List<TmdbSearchResultDto>();

            return new TmdbTitleDetailsDto(
                movie.Id,
                movie.Title ?? "Untitled Movie",
                movie.OriginalTitle,
                isAnime ? MediaType.Anime : MediaType.Movie,
                movie.PosterPath,
                movie.BackdropPath,
                movie.Overview,
                movie.ReleaseDate,
                movie.Status,
                movie.VoteAverage,
                movie.VoteCount,
                movie.Runtime,
                movie.Genres?.Where(g => !string.IsNullOrEmpty(g.Name)).Select(g => g.Name!).ToList() ?? new List<string>(),
                trailer,
                cast,
                directors,
                new List<TvSeasonSummaryDto>(),
                watchProviders,
                similar,
                null
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get movie details for {MovieId}", movieId);
            return null;
        }
    }

    public async Task<TmdbTitleDetailsDto?> GetTvShowDetailsAsync(int tvShowId, CancellationToken ct = default)
    {
        try
        {
            var tv = await _client.GetTvShowAsync(tvShowId,
                TvShowMethods.Credits | TvShowMethods.Videos | TvShowMethods.Similar | TvShowMethods.WatchProviders,
                cancellationToken: ct);

            if (tv == null) return null;

            var isAnime = (tv.Genres != null && tv.Genres.Any(g => g.Id == AnimationGenreId)) &&
                          (string.Equals(tv.OriginalLanguage, JapaneseLanguage, StringComparison.OrdinalIgnoreCase) ||
                           (tv.OriginCountry != null && tv.OriginCountry.Contains(JapanCountry)));

            var trailer = tv.Videos?.Results?
                .FirstOrDefault(v => string.Equals(v.Site, "YouTube", StringComparison.OrdinalIgnoreCase) &&
                                     (string.Equals(v.Type, "Trailer", StringComparison.OrdinalIgnoreCase) ||
                                      string.Equals(v.Type, "Teaser", StringComparison.OrdinalIgnoreCase)))?.Key;

            var cast = tv.Credits?.Cast?
                .Take(15)
                .Select(c => new CastMemberDto(c.Id, c.Name, c.Character, c.ProfilePath, c.Order))
                .ToList() ?? new List<CastMemberDto>();

            var directors = tv.CreatedBy?
                .Select(c => new CrewMemberDto(c.Id, c.Name, "Creator", "Writing", c.ProfilePath))
                .ToList() ?? new List<CrewMemberDto>();

            var seasons = tv.Seasons?
                .Where(s => s.SeasonNumber > 0)
                .Select(s => new TvSeasonSummaryDto(
                    s.SeasonNumber,
                    s.Name ?? $"Season {s.SeasonNumber}",
                    s.EpisodeCount,
                    s.AirDate,
                    s.PosterPath,
                    s.Overview))
                .ToList() ?? new List<TvSeasonSummaryDto>();

            var watchProviders = ExtractWatchProviders(tv.WatchProviders);

            var similar = tv.Similar?.Results?
                .Take(6)
                .Select(s => new TmdbSearchResultDto(
                    s.Id,
                    s.Name ?? "Untitled",
                    isAnime ? MediaType.Anime : MediaType.TvSeries,
                    s.PosterPath,
                    s.BackdropPath,
                    s.FirstAirDate,
                    s.VoteAverage,
                    s.Overview))
                .ToList() ?? new List<TmdbSearchResultDto>();

            return new TmdbTitleDetailsDto(
                tv.Id,
                tv.Name ?? "Untitled Series",
                tv.OriginalName,
                isAnime ? MediaType.Anime : MediaType.TvSeries,
                tv.PosterPath,
                tv.BackdropPath,
                tv.Overview,
                tv.FirstAirDate,
                tv.Status,
                tv.VoteAverage,
                tv.VoteCount,
                tv.EpisodeRunTime?.FirstOrDefault(),
                tv.Genres?.Where(g => !string.IsNullOrEmpty(g.Name)).Select(g => g.Name!).ToList() ?? new List<string>(),
                trailer,
                cast,
                directors,
                seasons,
                watchProviders,
                similar,
                null
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get TV show details for {TvShowId}", tvShowId);
            return null;
        }
    }

    public async Task<FollowedPersonDto?> GetPersonDetailsAsync(int personId, CancellationToken ct = default)
    {
        try
        {
            var person = await _client.GetPersonAsync(personId,
                PersonMethods.MovieCredits | PersonMethods.TvCredits,
                cancellationToken: ct);

            if (person == null) return null;

            var filmography = new List<PersonCreditDto>();

            if (person.MovieCredits?.Cast != null)
            {
                foreach (var c in person.MovieCredits.Cast)
                {
                    var isUpcoming = c.ReleaseDate.HasValue && c.ReleaseDate.Value > DateTime.Today;
                    filmography.Add(new PersonCreditDto(
                        c.Id,
                        c.Title ?? "Untitled Movie",
                        MediaType.Movie,
                        c.Character,
                        c.ReleaseDate,
                        c.PosterPath,
                        c.VoteAverage,
                        IsWatched: false,
                        IsUpcoming: isUpcoming
                    ));
                }
            }

            if (person.TvCredits?.Cast != null)
            {
                foreach (var c in person.TvCredits.Cast)
                {
                    var isUpcoming = c.FirstAirDate.HasValue && c.FirstAirDate.Value > DateTime.Today;
                    filmography.Add(new PersonCreditDto(
                        c.Id,
                        c.Name ?? "Untitled Series",
                        MediaType.TvSeries,
                        c.Character,
                        c.FirstAirDate,
                        c.PosterPath,
                        null,
                        IsWatched: false,
                        IsUpcoming: isUpcoming
                    ));
                }
            }

            if (person.MovieCredits?.Crew != null)
            {
                foreach (var c in person.MovieCredits.Crew.Where(cr => string.Equals(cr.Job, "Director", StringComparison.OrdinalIgnoreCase)))
                {
                    var isUpcoming = c.ReleaseDate.HasValue && c.ReleaseDate.Value > DateTime.Today;
                    filmography.Add(new PersonCreditDto(
                        c.Id,
                        c.Title ?? "Untitled Movie",
                        MediaType.Movie,
                        "Director",
                        c.ReleaseDate,
                        c.PosterPath,
                        null,
                        IsWatched: false,
                        IsUpcoming: isUpcoming
                    ));
                }
            }

            var orderedFilmography = filmography
                .GroupBy(c => new { c.Id, c.Type })
                .Select(g => g.First())
                .OrderByDescending(c => c.ReleaseDate ?? DateTime.MinValue)
                .ToList();

            var upcoming = orderedFilmography
                .Where(c => c.IsUpcoming || (c.ReleaseDate.HasValue && c.ReleaseDate.Value >= DateTime.Today.AddDays(-30)))
                .Take(10)
                .ToList();

            return new FollowedPersonDto(
                Guid.Empty,
                person.Id,
                person.Name ?? "Unknown Person",
                person.KnownForDepartment ?? "Acting",
                person.ProfilePath,
                person.Biography,
                person.PlaceOfBirth,
                person.Birthday,
                DateTime.UtcNow,
                null,
                0,
                orderedFilmography.Count,
                0,
                upcoming,
                orderedFilmography
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get person details for {PersonId}", personId);
            return null;
        }
    }

    public async Task<TmdbDiscoverDto> GetDiscoverAsync(CancellationToken ct = default)
    {
        try
        {
            var trendingTask = _client.GetTrendingMoviesAsync(TimeWindow.Day, cancellationToken: ct);
            var popularMoviesTask = _client.GetMoviePopularListAsync(cancellationToken: ct);
            var popularTvTask = _client.GetTvShowPopularAsync(cancellationToken: ct);
            var topRatedTask = _client.GetMovieTopRatedListAsync(cancellationToken: ct);
            var upcomingTask = _client.GetMovieUpcomingListAsync(cancellationToken: ct);

            await Task.WhenAll(trendingTask, popularMoviesTask, popularTvTask, topRatedTask, upcomingTask);

            var trending = trendingTask.Result?.Results?
                .Take(10)
                .Select(m => new TmdbSearchResultDto(m.Id, m.Title ?? "Untitled", MediaType.Movie, m.PosterPath, m.BackdropPath, m.ReleaseDate, m.VoteAverage, m.Overview))
                .ToList() ?? new List<TmdbSearchResultDto>();

            var popularMovies = popularMoviesTask.Result?.Results?
                .Take(10)
                .Select(m => new TmdbSearchResultDto(m.Id, m.Title ?? "Untitled", MediaType.Movie, m.PosterPath, m.BackdropPath, m.ReleaseDate, m.VoteAverage, m.Overview))
                .ToList() ?? new List<TmdbSearchResultDto>();

            var popularTv = popularTvTask.Result?.Results?
                .Take(10)
                .Select(t => new TmdbSearchResultDto(t.Id, t.Name ?? "Untitled", MediaType.TvSeries, t.PosterPath, t.BackdropPath, t.FirstAirDate, t.VoteAverage, t.Overview))
                .ToList() ?? new List<TmdbSearchResultDto>();

            var topRated = topRatedTask.Result?.Results?
                .Take(10)
                .Select(m => new TmdbSearchResultDto(m.Id, m.Title ?? "Untitled", MediaType.Movie, m.PosterPath, m.BackdropPath, m.ReleaseDate, m.VoteAverage, m.Overview))
                .ToList() ?? new List<TmdbSearchResultDto>();

            var upcoming = upcomingTask.Result?.Results?
                .Take(10)
                .Select(m => new TmdbSearchResultDto(m.Id, m.Title ?? "Untitled", MediaType.Movie, m.PosterPath, m.BackdropPath, m.ReleaseDate, m.VoteAverage, m.Overview))
                .ToList() ?? new List<TmdbSearchResultDto>();

            // Curated Anime Highlights (Popular Japanese anime series)
            var animeHighlights = popularTv
                .Where(t => t.Overview != null && (t.Overview.Contains("Anime", StringComparison.OrdinalIgnoreCase) || t.Overview.Contains("Japan", StringComparison.OrdinalIgnoreCase)))
                .ToList();

            if (animeHighlights.Count < 4)
            {
                // Top-notch anime titles
                animeHighlights = new List<TmdbSearchResultDto>
                {
                    new(85937, "Demon Slayer: Kimetsu no Yaiba", MediaType.Anime, "/xUfRZu2mi8jH6SzQEJGP6tjBuYj.jpg", "/nTvM4mhqZlHIqvYs0Oz3068Q2IE.jpg", new DateTime(2019, 4, 6), 8.7, "Tanjiro sets out on a perilous path to avenge his family and save his sister."),
                    new(1429, "Attack on Titan", MediaType.Anime, "/hTP1dtwGFamzeVWYVSHwpz3vXiP.jpg", "/7k3iK93c9q7a4h2Q2U4zJ2RzUu.jpg", new DateTime(2013, 4, 7), 8.7, "Humanity fights giant humanoids in a post-apocalyptic walled world."),
                    new(37854, "One Piece", MediaType.Anime, "/cMD9Ygz11yjEZziEiikTM9Ohnak.jpg", "/4MCKNAc6AbWjEsM2crSrGlQI4BM.jpg", new DateTime(1999, 10, 20), 8.7, "Monkey D. Luffy embarks on an epic journey to become the King of the Pirates."),
                    new(46298, "Hunter x Hunter", MediaType.Anime, "/ucmpVHI4eaUvI85U4a434Bv9Q2Z.jpg", "/2n5b4zZl4a3f4e2r7e8w9q0p1.jpg", new DateTime(2011, 10, 2), 8.8, "Gon Freecss seeks his father and becomes an elite licensed Hunter.")
                };
            }

            var recommendations = topRated.Take(6).ToList();

            return new TmdbDiscoverDto(
                trending,
                popularMovies,
                popularTv,
                topRated,
                upcoming,
                animeHighlights,
                recommendations
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get discover content from TMDb");
            return new TmdbDiscoverDto(
                new List<TmdbSearchResultDto>(),
                new List<TmdbSearchResultDto>(),
                new List<TmdbSearchResultDto>(),
                new List<TmdbSearchResultDto>(),
                new List<TmdbSearchResultDto>(),
                new List<TmdbSearchResultDto>(),
                new List<TmdbSearchResultDto>()
            );
        }
    }

    public async Task<IReadOnlyList<TvEpisodeSummaryDto>> GetTvSeasonEpisodesAsync(int tvShowId, int seasonNumber, CancellationToken ct = default)
    {
        try
        {
            var season = await _client.GetTvSeasonAsync(tvShowId, seasonNumber, cancellationToken: ct);
            if (season?.Episodes == null) return Array.Empty<TvEpisodeSummaryDto>();

            return season.Episodes.Select(e => new TvEpisodeSummaryDto(
                (int)e.EpisodeNumber,
                e.SeasonNumber,
                e.Name ?? $"Episode {e.EpisodeNumber}",
                e.Overview,
                e.AirDate,
                e.StillPath,
                e.VoteAverage
            )).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get TV season episodes for Show {ShowId} Season {Season}", tvShowId, seasonNumber);
            return Array.Empty<TvEpisodeSummaryDto>();
        }
    }

    public async Task<IReadOnlyList<TmdbSearchResultDto>> GetSimilarTitlesAsync(int id, MediaType type, CancellationToken ct = default)
    {
        try
        {
            if (type == MediaType.Movie)
            {
                var similar = await _client.GetMovieSimilarAsync(id, 1, cancellationToken: ct);
                if (similar?.Results == null) return Array.Empty<TmdbSearchResultDto>();

                return similar.Results.Take(10).Select(m => new TmdbSearchResultDto(
                    m.Id,
                    m.Title ?? "Untitled",
                    MediaType.Movie,
                    m.PosterPath,
                    m.BackdropPath,
                    m.ReleaseDate,
                    m.VoteAverage,
                    m.Overview
                )).ToList();
            }
            else
            {
                var similar = await _client.GetTvShowSimilarAsync(id, 1, cancellationToken: ct);
                if (similar?.Results == null) return Array.Empty<TmdbSearchResultDto>();

                return similar.Results.Take(10).Select(t => new TmdbSearchResultDto(
                    t.Id,
                    t.Name ?? "Untitled",
                    type == MediaType.Anime ? MediaType.Anime : MediaType.TvSeries,
                    t.PosterPath,
                    t.BackdropPath,
                    t.FirstAirDate,
                    t.VoteAverage,
                    t.Overview
                )).ToList();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get similar titles for {Id} {Type}", id, type);
            return Array.Empty<TmdbSearchResultDto>();
        }
    }

    public async Task<WatchProvidersDto?> GetWatchProvidersAsync(int id, MediaType type, CancellationToken ct = default)
    {
        try
        {
            TMDbGeneral.SingleResultContainer<Dictionary<string, TMDbGeneral.WatchProviders>>? providersContainer = null;
            if (type == MediaType.Movie)
            {
                providersContainer = await _client.GetMovieWatchProvidersAsync(id, cancellationToken: ct);
            }
            else
            {
                providersContainer = await _client.GetTvShowWatchProvidersAsync(id, cancellationToken: ct);
            }

            return ExtractWatchProviders(providersContainer);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get watch providers for {Id} {Type}", id, type);
            return null;
        }
    }

    private static WatchProvidersDto? ExtractWatchProviders(TMDbGeneral.SingleResultContainer<Dictionary<string, TMDbGeneral.WatchProviders>>? container)
    {
        if (container?.Results == null || container.Results.Count == 0) return null;

        TMDbGeneral.WatchProviders? regionData = null;

        if (container.Results.TryGetValue("US", out var usData))
        {
            regionData = usData;
        }
        else if (container.Results.TryGetValue("GB", out var gbData))
        {
            regionData = gbData;
        }
        else
        {
            regionData = container.Results.Values.FirstOrDefault();
        }

        if (regionData == null) return null;

        var flatrate = regionData.FlatRate?
            .Select(p => new WatchProviderItemDto(p.ProviderId ?? 0, p.ProviderName ?? "Streaming Provider", p.LogoPath))
            .ToList() ?? new List<WatchProviderItemDto>();

        var rent = regionData.Rent?
            .Select(p => new WatchProviderItemDto(p.ProviderId ?? 0, p.ProviderName ?? "Rental Provider", p.LogoPath))
            .ToList() ?? new List<WatchProviderItemDto>();

        var buy = regionData.Buy?
            .Select(p => new WatchProviderItemDto(p.ProviderId ?? 0, p.ProviderName ?? "Purchase Provider", p.LogoPath))
            .ToList() ?? new List<WatchProviderItemDto>();

        return new WatchProvidersDto(flatrate, rent, buy, regionData.Link);
    }
}
