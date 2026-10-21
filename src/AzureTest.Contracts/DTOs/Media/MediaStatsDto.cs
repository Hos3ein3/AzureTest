using System.Collections.Generic;
using AzureTest.Domain.Media.Enums;

namespace AzureTest.Contracts.DTOs.Media;

public record GenreStatDto(
    string Genre,
    int Count,
    double Percentage
);

public record PersonStatDto(
    string Name,
    string Role,
    int WatchedCount
);

public record StatusCountDto(
    string Status,
    int Count,
    string ColorHex
);

public record TypeCountDto(
    string MediaType,
    int Count,
    int HoursWatched
);

public record RewatchedTitleDto(
    int TmdbId,
    MediaType Type,
    string Title,
    string? PosterPath,
    int RewatchCount,
    int TotalWatches,
    int EstimatedHours
);

public record MediaStatsDto(
    int TotalItemsWatched,
    int TotalEstimatedMinutesWatched,
    int TotalEstimatedHoursWatched,
    int TotalEpisodesWatched,
    int TotalMoviesWatched,
    int TotalSeriesWatched,
    int TotalAnimeWatched,
    double AverageRating,
    List<GenreStatDto> TopGenres,
    List<PersonStatDto> TopActors,
    List<StatusCountDto> StatusBreakdown,
    List<TypeCountDto> TypeBreakdown,
    Dictionary<int, int> RatingDistribution, // rating 1-10 -> count
    int TotalRewatchCount = 0,
    int TotalRewatchedTitles = 0,
    int TotalRewatchHours = 0,
    List<RewatchedTitleDto>? MostRewatchedTitles = null
);
