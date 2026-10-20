using System;
using System.Collections.Generic;
using AzureTest.Domain.Media.Enums;

namespace AzureTest.Contracts.DTOs.Media;

public record CalendarEventDto(
    int TmdbId,
    string Title,
    MediaType Type,
    DateTime ReleaseDate,
    string? PosterPath,
    string EventType, // "Movie Premiere", "Season Premiere", "Episode Airing", "Wishlist Release"
    string? Detail,
    string? SeriesStatus // "Returning Series", "Ended", "Canceled"
);

public record MediaCalendarDto(
    int Year,
    int Month,
    List<CalendarEventDto> Events,
    List<CalendarEventDto> UpcomingWishlist,
    List<CalendarEventDto> StatusAlerts // Renewals, cancellations, returning
);
