using System;
using System.Collections.Generic;
using AzureTest.Domain.Entities.Outbox;

namespace AzureTest.MVC.ViewModels.Outbox;

public class OutboxMessageListViewModel
{
    public IReadOnlyList<OutboxMessage> Messages { get; set; } = Array.Empty<OutboxMessage>();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public int TotalPages => Math.Max(1, (int)Math.Ceiling((double)TotalCount / (PageSize > 0 ? PageSize : 20)));
    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPages;

    // Filters
    public string? SearchTerm { get; set; }
    public bool? IsDone { get; set; } // null = All, true = Done (Processed), false = Pending
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }

    // Sorting
    public string? SortBy { get; set; } = "OccurredOnUtc";
    public bool SortDescending { get; set; } = true;

    // High-level statistics
    public int PendingCount { get; set; }
    public int ProcessedCount { get; set; }
    public int FailedCount { get; set; }
}
