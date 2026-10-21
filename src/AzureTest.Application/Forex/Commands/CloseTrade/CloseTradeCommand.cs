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

namespace AzureTest.Application.Forex.Commands.CloseTrade;

public sealed record CloseTradeCommand(Guid TradeId, ForexExitReason Reason = ForexExitReason.Manual) : ICommand<Result<decimal>>;

public sealed class CloseTradeCommandValidator : AbstractValidator<CloseTradeCommand>
{
    public CloseTradeCommandValidator()
    {
        RuleFor(x => x.TradeId).NotEmpty();
    }
}

public sealed class CloseTradeCommandHandler : ICommandHandler<CloseTradeCommand, Result<decimal>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IForexMarketDataService _marketData;
    private readonly IForexExecutionService _execution;
    private readonly ICurrentUser _currentUser;

    public CloseTradeCommandHandler(
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

    public async Task<Result<decimal>> HandleAsync(CloseTradeCommand command, CancellationToken cancellationToken)
    {
        var trade = await _unitOfWork.ForexTrades.GetByIdAsync(command.TradeId);
        if (trade == null) return Result<decimal>.Failure("Trade not found.");
        if (trade.UserId != _currentUser.Id) return Result<decimal>.Failure("Unauthorized access to trade.");

        var account = await _unitOfWork.ForexAccounts.GetByIdAsync(trade.AccountId);
        if (account == null) return Result<decimal>.Failure("Account not found.");

        var quote = await _marketData.GetQuoteAsync(trade.Symbol, cancellationToken);
        var currentMarketPrice = trade.Direction == ForexTradeDirection.Buy ? quote.Bid : quote.Ask;

        var closeResult = await _execution.ClosePositionAsync(account, trade, currentMarketPrice, command.Reason, cancellationToken);
        if (closeResult.IsFailure) return closeResult;

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result<decimal>.Success(closeResult.Value);
    }
}
