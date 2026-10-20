using System.Collections.Generic;

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
    Dictionary<int, int> RatingDistribution // rating 1-10 -> count
);
