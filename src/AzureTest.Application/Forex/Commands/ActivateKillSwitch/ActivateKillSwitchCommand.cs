using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Forex.Services;
using AzureTest.Application.Interfaces.Data;
using AzureTest.Application.Interfaces.Services;
using AzureTest.Application.Messaging;
using FluentValidation;

namespace AzureTest.Application.Forex.Commands.ActivateKillSwitch;

public sealed record ActivateKillSwitchCommand(string Reason = "Emergency Manual Kill Switch Triggered") : ICommand<Result<int>>;

public sealed class ActivateKillSwitchCommandValidator : AbstractValidator<ActivateKillSwitchCommand>
{
    public ActivateKillSwitchCommandValidator()
    {
        RuleFor(x => x.Reason).NotEmpty();
    }
}

public sealed class ActivateKillSwitchCommandHandler : ICommandHandler<ActivateKillSwitchCommand, Result<int>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IForexExecutionService _execution;
    private readonly ICurrentUser _currentUser;

    public ActivateKillSwitchCommandHandler(
        IUnitOfWork unitOfWork,
        IForexExecutionService execution,
        ICurrentUser currentUser)
    {
        _unitOfWork = unitOfWork;
        _execution = execution;
        _currentUser = currentUser;
    }

    public async Task<Result<int>> HandleAsync(ActivateKillSwitchCommand command, CancellationToken cancellationToken)
    {
        var accounts = await _unitOfWork.ForexAccounts.FindAsync(a => a.UserId == _currentUser.Id);
        var account = accounts.FirstOrDefault();
        if (account == null) return Result<int>.Failure("No active Forex account found.");

        var result = await _execution.ExecuteKillSwitchAsync(account, command.Reason, cancellationToken);
        if (result.IsFailure) return result;

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return result;
    }
}
