using AzureTest.Application.Abstractions;

using System;
using System.Threading;
using System.Threading.Tasks;

using AzureTest.Application.Interfaces.Data;
using AzureTest.Application.Interfaces.Services;
using AzureTest.Application.Messaging;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace AzureTest.Application.Debts.Commands;

// ── Command ───────────────────────────────────────────────────────────────────

public sealed record DeleteDebtCommand(Guid Id) : ICommand<Result<bool>>;

// ── Validator ─────────────────────────────────────────────────────────────────

public sealed class DeleteDebtCommandValidator : AbstractValidator<DeleteDebtCommand>
{
    public DeleteDebtCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage("Debt ID is required.");
    }
}

// ── Handler ───────────────────────────────────────────────────────────────────

public sealed class DeleteDebtCommandHandler : BaseCommandHandler<DeleteDebtCommand, Result<bool>>
{
    private readonly ICurrentUser _currentUser;

    public DeleteDebtCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUser currentUser,
        ILogger<DeleteDebtCommandHandler> logger)
        : base(unitOfWork, logger)
    {
        _currentUser = currentUser;
    }

    protected override async Task<Result<bool>> ExecuteAsync(DeleteDebtCommand command, CancellationToken cancellationToken)
    {
        var debt = await UnitOfWork.Debts.GetByIdAsync(command.Id);
        if (debt == null)
            return false;

        debt.IsDeleted = true;
        debt.DeletedOn = DateTime.UtcNow;
        debt.DeletedBy = _currentUser.Id.ToString();

        UnitOfWork.Debts.Update(debt);

        return await Task.FromResult(true);
    }
}
