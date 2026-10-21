using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Interfaces.Data;
using AzureTest.Application.Interfaces.Services;
using AzureTest.Application.Messaging;
using AzureTest.Domain.Entities.Forex;
using FluentValidation;

namespace AzureTest.Application.Forex.Commands.ResetDemoAccount;

public sealed record ResetDemoAccountCommand(decimal InitialBalance = 10_000m) : ICommand<Result>;

public sealed class ResetDemoAccountCommandValidator : AbstractValidator<ResetDemoAccountCommand>
{
    public ResetDemoAccountCommandValidator()
    {
        RuleFor(x => x.InitialBalance)
            .InclusiveBetween(ForexAccount.Rules.InitialBalanceMin, ForexAccount.Rules.InitialBalanceMax);
    }
}

public sealed class ResetDemoAccountCommandHandler : ICommandHandler<ResetDemoAccountCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUser _currentUser;

    public ResetDemoAccountCommandHandler(IUnitOfWork unitOfWork, ICurrentUser currentUser)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task<Result> HandleAsync(ResetDemoAccountCommand command, CancellationToken cancellationToken)
    {
        var accounts = await _unitOfWork.ForexAccounts.FindAsync(a => a.UserId == _currentUser.Id);
        var account = accounts.FirstOrDefault();

        if (account == null)
        {
            account = ForexAccount.Create(_currentUser.Id, "Demo Primary", "USD", command.InitialBalance, 30, true);
            _unitOfWork.ForexAccounts.Add(account);
        }
        else
        {
            account.ResetAccount(command.InitialBalance);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
