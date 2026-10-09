using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Interfaces.Data.NoSql;
using AzureTest.Application.Messaging;
using AzureTest.Contracts.DTOs.Common;
using AzureTest.Contracts.DTOs.Dashboards;

namespace AzureTest.Application.Dashboards.Components.Queries;

public record GetAllComponentsQuery(string? Search = null, string? Category = null, string? Type = null, string? Sort = null, string? Dir = null, int Page = 1, int PageSize = 10, AzureTest.Contracts.DTOs.Dashboards.GetAllComponentsFilters? Filters = null) : IQuery<PagedResult<UnifiedComponentDto>>
{
    public GetAllComponentsQuery(AzureTest.Contracts.DTOs.Dashboards.GetAllComponentsFilters filters)
        : this(filters.SearchTerm, filters.Category, filters.Type, filters.SortBy, filters.SortDescending ? "desc" : "asc", filters.PageNumber, filters.PageSize, filters)
    {
    }
}

public class GetAllComponentsQueryHandler : IQueryHandler<GetAllComponentsQuery, PagedResult<UnifiedComponentDto>>
{
    private readonly INoSqlUnitOfWork _unitOfWork;

    public GetAllComponentsQueryHandler(INoSqlUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<PagedResult<UnifiedComponentDto>> HandleAsync(GetAllComponentsQuery query, CancellationToken cancellationToken = default)
    {
        var standardComps = await _unitOfWork.DashboardComponents.GetAllAsync(cancellationToken);
        var rawComps = await _unitOfWork.DashboardRawComponents.GetAllAsync(cancellationToken);

        var unifiedList = new List<UnifiedComponentDto>();

        unifiedList.AddRange(standardComps.Select(c => new UnifiedComponentDto
        {
            Id = c.Id,
            Name = c.Name,
            Description = c.Description ?? "",
            Type = "Standard",
            Category = c.Category ?? "General",
            RequiredRole = c.RequiredRole ?? "",
            IsRaw = false
        }));

        unifiedList.AddRange(rawComps.Select(c => new UnifiedComponentDto
        {
            Id = c.Id,
            Name = c.Name,
            Description = c.Type == "RawHtml" ? "Raw HTML Snippet" : "JSON Configuration",
            Type = c.Type,
            Category = c.Category ?? "General",
            RequiredRole = c.RequiredRole ?? "",
            IsRaw = true
        }));

        var filtered = unifiedList.AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var lowerSearch = query.Search.ToLower();
            filtered = filtered.Where(c => c.Name.ToLower().Contains(lowerSearch) || c.Description.ToLower().Contains(lowerSearch) || c.Type.ToLower().Contains(lowerSearch));
        }

        if (!string.IsNullOrWhiteSpace(query.Category))
        {
            var lowerCat = query.Category.ToLower();
            filtered = filtered.Where(c => c.Category.ToLower() == lowerCat);
        }

        if (!string.IsNullOrWhiteSpace(query.Type))
        {
            var lowerType = query.Type.ToLower();
            filtered = filtered.Where(c => c.Type.ToLower() == lowerType);
        }

        if (!string.IsNullOrWhiteSpace(query.Sort))
        {
            var sort = query.Sort.ToLower();
            if (sort == "name")
                filtered = query.Dir == "desc" ? filtered.OrderByDescending(c => c.Name) : filtered.OrderBy(c => c.Name);
            else if (sort == "category")
                filtered = query.Dir == "desc" ? filtered.OrderByDescending(c => c.Category) : filtered.OrderBy(c => c.Category);
            else if (sort == "type")
                filtered = query.Dir == "desc" ? filtered.OrderByDescending(c => c.Type) : filtered.OrderBy(c => c.Type);
        }
        else
        {
            filtered = filtered.OrderBy(c => c.Name);
        }

        var totalCount = filtered.Count();
        var pagedItems = filtered.Skip((query.Page - 1) * query.PageSize).Take(query.PageSize).ToList();

        return new PagedResult<UnifiedComponentDto>(pagedItems, totalCount, query.Page, query.PageSize);
    }
}
