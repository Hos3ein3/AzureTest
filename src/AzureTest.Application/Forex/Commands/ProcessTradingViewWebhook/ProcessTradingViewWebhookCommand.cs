using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Forex.Services;
using AzureTest.Application.Interfaces.Data;
using AzureTest.Application.Messaging;
using AzureTest.Contracts.DTOs.Forex;
using FluentValidation;

namespace AzureTest.Application.Forex.Commands.ProcessTradingViewWebhook;

public sealed record ProcessTradingViewWebhookCommand(TradingViewWebhookPayloadDto Payload) : ICommand<Result<Guid>>;

public sealed class ProcessTradingViewWebhookCommandValidator : AbstractValidator<ProcessTradingViewWebhookCommand>
{
    public ProcessTradingViewWebhookCommandValidator()
    {
        RuleFor(x => x.Payload).NotNull();
        RuleFor(x => x.Payload.Ticker).NotEmpty();
        RuleFor(x => x.Payload.Action).NotEmpty();
        RuleFor(x => x.Payload.SecretToken).NotEmpty();
    }
}

public sealed class ProcessTradingViewWebhookCommandHandler : ICommandHandler<ProcessTradingViewWebhookCommand, Result<Guid>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IForexExecutionService _execution;

    public ProcessTradingViewWebhookCommandHandler(
        IUnitOfWork unitOfWork,
        IForexExecutionService execution)
    {
        _unitOfWork = unitOfWork;
        _execution = execution;
    }

    public async Task<Result<Guid>> HandleAsync(ProcessTradingViewWebhookCommand command, CancellationToken cancellationToken)
    {
        // For webhook signals, bind to the primary active Forex demo account
        var accounts = await _unitOfWork.ForexAccounts.FindAsync(a => a.IsDemo && !a.IsDeleted);
        var account = accounts.FirstOrDefault();
        if (account == null)
        {
            var anyAccount = (await _unitOfWork.ForexAccounts.GetAllAsync()).FirstOrDefault();
            if (anyAccount == null) return Result<Guid>.Failure("No Forex account configured to receive signals.");
            account = anyAccount;
        }

        var result = await _execution.ProcessTradingViewWebhookAsync(command.Payload, account, cancellationToken);
        if (result.IsFailure) return result;

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return result;
    }
}
