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

        string? cleanMultiTp = string.IsNullOrWhiteSpace(cmd.MultipleTakeProfitsJson) ? null : cmd.MultipleTakeProfitsJson.Trim();
        if (cleanMultiTp != null)
        {
            try
            {
                using var doc = System.Text.Json.JsonDocument.Parse(cleanMultiTp);
                if (doc.RootElement.ValueKind == System.Text.Json.JsonValueKind.Array)
                {
                    decimal totalPct = 0;
                    decimal? firstLevelPrice = null;
                    foreach (var el in doc.RootElement.EnumerateArray())
                    {
                        if (el.TryGetProperty("price", out var pProp) && pProp.TryGetDecimal(out var tpPrice))
                        {
                            firstLevelPrice ??= tpPrice;
                            if (position.Side == TradingOrderSide.Buy && tpPrice <= currentPrice)
                            {
                                return Result<Guid>.Failure($"Multi-level Take Profit price (${tpPrice.ToString("#,##0.00######", System.Globalization.CultureInfo.InvariantCulture)}) must be above current price (${currentPrice.ToString("#,##0.00######", System.Globalization.CultureInfo.InvariantCulture)}) for Buy position.");
                            }
                            if (position.Side == TradingOrderSide.Sell && tpPrice >= currentPrice)
                            {
                                return Result<Guid>.Failure($"Multi-level Take Profit price (${tpPrice.ToString("#,##0.00######", System.Globalization.CultureInfo.InvariantCulture)}) must be below current price (${currentPrice.ToString("#,##0.00######", System.Globalization.CultureInfo.InvariantCulture)}) for Sell position.");
                            }
                        }

                        if (el.TryGetProperty("percentage", out var pctProp) && pctProp.TryGetDecimal(out var tpPct))
                        {
                            if (tpPct <= 0 || tpPct > 100)
                            {
                                return Result<Guid>.Failure("Each Take Profit level percentage must be between 1% and 100%.");
                            }
                            totalPct += tpPct;
                        }
                    }

                    if (Math.Round(totalPct, 1) != 100m)
                    {
                        return Result<Guid>.Failure($"Total allocation across Take Profit levels must equal 100% (currently {totalPct}%).");
                    }

                    if (!effectiveTp.HasValue && firstLevelPrice.HasValue)
                    {
                        effectiveTp = firstLevelPrice.Value;
                    }
                }
            }
            catch (System.Text.Json.JsonException ex)
            {
                return Result<Guid>.Failure($"Invalid Take Profit levels JSON format: {ex.Message}");
            }
        }
        else
        {
            if (position.Side == TradingOrderSide.Buy && effectiveTp.HasValue && effectiveTp.Value <= currentPrice)
            {
                return Result<Guid>.Failure($"For Buy (Long) position, Take Profit (${effectiveTp.Value.ToString("#,##0.00000", System.Globalization.CultureInfo.InvariantCulture)}) must be strictly above current market price (${currentPrice.ToString("#,##0.00000", System.Globalization.CultureInfo.InvariantCulture)}).");
            }
            if (position.Side == TradingOrderSide.Sell && effectiveTp.HasValue && effectiveTp.Value >= currentPrice)
            {
                return Result<Guid>.Failure($"For Sell (Short) position, Take Profit (${effectiveTp.Value.ToString("#,##0.00000", System.Globalization.CultureInfo.InvariantCulture)}) must be strictly below current market price (${currentPrice.ToString("#,##0.00000", System.Globalization.CultureInfo.InvariantCulture)}).");
            }
        }

        if (position.Side == TradingOrderSide.Buy)
        {
            if (effectiveSl.HasValue && effectiveSl.Value >= currentPrice)
            {
                return Result<Guid>.Failure($"For Buy (Long) position, Stop Loss (${effectiveSl.Value.ToString("#,##0.00000", System.Globalization.CultureInfo.InvariantCulture)}) must be strictly below current market price (${currentPrice.ToString("#,##0.00000", System.Globalization.CultureInfo.InvariantCulture)}).");
            }
        }
        else // Sell (Short)
        {
            if (effectiveSl.HasValue && effectiveSl.Value <= currentPrice)
            {
                return Result<Guid>.Failure($"For Sell (Short) position, Stop Loss (${effectiveSl.Value.ToString("#,##0.00000", System.Globalization.CultureInfo.InvariantCulture)}) must be strictly above current market price (${currentPrice.ToString("#,##0.00000", System.Globalization.CultureInfo.InvariantCulture)}).");
            }
        }

        position.UpdateStops(effectiveSl, effectiveTp, cmd.TrailingStopDistance, cleanMultiTp);

        await _uow.SaveChangesAsync(ct);
        return Result<Guid>.Success(position.Id);
    }
}
