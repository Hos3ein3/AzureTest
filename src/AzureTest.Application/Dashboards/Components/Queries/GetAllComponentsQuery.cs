using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Interfaces.Data.NoSql;
using AzureTest.Application.Messaging;
using AzureTest.Contracts.DTOs.Common;
using AzureTest.Domain.Documents;

namespace AzureTest.Application.Dashboards.Components.Queries;

public record GetAllComponentsQuery(string? SearchTerm = null, string? Category = null, string? SortBy = null, bool SortDescending = false, int PageNumber = 1, int PageSize = 10) : IQuery<PagedResult<DashboardComponent>>;

public class GetAllComponentsQueryHandler : IQueryHandler<GetAllComponentsQuery, PagedResult<DashboardComponent>>
{
    private readonly INoSqlUnitOfWork _unitOfWork;

    public GetAllComponentsQueryHandler(INoSqlUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<PagedResult<DashboardComponent>> HandleAsync(GetAllComponentsQuery query, CancellationToken cancellationToken = default)
    {
        var all = await _unitOfWork.DashboardComponents.GetAllAsync(cancellationToken);
        var filtered = all.AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.SearchTerm))
        {
            var lowerSearch = query.SearchTerm.ToLower();
            filtered = filtered.Where(c => c.Name.ToLower().Contains(lowerSearch) || c.Description.ToLower().Contains(lowerSearch) || c.PartialViewName.ToLower().Contains(lowerSearch));
        }

        if (!string.IsNullOrWhiteSpace(query.Category))
        {
            var lowerCat = query.Category.ToLower();
            filtered = filtered.Where(c => c.Category.ToLower() == lowerCat);
        }

        if (!string.IsNullOrWhiteSpace(query.SortBy))
        {
            var sort = query.SortBy.ToLower();
            if (sort == "name")
                filtered = query.SortDescending ? filtered.OrderByDescending(c => c.Name) : filtered.OrderBy(c => c.Name);
            else if (sort == "category")
                filtered = query.SortDescending ? filtered.OrderByDescending(c => c.Category) : filtered.OrderBy(c => c.Category);
            else if (sort == "partialviewname")
                filtered = query.SortDescending ? filtered.OrderByDescending(c => c.PartialViewName) : filtered.OrderBy(c => c.PartialViewName);
        }
        else
        {
            filtered = filtered.OrderBy(c => c.Name);
        }

        var totalCount = filtered.Count();
        var pagedItems = filtered.Skip((query.PageNumber - 1) * query.PageSize).Take(query.PageSize).ToList();

        return new PagedResult<DashboardComponent>(pagedItems, totalCount, query.PageNumber, query.PageSize);
    }
}
