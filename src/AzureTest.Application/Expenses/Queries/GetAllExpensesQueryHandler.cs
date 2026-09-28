using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Interfaces.Data;
using AzureTest.Application.Interfaces.Data.NoSql;
using AzureTest.Application.Interfaces.Services;
using AzureTest.Application.Messaging;
using AzureTest.Contracts.DTOs.Common;
using AzureTest.Contracts.DTOs.Expenses;
using AzureTest.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace AzureTest.Application.Expenses.Queries;

// ── Query ─────────────────────────────────────────────────────────────────────

public sealed record GetAllExpensesQuery(GetAllExpensesFilters? Filters = null, Guid? UserId = null) : IQuery<PagedResult<ExpenseDto>>;

// ── Handler ───────────────────────────────────────────────────────────────────

public sealed class GetAllExpensesQueryHandler : BaseQueryHandler<GetAllExpensesQuery, PagedResult<ExpenseDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly INoSqlUnitOfWork _noSqlUnitOfWork;
    private readonly ICurrentUser _currentUser;

    public GetAllExpensesQueryHandler(
        IUnitOfWork unitOfWork,
        INoSqlUnitOfWork noSqlUnitOfWork,
        ICurrentUser currentUser,
        ILogger<GetAllExpensesQueryHandler> logger)
        : base(logger)
    {
        _unitOfWork = unitOfWork;
        _noSqlUnitOfWork = noSqlUnitOfWork;
        _currentUser = currentUser;
    }

    protected override async Task<PagedResult<ExpenseDto>> ExecuteAsync(GetAllExpensesQuery request, CancellationToken cancellationToken)
    {
        bool isAdmin = _currentUser.Roles.Contains("Admin");

        // Non-admins must strictly only see their own expenses to guarantee privacy
        Guid? targetUserId = !isAdmin
            ? _currentUser.Id
            : (request.UserId ?? request.Filters?.UserId);

        var expensesQuery = targetUserId.HasValue
            ? await _unitOfWork.Expenses.FindAsync(e => !e.IsDeleted && e.UserId == targetUserId.Value)
            : await _unitOfWork.Expenses.FindAsync(e => !e.IsDeleted);

        var expenses = expensesQuery.ToList();
        var queryable = expenses.AsQueryable();

        if (request.Filters?.StartDate.HasValue == true)
        {
            queryable = queryable.Where(e => e.Date >= request.Filters.StartDate.Value);
        }

        if (request.Filters?.EndDate.HasValue == true)
        {
            queryable = queryable.Where(e => e.Date <= request.Filters.EndDate.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.Filters?.SearchTerm))
        {
            var term = request.Filters.SearchTerm.ToLower();
            queryable = queryable.Where(e => 
                (!string.IsNullOrEmpty(e.Description) && e.Description.ToLower().Contains(term)));
        }

        int totalCount = queryable.Count();
        int pageNumber = request.Filters?.PageNumber ?? 1;
        int pageSize = request.Filters?.PageSize ?? 10;
        
        string sortBy = (request.Filters?.SortBy ?? "Date").ToLower();
        bool desc = request.Filters?.SortDescending ?? true;

        queryable = sortBy switch
        {
            "amount" => desc ? queryable.OrderByDescending(e => e.Amount) : queryable.OrderBy(e => e.Amount),
            _ => desc ? queryable.OrderByDescending(e => e.Date) : queryable.OrderBy(e => e.Date)
        };

        var pagedExpenses = (pageSize > 0)
            ? queryable.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList()
            : queryable.ToList();

        var dtos = pagedExpenses.Select(e => new ExpenseDto
        {
            Id = e.Id,
            Amount = e.Amount,
            Date = e.Date,
            Description = e.Description,
            CategoryId = e.CategoryId
        }).ToList();

        // Populate category names efficiently by batching distinct category IDs
        try
        {
            var distinctCatIds = dtos
                .Where(d => d.CategoryId.HasValue)
                .Select(d => d.CategoryId!.Value)
                .Distinct()
                .ToList();

            if (distinctCatIds.Count > 0)
            {
                var categoryMap = new Dictionary<Guid, string>();
                foreach (var catId in distinctCatIds)
                {
                    var category = await _noSqlUnitOfWork.ExpenseCategories.GetByIdAsync(catId);
                    if (category != null && !string.IsNullOrWhiteSpace(category.Name))
                    {
                        categoryMap[catId] = category.Name;
                    }
                }

                foreach (var dto in dtos)
                {
                    if (dto.CategoryId.HasValue && categoryMap.TryGetValue(dto.CategoryId.Value, out var name))
                    {
                        dto.CategoryName = name;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "Failed to resolve expense category names");
        }

        return new PagedResult<ExpenseDto>(dtos, totalCount, pageNumber, pageSize > 0 ? pageSize : totalCount);
    }
}
