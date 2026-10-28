using System;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Interfaces.Data;
using AzureTest.Application.Messaging;
using AzureTest.Application.Trading.Services;
using AzureTest.Domain.Entities.Trading.Enums;
using FluentValidation;

namespace AzureTest.Application.Trading.Commands.ModifyTradingPosition;

public sealed record ModifyTradingPositionCommand(
    Guid PositionId,
    Guid UserId,
    Guid TenantId,
    decimal? StopLoss = null,
    decimal? TakeProfit = null,
    decimal? TrailingStopDistance = null,
    string? MultipleTakeProfitsJson = null) : ICommand<Result<Guid>>;

public sealed class ModifyTradingPositionCommandValidator : AbstractValidator<ModifyTradingPositionCommand>
{
    public ModifyTradingPositionCommandValidator()
    {
        RuleFor(x => x.PositionId).NotEmpty();
        When(x => x.StopLoss.HasValue && x.StopLoss.Value > 0, () =>
        {
            RuleFor(x => x.StopLoss!.Value).GreaterThan(0m);
        });
        When(x => x.TakeProfit.HasValue && x.TakeProfit.Value > 0, () =>
        {
            RuleFor(x => x.TakeProfit!.Value).GreaterThan(0m);
        });
        When(x => x.TrailingStopDistance.HasValue, () =>
        {
            RuleFor(x => x.TrailingStopDistance!.Value).GreaterThan(0m);
        });
    }
}

public sealed class ModifyTradingPositionCommandHandler : ICommandHandler<ModifyTradingPositionCommand, Result<Guid>>
{
    private readonly IUnitOfWork _uow;
    private readonly ITradingMarketDataService _marketData;

    public ModifyTradingPositionCommandHandler(IUnitOfWork uow, ITradingMarketDataService marketData)
    {
        _uow = uow;
        _marketData = marketData;
    }

    public async Task<Result<Guid>> HandleAsync(ModifyTradingPositionCommand cmd, CancellationToken ct)
    {
        var position = await _uow.TradingPositions.GetByIdAsync(cmd.PositionId);
        if (position == null || position.Status != TradingPositionStatus.Open || position.UserId != cmd.UserId || (cmd.TenantId != Guid.Empty && position.TenantId != cmd.TenantId))
        {
            return Result<Guid>.Failure("Active position not found or already closed.");
        }

        var currentPrice = await _marketData.GetCurrentPriceAsync(position.Symbol, position.CurrentPrice, ct);
        position.UpdateCurrentPrice(currentPrice);

        decimal? effectiveSl = (cmd.StopLoss.HasValue && cmd.StopLoss.Value > 0) ? cmd.StopLoss : null;
        decimal? effectiveTp = (cmd.TakeProfit.HasValue && cmd.TakeProfit.Value > 0) ? cmd.TakeProfit : null;

        if (position.Side == TradingOrderSide.Buy)
        {
            if (effectiveSl.HasValue && effectiveSl.Value >= currentPrice)
            {
                return Result<Guid>.Failure($"For Buy (Long) position, Stop Loss (${effectiveSl.Value.ToString("#,##0.00000", System.Globalization.CultureInfo.InvariantCulture)}) must be strictly below current market price (${currentPrice.ToString("#,##0.00000", System.Globalization.CultureInfo.InvariantCulture)}).");
            }
            if (effectiveTp.HasValue && effectiveTp.Value <= currentPrice)
            {
                return Result<Guid>.Failure($"For Buy (Long) position, Take Profit (${effectiveTp.Value.ToString("#,##0.00000", System.Globalization.CultureInfo.InvariantCulture)}) must be strictly above current market price (${currentPrice.ToString("#,##0.00000", System.Globalization.CultureInfo.InvariantCulture)}).");
            }
        }
        else // Sell (Short)
        {
            if (effectiveSl.HasValue && effectiveSl.Value <= currentPrice)
            {
                return Result<Guid>.Failure($"For Sell (Short) position, Stop Loss (${effectiveSl.Value.ToString("#,##0.00000", System.Globalization.CultureInfo.InvariantCulture)}) must be strictly above current market price (${currentPrice.ToString("#,##0.00000", System.Globalization.CultureInfo.InvariantCulture)}).");
            }
            if (effectiveTp.HasValue && effectiveTp.Value >= currentPrice)
            {
                return Result<Guid>.Failure($"For Sell (Short) position, Take Profit (${effectiveTp.Value.ToString("#,##0.00000", System.Globalization.CultureInfo.InvariantCulture)}) must be strictly below current market price (${currentPrice.ToString("#,##0.00000", System.Globalization.CultureInfo.InvariantCulture)}).");
            }
        }

        position.UpdateStops(effectiveSl, effectiveTp, cmd.TrailingStopDistance, cmd.MultipleTakeProfitsJson);

        await _uow.SaveChangesAsync(ct);
        return Result<Guid>.Success(position.Id);
    }
}
