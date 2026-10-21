using System;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Crypto.Services;
using AzureTest.Application.Messaging;
using AzureTest.Domain.Entities.Crypto;
using FluentValidation;

namespace AzureTest.Application.Crypto.Commands.TriggerKillSwitch;

public sealed record TriggerKillSwitchCommand(Guid AccountId, string Reason) : ICommand<Result<int>>;

public sealed class TriggerKillSwitchCommandValidator : AbstractValidator<TriggerKillSwitchCommand>
{
    public TriggerKillSwitchCommandValidator()
    {
        RuleFor(x => x.AccountId).NotEmpty();
        RuleFor(x => x.Reason)
            .NotEmpty()
            .MaximumLength(CryptoAccount.Rules.ReasonMaxLength);
    }
}

public sealed class TriggerKillSwitchCommandHandler : ICommandHandler<TriggerKillSwitchCommand, Result<int>>
{
    private readonly ICryptoExecutionService _executionService;

    public TriggerKillSwitchCommandHandler(ICryptoExecutionService executionService)
    {
        _executionService = executionService;
    }

    public async Task<Result<int>> HandleAsync(TriggerKillSwitchCommand cmd, CancellationToken ct)
    {
        return await _executionService.TriggerKillSwitchAsync(cmd.AccountId, cmd.Reason, ct);
    }
}
