using AzureTest.Application.Abstractions;

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using AzureTest.Application.Interfaces.Data;
using AzureTest.Application.Interfaces.Data.NoSql;
using AzureTest.Application.Interfaces.Services;
using AzureTest.Application.Messaging;
using AzureTest.Domain.Documents;
using AzureTest.Domain.Entities;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace AzureTest.Application.Debts.Commands;

// ── Command ───────────────────────────────────────────────────────────────────

public sealed record CreateDebtCommand(
    decimal Amount,
    DateTime DateOfBorrow,
    DateTime? DateOfReturn,
    DateTime? PromissedReturnDate,
    decimal Profit,
    Guid? DebtPersonId,
    string? NewDebtPersonName,
    string? Description,
    bool IsDone
) : ICommand<Result<Guid>>;

// ── Validator ─────────────────────────────────────────────────────────────────

public sealed class CreateDebtCommandValidator : AbstractValidator<CreateDebtCommand>
{
    public CreateDebtCommandValidator()
    {
        RuleFor(x => x.Amount).GreaterThan(0).WithMessage("Amount must be greater than zero.");
        RuleFor(x => x.DateOfBorrow).NotEmpty().WithMessage("Date of borrow is required.");
        RuleFor(x => x.Profit).GreaterThanOrEqualTo(0).WithMessage("Profit must be greater than or equal to zero.");
        
        RuleFor(x => x)
            .Must(x => x.DebtPersonId.HasValue || !string.IsNullOrWhiteSpace(x.NewDebtPersonName))
            .WithMessage("Either an existing DebtPerson must be selected, or a new DebtPerson name must be provided.");
    }
}

// ── Handler ───────────────────────────────────────────────────────────────────

public sealed class CreateDebtCommandHandler : BaseCommandHandler<CreateDebtCommand, Result<Guid>>
{
    private readonly INoSqlUnitOfWork _noSqlUnitOfWork;
    private readonly ICurrentUser _currentUser;

    public CreateDebtCommandHandler(
        IUnitOfWork unitOfWork,
        INoSqlUnitOfWork noSqlUnitOfWork,
        ICurrentUser currentUser,
        ILogger<CreateDebtCommandHandler> logger)
        : base(unitOfWork, logger)
    {
        _noSqlUnitOfWork = noSqlUnitOfWork;
        _currentUser = currentUser;
    }

    protected override async Task<Result<Guid>> ExecuteAsync(CreateDebtCommand command, CancellationToken cancellationToken)
    {
        Guid? debtPersonId = command.DebtPersonId;

        if (debtPersonId is null && !string.IsNullOrWhiteSpace(command.NewDebtPersonName))
        {
            var searchResults = await _noSqlUnitOfWork.DebtPersons.SearchByNameAsync(command.NewDebtPersonName.Trim());
            var existing = searchResults.FirstOrDefault(x => string.Equals(x.FullName, command.NewDebtPersonName.Trim(), StringComparison.OrdinalIgnoreCase));
            
            if (existing is not null)
            {
                debtPersonId = existing.Id;
            }
            else
            {
                var newDebtPerson = new DebtPerson
                {
                    UserId = _currentUser.Id,
                    FullName = command.NewDebtPersonName.Trim()
                };
                await _noSqlUnitOfWork.DebtPersons.AddAsync(newDebtPerson);
                debtPersonId = newDebtPerson.Id;
            }
        }

        var debt = new Debt
        {
            UserId = _currentUser.Id,
            Amount = command.Amount,
            DateOfBorrow = command.DateOfBorrow.ToUniversalTime(),
            DateOfReturn = command.DateOfReturn?.ToUniversalTime(),
            PromissedReturnDate = command.PromissedReturnDate?.ToUniversalTime(),
            Profit = command.Profit,
            DebtPersonId = debtPersonId!.Value,
            Description = command.Description,
            IsDone = command.IsDone,
            CreatedOn = DateTime.UtcNow,
            CreatedBy = _currentUser.Id.ToString()
        };

        UnitOfWork.Debts.Add(debt);

        return await Task.FromResult(debt.Id);
    }
}
