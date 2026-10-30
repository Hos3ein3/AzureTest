using System;
using System.Collections.Generic;

namespace AzureTest.Contracts.DTOs.Outbox;

public sealed record OutboxMessageDto(
    Guid Id,
    string EventType,
    string Payload,
    DateTime OccurredOnUtc,
    DateTime? ProcessedOnUtc,
    string? Error,
    int RetryCount
);

public sealed record InboxMessageDto(
    Guid Id,
    string HandlerName,
    DateTime ReceivedOnUtc,
    DateTime ProcessedOnUtc
);

public sealed record GetOutboxMessagesFilterDto(
    string? SearchTerm = null,
    bool? IsDone = null,
    DateTime? FromDate = null,
    DateTime? ToDate = null,
    string? SortBy = "OccurredOnUtc",
    bool SortDescending = true,
    int PageNumber = 1,
    int PageSize = 20
);

public sealed record GetInboxMessagesFilterDto(
    string? SearchTerm = null,
    DateTime? FromDate = null,
    DateTime? ToDate = null,
    string? SortBy = "ReceivedOnUtc",
    bool SortDescending = true,
    int PageNumber = 1,
    int PageSize = 20
);

public sealed record OutboxMessageListResponseDto
{
    public IReadOnlyList<OutboxMessageDto> Messages { get; init; } = Array.Empty<OutboxMessageDto>();
    public int TotalCount { get; init; }
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 20;
    public int TotalPages => Math.Max(1, (int)Math.Ceiling((double)TotalCount / (PageSize > 0 ? PageSize : 20)));
    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPages;
    public string? SearchTerm { get; init; }
    public bool? IsDone { get; init; }
    public DateTime? FromDate { get; init; }
    public DateTime? ToDate { get; init; }
    public string? SortBy { get; init; } = "OccurredOnUtc";
    public bool SortDescending { get; init; } = true;
    public int PendingCount { get; init; }
    public int ProcessedCount { get; init; }
    public int FailedCount { get; init; }
}

public sealed record InboxMessageListResponseDto
{
    public IReadOnlyList<InboxMessageDto> Messages { get; init; } = Array.Empty<InboxMessageDto>();
    public int TotalCount { get; init; }
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 20;
    public int TotalPages => Math.Max(1, (int)Math.Ceiling((double)TotalCount / (PageSize > 0 ? PageSize : 20)));
    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPages;
    public string? SearchTerm { get; init; }
    public DateTime? FromDate { get; init; }
    public DateTime? ToDate { get; init; }
    public string? SortBy { get; init; } = "ReceivedOnUtc";
    public bool SortDescending { get; init; } = true;
    public int TotalProcessedCount { get; init; }
}

public sealed record OutboxPayloadResponseDto(
    Guid Id,
    string EventType,
    DateTime OccurredOnUtc,
    DateTime? ProcessedOnUtc,
    int RetryCount,
    string? Error,
    string Payload
);

public sealed record DeleteOutboxMessageRequestDto(
    Guid Id
);

public sealed record DeleteInboxMessageRequestDto(
    Guid Id
);

public sealed record DeleteDateRangeRequestDto(
    DateTime? FromDate = null,
    DateTime? ToDate = null
);
