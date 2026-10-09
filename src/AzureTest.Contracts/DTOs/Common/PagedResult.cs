using System;
using System.Collections.Generic;

namespace AzureTest.Contracts.DTOs.Common;

/// <summary>
/// Common interface for all search, filter, and pagination requests.
/// </summary>
public interface IPagedFilter
{
    string? SearchTerm { get; set; }
    string? SortBy { get; set; }
    bool SortDescending { get; set; }
    int PageNumber { get; set; }
    int PageSize { get; set; }
}

/// <summary>
/// Base class for strongly typed filter requests in MVC and API layers.
/// </summary>
public class PagedFilterRequest : IPagedFilter
{
    public string? SearchTerm { get; set; }
    public string? SortBy { get; set; }
    public bool SortDescending { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

/// <summary>
/// Base interface for strong-typed paged results.
/// </summary>
public interface IPagedResult
{
    int TotalCount { get; }
    int PageNumber { get; }
    int PageSize { get; }
    int TotalPages { get; }
    bool HasPreviousPage { get; }
    bool HasNextPage { get; }
}

/// <summary>
/// Strongly-typed paginated result supporting optional strongly-typed filters.
/// </summary>
public class PagedResult<T, TFilter> : IPagedResult where TFilter : class, IPagedFilter
{
    public IReadOnlyList<T> Items { get; init; } = Array.Empty<T>();
    public int TotalCount { get; init; }
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
    public TFilter? Filters { get; init; }

    public int TotalPages => PageSize > 0 ? (int)Math.Ceiling(TotalCount / (double)PageSize) : 0;
    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPages;

    public PagedResult() { }

    public PagedResult(IReadOnlyList<T> items, int totalCount, int pageNumber, int pageSize, TFilter? filters = null)
    {
        Items = items;
        TotalCount = totalCount;
        PageNumber = pageNumber;
        PageSize = pageSize;
        Filters = filters;
    }
}

/// <summary>
/// Backward-compatible single-generic PagedResult<T> with optional IPagedFilter interface support.
/// </summary>
public record PagedResult<T>(
    IReadOnlyList<T> Items,
    int TotalCount,
    int PageNumber,
    int PageSize
) : IPagedResult
{
    public IPagedFilter? BaseFilter { get; init; }

    public int TotalPages => PageSize > 0 ? (int)Math.Ceiling(TotalCount / (double)PageSize) : 0;
    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPages;

    public PagedResult(IReadOnlyList<T> items, int totalCount, int pageNumber, int pageSize, IPagedFilter? filter)
        : this(items, totalCount, pageNumber, pageSize)
    {
        BaseFilter = filter;
    }
}
