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
using AzureTest.Domain.Entities;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace AzureTest.Application.Debts.Queries;

// ── Query ─────────────────────────────────────────────────────────────────────

public sealed record GetAllDebtsQuery(Guid? UserId = null) : IQuery<IReadOnlyList<DebtDto>>;

// ── Handler ───────────────────────────────────────────────────────────────────

public sealed class GetAllDebtsQueryHandler : BaseQueryHandler<GetAllDebtsQuery, IReadOnlyList<DebtDto>>
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

    protected override async Task<IReadOnlyList<DebtDto>> ExecuteAsync(GetAllDebtsQuery query, CancellationToken cancellationToken)
    {
        var targetUserId = query.UserId ?? _currentUser.Id;
        
        var queryable = await _unitOfWork.Debts.FindAsync(r => !r.IsDeleted);
        var debts = queryable.ToList();
        
        if (query.UserId.HasValue || !_currentUser.Roles.Contains("Admin"))
        {
            debts = debts.Where(r => r.UserId == targetUserId).ToList();
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

        var result = debts.Select(d => new DebtDto
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
            IsDone = d.IsDone
        }).OrderByDescending(d => d.DateOfBorrow).ToList();

        return result;
    }
}
