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

namespace AzureTest.Application.Forex.Commands.PartialCloseTrade;

public sealed record PartialCloseTradeCommand(Guid TradeId, decimal LotsToClose) : ICommand<Result<decimal>>;

public sealed class PartialCloseTradeCommandValidator : AbstractValidator<PartialCloseTradeCommand>
{
    public PartialCloseTradeCommandValidator()
    {
        RuleFor(x => x.TradeId).NotEmpty();
        RuleFor(x => x.LotsToClose).GreaterThan(0m).WithMessage("Lots to close must be greater than 0.");
    }
}

public sealed class PartialCloseTradeCommandHandler : ICommandHandler<PartialCloseTradeCommand, Result<decimal>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IForexMarketDataService _marketData;
    private readonly IForexExecutionService _execution;
    private readonly ICurrentUser _currentUser;

    public PartialCloseTradeCommandHandler(
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

    public async Task<Result<decimal>> HandleAsync(PartialCloseTradeCommand command, CancellationToken cancellationToken)
    {
        var trade = await _unitOfWork.ForexTrades.GetByIdAsync(command.TradeId);
        if (trade == null) return Result<decimal>.Failure("Trade not found.");
        if (trade.UserId != _currentUser.Id) return Result<decimal>.Failure("Unauthorized access to trade.");

        var account = await _unitOfWork.ForexAccounts.GetByIdAsync(trade.AccountId);
        if (account == null) return Result<decimal>.Failure("Account not found.");

        var quote = await _marketData.GetQuoteAsync(trade.Symbol, cancellationToken);
        var currentMarketPrice = trade.Direction == ForexTradeDirection.Buy ? quote.Bid : quote.Ask;

        var result = await _execution.ExecutePartialCloseAsync(account, trade, command.LotsToClose, currentMarketPrice, cancellationToken);
        if (result.IsFailure) return result;

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return result;
    }
}
