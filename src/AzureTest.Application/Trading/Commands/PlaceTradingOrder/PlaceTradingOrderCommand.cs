using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Interfaces.Data;
using AzureTest.Application.Messaging;
using AzureTest.Application.Trading.Services;
using AzureTest.Domain.Entities.Trading;
using AzureTest.Domain.Entities.Trading.Enums;
using FluentValidation;

namespace AzureTest.Application.Trading.Commands.PlaceTradingOrder;

public sealed record PlaceTradingOrderCommand(
    Guid UserId,
    Guid TenantId,
    string Symbol,
    string AssetName,
    AssetClass AssetClass,
    TradingOrderSide Side,
    TradingOrderType OrderType,
    decimal Quantity,
    decimal? Price = null,
    decimal? TriggerPrice = null,
    int Leverage = TradingPosition.Rules.DefaultLeverage,
    decimal? StopLoss = null,
    decimal? TakeProfit = null,
    string? MultipleTakeProfitsJson = null,
    decimal? TrailingStopDistance = null,
    TimeInForce TimeInForce = TimeInForce.GTC,
    DateTime? ExpiryDateUtc = null) : ICommand<Result<Guid>>
{
    public static class Mapping
    {
        public static TradingOrder ToOrder(PlaceTradingOrderCommand cmd, Guid accountId) =>
            TradingOrder.Create(
                accountId,
                cmd.UserId,
                cmd.TenantId,
                cmd.Symbol,
                cmd.AssetName,
                cmd.AssetClass,
                cmd.Side,
                cmd.OrderType,
                cmd.Quantity,
                cmd.Price,
                cmd.TriggerPrice,
                cmd.Leverage,
                cmd.StopLoss,
                cmd.TakeProfit,
                cmd.MultipleTakeProfitsJson,
                cmd.TrailingStopDistance,
                cmd.TimeInForce,
                cmd.ExpiryDateUtc);

        public static TradingPosition ToPosition(PlaceTradingOrderCommand cmd, Guid accountId, decimal executionPrice) =>
            TradingPosition.Open(
                accountId,
                cmd.UserId,
                cmd.TenantId,
                cmd.Symbol,
                cmd.AssetName,
                cmd.AssetClass,
                cmd.Side,
                cmd.Quantity,
                executionPrice,
                cmd.Leverage,
                cmd.StopLoss,
                cmd.TakeProfit,
                cmd.MultipleTakeProfitsJson,
                cmd.TrailingStopDistance);
    }
}

public sealed class PlaceTradingOrderCommandValidator : AbstractValidator<PlaceTradingOrderCommand>
{
    public PlaceTradingOrderCommandValidator()
    {
        RuleFor(x => x.Symbol)
            .NotEmpty()
            .WithMessage("Asset symbol is required.")
            .MinimumLength(TradingOrder.Rules.SymbolMinLength)
            .MaximumLength(TradingOrder.Rules.SymbolMaxLength);

        RuleFor(x => x.Quantity)
            .GreaterThan(0m)
            .WithMessage("Quantity must be greater than 0.");

        RuleFor(x => x.Leverage)
            .InclusiveBetween(TradingOrder.Rules.LeverageMin, TradingOrder.Rules.LeverageMax)
            .WithMessage($"Leverage must be between {TradingOrder.Rules.LeverageMin}x and {TradingOrder.Rules.LeverageMax}x.");

        When(x => x.OrderType == TradingOrderType.Limit, () =>
        {
            RuleFor(x => x.Price)
                .NotNull().WithMessage("Limit order requires a limit price.")
                .GreaterThan(0m).WithMessage("Limit price must be greater than 0.");
        });

        When(x => x.OrderType == TradingOrderType.Stop, () =>
        {
            RuleFor(x => x.TriggerPrice)
                .NotNull().WithMessage("Stop order requires a trigger price.")
                .GreaterThan(0m).WithMessage("Trigger price must be greater than 0.");
        });

        When(x => x.TrailingStopDistance.HasValue, () =>
        {
            RuleFor(x => x.TrailingStopDistance!.Value)
                .GreaterThan(0m).WithMessage("Trailing stop distance must be greater than 0.");
        });
    }
}

public sealed class PlaceTradingOrderCommandHandler : ICommandHandler<PlaceTradingOrderCommand, Result<Guid>>
{
    private readonly IUnitOfWork _uow;
    private readonly ITradingMarketDataService _marketData;
    private readonly AzureTest.Application.Interfaces.Services.IAppNotificationService? _notificationService;

