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

public sealed record GetAllExpensesQuery(GetAllExpensesFilters Filters) : IQuery<PagedResult<ExpenseDto>>;

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

    protected override async Task<PagedResult<ExpenseDto>> ExecuteAsync(GetAllExpensesQuery query, CancellationToken cancellationToken)
    {
        var targetUserId = query.Filters?.UserId ?? _currentUser.Id;

        var expensesQuery = await _unitOfWork.Expenses.FindAsync(e => !e.IsDeleted);
        var expenses = expensesQuery.ToList();

        if (query.Filters?.UserId != null || !_currentUser.Roles.Contains("Admin"))
        {
            expenses = expenses.Where(e => e.UserId == targetUserId).ToList();
        }

        var queryable = expenses.AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.Filters?.SearchTerm))
        {
            var term = query.Filters.SearchTerm.ToLower();
            queryable = queryable.Where(e => 
                (!string.IsNullOrEmpty(e.Description) && e.Description.ToLower().Contains(term)));
        }

        int totalCount = queryable.Count();
        int pageNumber = query.Filters?.PageNumber ?? 1;
        int pageSize = query.Filters?.PageSize ?? 10;
        
        string sortBy = (query.Filters?.SortBy ?? "Date").ToLower();
        bool desc = query.Filters?.SortDescending ?? true;

        queryable = sortBy switch
        {
            "amount" => desc ? queryable.OrderByDescending(e => e.Amount) : queryable.OrderBy(e => e.Amount),
            _ => desc ? queryable.OrderByDescending(e => e.Date) : queryable.OrderBy(e => e.Date)
        };

        var pagedExpenses = queryable
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        var dtos = pagedExpenses.Select(e => new ExpenseDto
        {
            Id = e.Id,
            Amount = e.Amount,
            Date = e.Date,
            Description = e.Description,
            CategoryId = e.CategoryId
        }).ToList();

        // Populate category names
        foreach (var dto in dtos)
        {
            if (dto.CategoryId.HasValue)
            {
                var category = await _noSqlUnitOfWork.ExpenseCategories.GetByIdAsync(dto.CategoryId.Value);
                if (category != null)
                {
                    dto.CategoryName = category.Name;
                }
            }
        }

        return new PagedResult<ExpenseDto>(dtos, totalCount, pageNumber, pageSize);
    }
}
