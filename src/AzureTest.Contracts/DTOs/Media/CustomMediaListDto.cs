using System;
using System.Collections.Generic;
using AzureTest.Domain.Media.Enums;

namespace AzureTest.Contracts.DTOs.Media;

public record CustomMediaListItemDto(
    int TmdbId,
    MediaType Type,
    string Title,
    string? PosterPath,
    int? Year,
    DateTime AddedAt
);

public record CustomMediaListDto(
    Guid Id,
    Guid UserId,
    string Title,
    string? Description,
    List<CustomMediaListItemDto> Items,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);

public record CreateCustomListRequestDto(
    string Title,
    string? Description = null
);

public record AddToListRequestDto(
    Guid ListId,
    int TmdbId,
    MediaType Type,
    string Title,
    string? PosterPath = null,
    int? Year = null
);