    public PlaceTradingOrderCommandHandler(
        IUnitOfWork uow,
        ITradingMarketDataService marketData,
        AzureTest.Application.Interfaces.Services.IAppNotificationService? notificationService = null)
    {
        _uow = uow;
        _marketData = marketData;
        _notificationService = notificationService;
    }

    public async Task<Result<Guid>> HandleAsync(PlaceTradingOrderCommand cmd, CancellationToken ct)
    {
        // Load or create the user's trading account scoped by TenantId
        var accounts = await _uow.TradingAccounts.GetAllAsync();
        var account = accounts.FirstOrDefault(a => a.UserId == cmd.UserId && (cmd.TenantId == Guid.Empty || a.TenantId == cmd.TenantId));
        if (account == null)
        {
            account = TradingAccount.Create(cmd.UserId, cmd.TenantId, "Primary Paper Trading Account");
            _uow.TradingAccounts.Add(account);
            await _uow.SaveChangesAsync(ct);
        }

        // Always resolve the fresh real-time market price at execution time to protect against stale cached client prices
        var marketPrice = await _marketData.GetCurrentPriceAsync(cmd.Symbol, cmd.Price ?? 100m, ct);
        var referencePrice = cmd.OrderType switch
        {
            TradingOrderType.Limit => cmd.Price ?? marketPrice,
            TradingOrderType.Stop => cmd.TriggerPrice ?? marketPrice,
            _ => marketPrice
        };

        // Directional Stop Loss & Take Profit Validation
        decimal? effectiveStopLoss = cmd.StopLoss;
        decimal? effectiveTakeProfit = cmd.TakeProfit;

        var precision = referencePrice < 0.0001m ? 8 : (referencePrice < 0.01m ? 6 : (referencePrice < 1.0m ? 5 : (referencePrice < 10m ? 4 : 2)));

        if (cmd.Side == TradingOrderSide.Buy)
        {
            if (effectiveStopLoss.HasValue && effectiveStopLoss.Value > 0)
            {
                if (cmd.OrderType == TradingOrderType.Market && effectiveStopLoss.Value >= referencePrice)
                {
                    // If market order price ticked slightly during network latency, clamp SL slightly below entry
                    effectiveStopLoss = Math.Round(referencePrice * 0.995m, precision);
                }
                else if (effectiveStopLoss.Value >= referencePrice)
                {
                    return Result<Guid>.Failure($"For Buy (Long) orders, Stop Loss (${effectiveStopLoss.Value.ToString("#,##0.00######", System.Globalization.CultureInfo.InvariantCulture)}) must be set strictly BELOW the entry price (${referencePrice.ToString("#,##0.00######", System.Globalization.CultureInfo.InvariantCulture)}).");
                }
            }

            if (effectiveTakeProfit.HasValue && effectiveTakeProfit.Value > 0)
            {
                if (cmd.OrderType == TradingOrderType.Market && effectiveTakeProfit.Value <= referencePrice)
                {
                    effectiveTakeProfit = Math.Round(referencePrice * 1.01m, precision);
                }
                else if (effectiveTakeProfit.Value <= referencePrice)
                {
                    return Result<Guid>.Failure($"For Buy (Long) orders, Take Profit (${effectiveTakeProfit.Value.ToString("#,##0.00######", System.Globalization.CultureInfo.InvariantCulture)}) must be set strictly ABOVE the entry price (${referencePrice.ToString("#,##0.00######", System.Globalization.CultureInfo.InvariantCulture)}).");
                }
            }
        }
        else // Sell (Short)
        {
            if (effectiveStopLoss.HasValue && effectiveStopLoss.Value > 0)
            {
                if (cmd.OrderType == TradingOrderType.Market && effectiveStopLoss.Value <= referencePrice)
                {
                    effectiveStopLoss = Math.Round(referencePrice * 1.005m, precision);
                }
                else if (effectiveStopLoss.Value <= referencePrice)
                {
                    return Result<Guid>.Failure($"For Sell (Short) orders, Stop Loss (${effectiveStopLoss.Value.ToString("#,##0.00######", System.Globalization.CultureInfo.InvariantCulture)}) must be set strictly ABOVE the entry price (${referencePrice.ToString("#,##0.00######", System.Globalization.CultureInfo.InvariantCulture)}).");
                }
            }

            if (effectiveTakeProfit.HasValue && effectiveTakeProfit.Value > 0)
            {
                if (cmd.OrderType == TradingOrderType.Market && effectiveTakeProfit.Value >= referencePrice)
                {
                    effectiveTakeProfit = Math.Round(referencePrice * 0.99m, precision);
                }
                else if (effectiveTakeProfit.Value >= referencePrice)
                {
                    return Result<Guid>.Failure($"For Sell (Short) orders, Take Profit (${effectiveTakeProfit.Value.ToString("#,##0.00######", System.Globalization.CultureInfo.InvariantCulture)}) must be set strictly BELOW the entry price (${referencePrice.ToString("#,##0.00######", System.Globalization.CultureInfo.InvariantCulture)}).");
                }
            }
        }

        cmd = cmd with { StopLoss = effectiveStopLoss, TakeProfit = effectiveTakeProfit };

        // Multi-level Take Profit JSON Validation
        if (!string.IsNullOrWhiteSpace(cmd.MultipleTakeProfitsJson))
        {
            try
            {
                using var doc = System.Text.Json.JsonDocument.Parse(cmd.MultipleTakeProfitsJson);
                if (doc.RootElement.ValueKind == System.Text.Json.JsonValueKind.Array)
                {
                    decimal totalPct = 0;
                    foreach (var el in doc.RootElement.EnumerateArray())
                    {
                        if (el.TryGetProperty("price", out var pProp) && pProp.TryGetDecimal(out var tpPrice))
                        {
                            if (cmd.Side == TradingOrderSide.Buy && tpPrice <= referencePrice)
                            {
                                return Result<Guid>.Failure($"Multi-level Take Profit price (${tpPrice.ToString("#,##0.00######", System.Globalization.CultureInfo.InvariantCulture)}) must be above entry price (${referencePrice.ToString("#,##0.00######", System.Globalization.CultureInfo.InvariantCulture)}) for Buy orders.");
                            }
                            if (cmd.Side == TradingOrderSide.Sell && tpPrice >= referencePrice)
                            {
                                return Result<Guid>.Failure($"Multi-level Take Profit price (${tpPrice.ToString("#,##0.00######", System.Globalization.CultureInfo.InvariantCulture)}) must be below entry price (${referencePrice.ToString("#,##0.00######", System.Globalization.CultureInfo.InvariantCulture)}) for Sell orders.");
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

                    if (totalPct > 100)
                    {
                        return Result<Guid>.Failure($"The sum of Take Profit level percentages ({totalPct}%) cannot exceed 100%.");
                    }
                }
            }
            catch (System.Text.Json.JsonException)
            {
                return Result<Guid>.Failure("Invalid format for multi-level Take Profit configuration.");
            }
        }

        if (cmd.OrderType == TradingOrderType.Market)
        {
            var position = PlaceTradingOrderCommand.Mapping.ToPosition(cmd, account.Id, marketPrice);

            // Check if account has enough free margin
            if (account.FreeMargin < position.InitialMargin)
            {
                return Result<Guid>.Failure($"Insufficient free paper margin. Required margin is ${position.InitialMargin:N2}, but your free margin is ${account.FreeMargin:N2}. Try increasing leverage or decreasing order quantity.");
            }

            account.DeductMargin(position.InitialMargin);

            var order = PlaceTradingOrderCommand.Mapping.ToOrder(cmd, account.Id);
            order.Fill();

            _uow.TradingPositions.Add(position);
            _uow.TradingOrders.Add(order);

            await _uow.SaveChangesAsync(ct);

            if (_notificationService != null)
            {
                var sideText = cmd.Side == TradingOrderSide.Buy ? "Buy (Long)" : "Sell (Short)";
                var title = $"Paper Trade Opened: {cmd.Symbol}";
                var message = $"Opened {sideText} position for {cmd.Quantity:G29} {cmd.Symbol} @ ${marketPrice:N2} ({cmd.Leverage}x leverage).";
                await _notificationService.SendNotificationAsync(
                    cmd.UserId,
                    cmd.TenantId,
                    title,
                    message,
                    type: "Trade",
                    actionUrl: "/Trading",
                    ct: ct);
            }

            return Result<Guid>.Success(position.Id);
        }
        else
        {
            // Limit or Stop Order
            var notional = cmd.Quantity * referencePrice;
            var estimatedMargin = notional / cmd.Leverage;

            if (account.FreeMargin < estimatedMargin)
            {
                return Result<Guid>.Failure($"Insufficient free paper margin to place pending order. Required margin: ${estimatedMargin:N2}, Free margin: ${account.FreeMargin:N2}.");
            }

            var order = PlaceTradingOrderCommand.Mapping.ToOrder(cmd, account.Id);
            _uow.TradingOrders.Add(order);

            await _uow.SaveChangesAsync(ct);
            return Result<Guid>.Success(order.Id);
        }
    }
}
