using System;

namespace AzureTest.Contracts.DTOs.Media;

public record MovieNewsItemDto(
    string Title,
    string Summary,
    string Source,
    DateTime PublishedAt,
    string Url,
    string? ImageUrl,
    string? Category = "Entertainment"
);
