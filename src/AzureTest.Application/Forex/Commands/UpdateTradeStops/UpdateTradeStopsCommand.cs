using System;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Interfaces.Data;
using AzureTest.Application.Interfaces.Services;
using AzureTest.Application.Messaging;
using AzureTest.Domain.Entities.Forex;
using FluentValidation;

namespace AzureTest.Application.Forex.Commands.UpdateTradeStops;

public sealed record UpdateTradeStopsCommand(
    Guid TradeId,
    decimal? StopLoss,
    decimal? TakeProfit,
    decimal TrailingStopPips = 0m,
    bool TriggerBreakEven = false
) : ICommand<Result>;

public sealed class UpdateTradeStopsCommandValidator : AbstractValidator<UpdateTradeStopsCommand>
{
    public UpdateTradeStopsCommandValidator()
    {
        RuleFor(x => x.TradeId).NotEmpty();
        RuleFor(x => x.TrailingStopPips).GreaterThanOrEqualTo(0m);
    }
}

public sealed class UpdateTradeStopsCommandHandler : ICommandHandler<UpdateTradeStopsCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUser _currentUser;

    public UpdateTradeStopsCommandHandler(IUnitOfWork unitOfWork, ICurrentUser currentUser)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task<Result> HandleAsync(UpdateTradeStopsCommand command, CancellationToken cancellationToken)
    {
        var trade = await _unitOfWork.ForexTrades.GetByIdAsync(command.TradeId);
        if (trade == null) return Result.Failure("Trade not found.");
        if (trade.UserId != _currentUser.Id) return Result.Failure("Unauthorized access to trade.");

        trade.UpdateStops(command.StopLoss, command.TakeProfit, command.TrailingStopPips);

        if (command.TriggerBreakEven)
        {
            trade.TriggerBreakEven();
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
