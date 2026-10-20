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

        // Security: Non-admin users are restricted to their own userId
        if (!isAdmin)
        {
            filters.UserIds = new List<Guid> { _currentUser.Id };
        }
        else if (filters.UserIds == null || !filters.UserIds.Any())
        {
            if (query.UserId.HasValue)
            {
                filters.UserIds = new List<Guid> { query.UserId.Value };
            }
        }

        List<Guid>? matchingPersonIds = null;
        if (!string.IsNullOrWhiteSpace(filters.SearchTerm))
        {
            var matchingPersons = await _noSqlUnitOfWork.DebtPersons.SearchByNameAsync(filters.SearchTerm);
            matchingPersonIds = matchingPersons.Select(p => p.Id).ToList();
        }

        // Domain-specific predicate for Debts
        System.Linq.Expressions.Expression<Func<Debt, bool>> predicate = d =>
            (!filters.DebtPersonId.HasValue || filters.DebtPersonId == Guid.Empty || d.DebtPersonId == filters.DebtPersonId.Value) &&
            (!filters.MinAmount.HasValue || d.Amount >= filters.MinAmount.Value) &&
            (!filters.MaxAmount.HasValue || d.Amount <= filters.MaxAmount.Value) &&
            (!filters.IsDone.HasValue || d.IsDone == filters.IsDone.Value) &&
            (string.IsNullOrWhiteSpace(filters.SearchTerm) ||
             (d.Description != null && d.Description.Contains(filters.SearchTerm)) ||
             (matchingPersonIds != null && matchingPersonIds.Contains(d.DebtPersonId)));

        if (string.IsNullOrWhiteSpace(filters.SortBy))
        {
            filters.SortBy = "DateOfBorrow";
            filters.SortDescending = true;
        }

        var pagedDebts = await _unitOfWork.Debts.GetPagedAsync(filters, predicate, cancellationToken);

        var debtPersonIds = pagedDebts.Items.Select(d => d.DebtPersonId).Distinct().ToList();
        var debtPersons = new Dictionary<Guid, string>();
        foreach (var personId in debtPersonIds)
        {
            var person = await _noSqlUnitOfWork.DebtPersons.GetByIdAsync(personId, cancellationToken);
            if (person != null)
            {
                debtPersons[personId] = person.FullName;
            }
        }

        var dtos = pagedDebts.Items.Select(d => new DebtDto
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
        }).ToList();

        return new PagedResult<DebtDto, GetAllDebtsFilters>(
            dtos,
            pagedDebts.TotalCount,
            pagedDebts.PageNumber,
            pagedDebts.PageSize,
            filters);
    }
}
