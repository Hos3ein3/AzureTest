using System;
using System.Collections.Generic;
using AzureTest.Domain.Media.Enums;

namespace AzureTest.Contracts.DTOs.Media;

public record TmdbSearchResultDto(
    int Id,
    string Title,
    MediaType Type,
    string? PosterPath,
    string? BackdropPath,
    DateTime? ReleaseDate,
    double? VoteAverage,
    string? Overview,
    string? Department = null,
    bool IsPerson = false
);

public record CastMemberDto(
    int PersonId,
    string Name,
    string? Character,
    string? ProfilePath,
    int Order
);

public record CrewMemberDto(
    int PersonId,
    string Name,
    string? Job,
    string? Department,
    string? ProfilePath
);

public record WatchProviderItemDto(
    int ProviderId,
    string ProviderName,
    string? LogoPath
);

public record WatchProvidersDto(
    List<WatchProviderItemDto> Flatrate,
    List<WatchProviderItemDto> Rent,
    List<WatchProviderItemDto> Buy,
    string? Link
);

public record TvSeasonSummaryDto(
    int SeasonNumber,
    string Name,
    int EpisodeCount,
    DateTime? AirDate,
    string? PosterPath,
    string? Overview,
    List<TvEpisodeSummaryDto>? Episodes = null
);

public record TvEpisodeSummaryDto(
    int EpisodeNumber,
    int SeasonNumber,
    string Name,
    string? Overview,
    DateTime? AirDate,
    string? StillPath,
    double? VoteAverage
);

public record TmdbTitleDetailsDto(
    int Id,
    string Title,
    string? OriginalTitle,
    MediaType Type,
    string? PosterPath,
    string? BackdropPath,
    string? Overview,
    DateTime? ReleaseDate,
    string? Status, // "Ended", "Returning Series", "Canceled", "Released", etc.
    double? VoteAverage,
    int VoteCount,
    int? RuntimeMinutes,
    List<string> Genres,
    string? TrailerYouTubeKey,
    List<CastMemberDto> Cast,
    List<CrewMemberDto> Directors,
    List<TvSeasonSummaryDto> Seasons,
    WatchProvidersDto? WatchProviders,
    List<TmdbSearchResultDto> SimilarTitles,
    WatchItemDto? UserWatchItem
);

public record TmdbDiscoverDto(
    List<TmdbSearchResultDto> TrendingToday,
    List<TmdbSearchResultDto> PopularMovies,
    List<TmdbSearchResultDto> PopularTv,
    List<TmdbSearchResultDto> TopRatedMovies,
    List<TmdbSearchResultDto> UpcomingMovies,
    List<TmdbSearchResultDto> AnimeHighlights,
    List<TmdbSearchResultDto> Recommendations
);
