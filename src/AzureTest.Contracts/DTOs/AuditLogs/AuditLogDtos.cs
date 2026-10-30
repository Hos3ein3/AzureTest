using System;
using System.Collections.Generic;
using AzureTest.Contracts.DTOs.Users;

namespace AzureTest.Contracts.DTOs.AuditLogs;

public sealed record AuditLogDto(
    Guid Id,
    Guid TenantId,
    Guid UserId,
    string? UserName,
    string? UserEmail,
    string ActionType,
    string ActionName,
    string? Payload,
    long ExecutionDurationMs,
    bool IsSuccess,
    string? ErrorMessage,
    string? IpAddress,
    Guid? ImpersonatedByAdminId,
    DateTime Timestamp
);

public sealed record GetAuditLogsFilterDto(
    string? ActionType = null,
    string? SearchTerm = null,
    Guid? FilterUserId = null,
    int PageNumber = 1,
    int PageSize = 20
);

public sealed record AuditLogListResponseDto
{
    public IReadOnlyList<AuditLogDto> Logs { get; init; } = Array.Empty<AuditLogDto>();
    public long TotalCount { get; init; }
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 20;
    public int TotalPages => Math.Max(1, (int)Math.Ceiling((double)TotalCount / (PageSize > 0 ? PageSize : 20)));
    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPages;
    public string? ActionType { get; init; }
    public string? SearchTerm { get; init; }
    public Guid? FilterUserId { get; init; }
    public IReadOnlyList<UserListDto> AvailableUsers { get; init; } = Array.Empty<UserListDto>();
    public bool IsAdmin { get; init; }
    public bool IsImpersonating { get; init; }
    public Guid CurrentTenantId { get; init; }
    public Guid CurrentUserId { get; init; }
}

public sealed record DeleteAuditLogRequestDto(
    Guid Id
);

public sealed record DeleteAuditLogsByUserRequestDto(
    Guid UserId
);

public sealed record DeleteAuditLogsByDateRangeRequestDto(
    DateTime? FromDate = null,
    DateTime? ToDate = null
);
