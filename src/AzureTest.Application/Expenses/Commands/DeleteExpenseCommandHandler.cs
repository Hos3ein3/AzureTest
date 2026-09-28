using System;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Interfaces.Data;
using AzureTest.Application.Interfaces.Services;
using AzureTest.Application.Messaging;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace AzureTest.Application.Expenses.Commands;

// ── Command ───────────────────────────────────────────────────────────────────

public sealed record DeleteExpenseCommand(Guid Id) : ICommand<bool>;

// ── Validator ─────────────────────────────────────────────────────────────────

public sealed class DeleteExpenseCommandValidator : AbstractValidator<DeleteExpenseCommand>
{
    public DeleteExpenseCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage("Id is required.");
    }
}

// ── Handler ───────────────────────────────────────────────────────────────────

public sealed class DeleteExpenseCommandHandler : BaseCommandHandler<DeleteExpenseCommand, bool>
{
    private readonly ICurrentUser _currentUser;

    public DeleteExpenseCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUser currentUser,
        ILogger<DeleteExpenseCommandHandler> logger)
        : base(unitOfWork, logger)
    {
        _currentUser = currentUser;
    }

    protected override async Task<bool> ExecuteAsync(DeleteExpenseCommand command, CancellationToken cancellationToken)
    {
        var expense = await UnitOfWork.Expenses.GetByIdAsync(command.Id);
        if (expense == null || expense.UserId != _currentUser.Id)
        {
            return false;
        }

        expense.SoftDelete(_currentUser.Id.ToString());
        UnitOfWork.Expenses.Update(expense);

        return true;
    }
}
