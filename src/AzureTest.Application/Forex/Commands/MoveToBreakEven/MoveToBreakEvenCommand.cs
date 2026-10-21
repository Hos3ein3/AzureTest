using System;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Forex.Services;
using AzureTest.Application.Interfaces.Data;
using AzureTest.Application.Interfaces.Services;
using AzureTest.Application.Messaging;
using AzureTest.Domain.Entities.Forex.Enums;
using FluentValidation;

namespace AzureTest.Application.Forex.Commands.MoveToBreakEven;

public sealed record MoveToBreakEvenCommand(Guid TradeId) : ICommand<Result<bool>>;

public sealed class MoveToBreakEvenCommandValidator : AbstractValidator<MoveToBreakEvenCommand>
{
    public MoveToBreakEvenCommandValidator()
    {
        RuleFor(x => x.TradeId).NotEmpty();
    }
}

public sealed class MoveToBreakEvenCommandHandler : ICommandHandler<MoveToBreakEvenCommand, Result<bool>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IForexMarketDataService _marketData;
    private readonly IForexExecutionService _execution;
    private readonly ICurrentUser _currentUser;

    public MoveToBreakEvenCommandHandler(
        IUnitOfWork unitOfWork,
        IForexMarketDataService marketData,
        IForexExecutionService execution,
        ICurrentUser currentUser)
    {
        _unitOfWork = unitOfWork;
        _marketData = marketData;
        _execution = execution;
        _currentUser = currentUser;
    }

    public async Task<Result<bool>> HandleAsync(MoveToBreakEvenCommand command, CancellationToken cancellationToken)
    {
        var trade = await _unitOfWork.ForexTrades.GetByIdAsync(command.TradeId);
        if (trade == null) return Result<bool>.Failure("Trade not found.");
        if (trade.UserId != _currentUser.Id) return Result<bool>.Failure("Unauthorized access to trade.");

        var account = await _unitOfWork.ForexAccounts.GetByIdAsync(trade.AccountId);
        if (account == null) return Result<bool>.Failure("Account not found.");

        var quote = await _marketData.GetQuoteAsync(trade.Symbol, cancellationToken);
        var currentMarketPrice = trade.Direction == ForexTradeDirection.Buy ? quote.Bid : quote.Ask;

        var result = await _execution.MoveToBreakEvenAsync(account, trade, currentMarketPrice, cancellationToken);
        if (result.IsFailure) return result;

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return result;
    }
}
