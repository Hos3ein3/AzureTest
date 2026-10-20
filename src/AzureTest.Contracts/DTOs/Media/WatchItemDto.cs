using System;
using System.Collections.Generic;
using AzureTest.Domain.Media.Enums;

namespace AzureTest.Contracts.DTOs.Media;

public record WatchItemDto(
    Guid Id,
    Guid UserId,
    int TmdbId,
    MediaType Type,
    string Title,
    string? OriginalTitle,
    string? PosterPath,
    string? BackdropPath,
    string? Overview,
    DateTime? ReleaseDate,
    double? TmdbVoteAverage,
    WatchStatus Status,
    int CurrentSeason,
    int CurrentEpisode,
    int TotalSeasons,
    int TotalEpisodes,
    int? RuntimeMinutes,
    int? UserRating,
    string? UserReview,
    DateTime? WatchedDate,
    DateTime? LastWatchedAt,
    List<string> Genres,
    string NextEpisodeLabel,
    int EstimatedWatchedMinutes
);

public record SaveWatchProgressDto(
    int Season,
    int Episode,
    int? TotalSeasons = null,
    int? TotalEpisodes = null
);

public record RateWatchItemDto(
    int? Rating,
    string? Review,
    DateTime? WatchedDate = null
);
