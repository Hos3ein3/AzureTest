using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Interfaces.Data;
using AzureTest.Application.Interfaces.Data.NoSql;
using AzureTest.Application.Interfaces.Services;
using AzureTest.Application.Messaging;
using AzureTest.Domain.Documents;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace AzureTest.Application.Debts.Commands;

// ── Command ───────────────────────────────────────────────────────────────────

public sealed record UpdateDebtCommand(
    Guid Id,
    decimal Amount,
    DateTime DateOfBorrow,
    DateTime? DateOfReturn,
    DateTime? PromissedReturnDate,
    decimal Profit,
    Guid? DebtPersonId,
    string? NewDebtPersonName,
    string? Description,
    bool IsDone
) : ICommand<bool>;

// ── Validator ─────────────────────────────────────────────────────────────────

public sealed class UpdateDebtCommandValidator : AbstractValidator<UpdateDebtCommand>
{
    public UpdateDebtCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage("Debt ID is required.");
        RuleFor(x => x.Amount).GreaterThan(0).WithMessage("Amount must be greater than zero.");
        RuleFor(x => x.DateOfBorrow).NotEmpty().WithMessage("Date of borrow is required.");
        RuleFor(x => x.Profit).GreaterThanOrEqualTo(0).WithMessage("Profit must be greater than or equal to zero.");
        
        RuleFor(x => x)
            .Must(x => x.DebtPersonId.HasValue || !string.IsNullOrWhiteSpace(x.NewDebtPersonName))
            .WithMessage("Either an existing DebtPerson must be selected, or a new DebtPerson name must be provided.");
    }
}

// ── Handler ───────────────────────────────────────────────────────────────────

public sealed class UpdateDebtCommandHandler : BaseCommandHandler<UpdateDebtCommand, bool>
{
    private readonly INoSqlUnitOfWork _noSqlUnitOfWork;
    private readonly ICurrentUser _currentUser;

    public UpdateDebtCommandHandler(
        IUnitOfWork unitOfWork,
        INoSqlUnitOfWork noSqlUnitOfWork,
        ICurrentUser currentUser,
        ILogger<UpdateDebtCommandHandler> logger)
        : base(unitOfWork, logger)
    {
        _noSqlUnitOfWork = noSqlUnitOfWork;
        _currentUser = currentUser;
    }

    protected override async Task<bool> ExecuteAsync(UpdateDebtCommand command, CancellationToken cancellationToken)
    {
        var debt = await UnitOfWork.Debts.GetByIdAsync(command.Id);
        if (debt == null)
            return false;

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

        debt.Amount = command.Amount;
        debt.DateOfBorrow = command.DateOfBorrow;
        debt.DateOfReturn = command.DateOfReturn;
        debt.PromissedReturnDate = command.PromissedReturnDate;
        debt.Profit = command.Profit;
        debt.DebtPersonId = debtPersonId!.Value;
        debt.Description = command.Description;
        debt.IsDone = command.IsDone;
        debt.ModifiedOn = DateTime.UtcNow;
        debt.ModifiedBy = _currentUser.Id.ToString();

        UnitOfWork.Debts.Update(debt);

        return await Task.FromResult(true);
    }
}
