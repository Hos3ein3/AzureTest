using System;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Interfaces.Data;
using AzureTest.Application.Interfaces.Data.NoSql;
using AzureTest.Application.Interfaces.Services;
using AzureTest.Application.Messaging;
using AzureTest.Contracts.DTOs.Debts;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace AzureTest.Application.Debts.Queries;

// ── Query ─────────────────────────────────────────────────────────────────────

public sealed record GetDebtByIdQuery(Guid Id) : IQuery<DebtDto?>;

// ── Validator ─────────────────────────────────────────────────────────────────

public sealed class GetDebtByIdQueryValidator : AbstractValidator<GetDebtByIdQuery>
{
    public GetDebtByIdQueryValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage("Debt ID is required.");
    }
}

// ── Handler ───────────────────────────────────────────────────────────────────

public sealed class GetDebtByIdQueryHandler : BaseQueryHandler<GetDebtByIdQuery, DebtDto?>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly INoSqlUnitOfWork _noSqlUnitOfWork;
    private readonly ICurrentUser _currentUser;

    public GetDebtByIdQueryHandler(
        IUnitOfWork unitOfWork,
        INoSqlUnitOfWork noSqlUnitOfWork,
        ICurrentUser currentUser,
        ILogger<GetDebtByIdQueryHandler> logger)
        : base(logger)
    {
        _unitOfWork = unitOfWork;
        _noSqlUnitOfWork = noSqlUnitOfWork;
        _currentUser = currentUser;
    }

    protected override async Task<DebtDto?> ExecuteAsync(GetDebtByIdQuery query, CancellationToken cancellationToken)
    {
        var debt = await _unitOfWork.Debts.GetByIdAsync(query.Id);

        if (debt == null || debt.IsDeleted || debt.UserId != _currentUser.Id)
        {
            return null;
        }

        string debtPersonName = "Unknown";
        var person = await _noSqlUnitOfWork.DebtPersons.GetByIdAsync(debt.DebtPersonId);
        if (person != null)
        {
            debtPersonName = person.FullName;
        }

        return new DebtDto
        {
            Id = debt.Id,
            UserId = debt.UserId,
            Amount = debt.Amount,
            DateOfBorrow = debt.DateOfBorrow,
            DateOfReturn = debt.DateOfReturn,
            PromissedReturnDate = debt.PromissedReturnDate,
            Profit = debt.Profit,
            DebtPersonId = debt.DebtPersonId,
            DebtPersonName = debtPersonName,
            Description = debt.Description,
            IsDone = debt.IsDone
        };
    }
}
