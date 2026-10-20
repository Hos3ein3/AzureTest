using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Interfaces.Data;
using AzureTest.Application.Interfaces.Data.NoSql;
using AzureTest.Application.Interfaces.Services;
using AzureTest.Application.Messaging;
using AzureTest.Contracts.DTOs.Debts;
using AzureTest.Contracts.DTOs.Common;
using AzureTest.Domain.Entities;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace AzureTest.Application.Debts.Queries;

// ── Query ─────────────────────────────────────────────────────────────────────

public sealed record GetAllDebtsQuery(GetAllDebtsFilters? Filters = null, Guid? UserId = null) : IQuery<PagedResult<DebtDto, GetAllDebtsFilters>>
{
    public GetAllDebtsQuery(Guid? userId) : this(null, userId) {}
}

// ── Handler ───────────────────────────────────────────────────────────────────

public sealed class GetAllDebtsQueryHandler : BaseQueryHandler<GetAllDebtsQuery, PagedResult<DebtDto, GetAllDebtsFilters>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly INoSqlUnitOfWork _noSqlUnitOfWork;
    private readonly ICurrentUser _currentUser;

    public GetAllDebtsQueryHandler(
        IUnitOfWork unitOfWork,
        INoSqlUnitOfWork noSqlUnitOfWork,
        ICurrentUser currentUser,
        ILogger<GetAllDebtsQueryHandler> logger)
        : base(logger)
    {
        _unitOfWork = unitOfWork;
        _noSqlUnitOfWork = noSqlUnitOfWork;
        _currentUser = currentUser;
    }

    protected override async Task<PagedResult<DebtDto, GetAllDebtsFilters>> ExecuteAsync(GetAllDebtsQuery query, CancellationToken cancellationToken)
    {
        var filters = query.Filters ?? new GetAllDebtsFilters();
        var isAdmin = _currentUser.IsAdmin || _currentUser.Roles.Contains("Admin");
        
        var queryable = await _unitOfWork.Debts.FindAsync(r => !r.IsDeleted);
        var debts = queryable.ToList();
        
        // Security: Non-admin users are restricted to their own userId
        if (!isAdmin)
        {
            filters.UserIds = new List<Guid> { _currentUser.Id };
            debts = debts.Where(r => r.UserId == _currentUser.Id).ToList();
        }
        else
        {
            if (filters.UserIds != null && filters.UserIds.Any())
            {
                var allowedUserIds = new HashSet<Guid>(filters.UserIds);
                debts = debts.Where(r => allowedUserIds.Contains(r.UserId)).ToList();
            }
            else if (query.UserId.HasValue)
            {
                debts = debts.Where(r => r.UserId == query.UserId.Value).ToList();
            }
        }

        // CreatedOn range filters
        if (filters.CreatedOnFrom.HasValue)
        {
            debts = debts.Where(d => d.CreatedOn >= filters.CreatedOnFrom.Value).ToList();
        }

        if (filters.CreatedOnTo.HasValue)
        {
            var toDate = filters.CreatedOnTo.Value.TimeOfDay == TimeSpan.Zero
                ? filters.CreatedOnTo.Value.Date.AddDays(1).AddTicks(-1)
                : filters.CreatedOnTo.Value;
            debts = debts.Where(d => d.CreatedOn <= toDate).ToList();
        }

        // Debt Person filter
        if (filters.DebtPersonId.HasValue && filters.DebtPersonId.Value != Guid.Empty)
        {
            debts = debts.Where(d => d.DebtPersonId == filters.DebtPersonId.Value).ToList();
        }

        // Amount filters
        if (filters.MinAmount.HasValue)
        {
            debts = debts.Where(d => d.Amount >= filters.MinAmount.Value).ToList();
        }

        if (filters.MaxAmount.HasValue)
        {
            debts = debts.Where(d => d.Amount <= filters.MaxAmount.Value).ToList();
        }
        
        var debtPersonIds = debts.Select(d => d.DebtPersonId).Distinct().ToList();
        var debtPersons = new Dictionary<Guid, string>();
        
        foreach(var personId in debtPersonIds)
        {
            var person = await _noSqlUnitOfWork.DebtPersons.GetByIdAsync(personId);
            if (person != null)
            {
                debtPersons[personId] = person.FullName;
            }
        }

        var dtos = debts.Select(d => new DebtDto
        {
            Id = d.Id,
            UserId = d.UserId,
            Amount = d.Amount,
            DateOfBorrow = d.DateOfBorrow,
            DateOfReturn = d.DateOfReturn,
            PromissedReturnDate = d.PromissedReturnDate,
            Profit = d.Profit,
            DebtPersonId = d.DebtPersonId,
            DebtPersonName = debtPersons.TryGetValue(d.DebtPersonId, out var name) ? name : "Unknown",
            Description = d.Description,
            IsDone = d.IsDone,
            CreatedOn = d.CreatedOn
        }).AsQueryable();

        if (!string.IsNullOrWhiteSpace(filters.SearchTerm))
        {
            var term = filters.SearchTerm.ToLower();
            dtos = dtos.Where(d => (d.Description != null && d.Description.ToLower().Contains(term)) ||
                                   (d.DebtPersonName != null && d.DebtPersonName.ToLower().Contains(term)));
        }

        if (filters.IsDone.HasValue)
        {
            dtos = dtos.Where(d => d.IsDone == filters.IsDone.Value);
        }

        string sortBy = (filters.SortBy ?? "dateofborrow").ToLower();
        bool desc = filters.SortDescending;

        dtos = sortBy switch
        {
            "amount" => desc ? dtos.OrderByDescending(d => d.Amount) : dtos.OrderBy(d => d.Amount),
            "debtpersonname" or "person" => desc ? dtos.OrderByDescending(d => d.DebtPersonName) : dtos.OrderBy(d => d.DebtPersonName),
            "dateofreturn" or "returndate" => desc ? dtos.OrderByDescending(d => d.DateOfReturn) : dtos.OrderBy(d => d.DateOfReturn),
            "profit" => desc ? dtos.OrderByDescending(d => d.Profit) : dtos.OrderBy(d => d.Profit),
            "createdon" or "createdat" => desc ? dtos.OrderByDescending(d => d.CreatedOn) : dtos.OrderBy(d => d.CreatedOn),
            _ => desc ? dtos.OrderByDescending(d => d.DateOfBorrow) : dtos.OrderBy(d => d.DateOfBorrow)
        };

        var dtoList = dtos.ToList();
        int totalCount = dtoList.Count;
        int currentPage = filters.PageNumber < 1 ? 1 : filters.PageNumber;
        int pageSize = filters.PageSize < 1 ? 10 : filters.PageSize;

        var items = (filters.PageSize <= 0)
            ? dtoList
            : dtoList.Skip((currentPage - 1) * pageSize).Take(pageSize).ToList();
        return new PagedResult<DebtDto, GetAllDebtsFilters>(items, totalCount, currentPage, pageSize, filters);
    }
}
