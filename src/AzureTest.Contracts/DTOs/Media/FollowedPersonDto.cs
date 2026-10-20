using System;
using System.Collections.Generic;
using AzureTest.Domain.Media.Enums;

namespace AzureTest.Contracts.DTOs.Media;

public record PersonCreditDto(
    int Id,
    string Title,
    MediaType Type,
    string? CharacterOrJob,
    DateTime? ReleaseDate,
    string? PosterPath,
    double? VoteAverage,
    bool IsWatched,
    bool IsUpcoming
);

public record FollowedPersonDto(
    Guid Id,
    int TmdbPersonId,
    string Name,
    string KnownForDepartment,
    string? ProfilePath,
    string? Biography,
    string? PlaceOfBirth,
    DateTime? Birthday,
    DateTime FollowedAt,
    string? Notes,
    int WatchedCount,
    int TotalCreditsCount,
    double ProgressPercentage,
    List<PersonCreditDto> UpcomingProjects,
    List<PersonCreditDto> Filmography
);

public record FollowPersonRequestDto(
    int TmdbPersonId,
    string Name,
    string? Department = "Acting",
    string? ProfilePath = null,
    string? Notes = null
);
