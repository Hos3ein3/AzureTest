using System;
using System.Collections.Generic;
using AzureTest.Domain.Documents.Audit;

namespace AzureTest.MVC.ViewModels.AuditLogs;

public class AuditLogListViewModel
{
    public IReadOnlyList<AuditLogDocument> Logs { get; set; } = Array.Empty<AuditLogDocument>();
    public long TotalCount { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public int TotalPages => Math.Max(1, (int)Math.Ceiling((double)TotalCount / (PageSize > 0 ? PageSize : 20)));
    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPages;

    public string? ActionType { get; set; }
    public string? SearchTerm { get; set; }

    public bool IsAdmin { get; set; }
    public bool IsImpersonating { get; set; }
    public Guid CurrentTenantId { get; set; }
    public Guid CurrentUserId { get; set; }
}
