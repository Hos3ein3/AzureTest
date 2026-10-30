using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Interfaces.Data;
using AzureTest.Application.Messaging;
using AzureTest.Application.Trading.Services;
using AzureTest.Contracts.DTOs.Trading;
using AzureTest.Domain.Entities.Trading;
using AzureTest.Domain.Entities.Trading.Enums;
using FluentValidation;

namespace AzureTest.Application.Trading.Commands.ExecuteTradingJsonBatch;

public sealed record ExecuteTradingJsonBatchCommand(
    string JsonPayload,
    Guid UserId,
    Guid TenantId) : ICommand<Result<IReadOnlyList<TradingActionResultDto>>>;

public sealed class ExecuteTradingJsonBatchCommandValidator : AbstractValidator<ExecuteTradingJsonBatchCommand>
{
    public ExecuteTradingJsonBatchCommandValidator()
    {
        RuleFor(x => x.JsonPayload).NotEmpty().WithMessage("JSON payload cannot be empty.");
    }
}

public sealed class ExecuteTradingJsonBatchCommandHandler : ICommandHandler<ExecuteTradingJsonBatchCommand, Result<IReadOnlyList<TradingActionResultDto>>>
{
    private readonly IUnitOfWork _uow;
    private readonly ITradingMarketDataService _marketData;

    public ExecuteTradingJsonBatchCommandHandler(IUnitOfWork uow, ITradingMarketDataService marketData)
    {
        _uow = uow;
        _marketData = marketData;
    }

    public async Task<Result<IReadOnlyList<TradingActionResultDto>>> HandleAsync(ExecuteTradingJsonBatchCommand cmd, CancellationToken ct)
    {
        var results = new List<TradingActionResultDto>();

        List<TradingJsonActionDto> actions;
        var jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

        try
        {
            var trimmed = cmd.JsonPayload.Trim();
            if (trimmed.StartsWith("["))
            {
                actions = JsonSerializer.Deserialize<List<TradingJsonActionDto>>(trimmed, jsonOptions) ?? new();
            }
            else
            {
                var batchReq = JsonSerializer.Deserialize<TradingJsonBatchRequestDto>(trimmed, jsonOptions);
                if (batchReq?.Actions != null && batchReq.Actions.Count > 0)
                {
                    actions = batchReq.Actions;
                }
                else
                {
                    var single = JsonSerializer.Deserialize<TradingJsonActionDto>(trimmed, jsonOptions);
                    actions = single != null ? new List<TradingJsonActionDto> { single } : new();
                }
            }
        }
        catch (Exception ex)
        {
            return Result<IReadOnlyList<TradingActionResultDto>>.Failure($"Failed to parse JSON payload: {ex.Message}");
        }

        // Ensure user account exists
        var accounts = await _uow.TradingAccounts.GetAllAsync();
        var account = accounts.FirstOrDefault(a => a.UserId == cmd.UserId && (cmd.TenantId == Guid.Empty || a.TenantId == cmd.TenantId));
        if (account == null)
        {
            account = TradingAccount.Create(cmd.UserId, cmd.TenantId, "Primary Paper Trading Account");
            _uow.TradingAccounts.Add(account);
            await _uow.SaveChangesAsync(ct);
        }

        foreach (var action in actions)
        {
            var actionType = (action.Action ?? string.Empty).Trim().ToLowerInvariant();

            switch (actionType)
            {
                case "place_order":
                case "buy":
                case "sell":
                case "order":
                    {
                        var symbol = action.Symbol ?? "BTCUSDT";
                        var assetClass = action.AssetClass ?? AssetClass.Crypto;
                        var side = action.Side ?? (actionType == "sell" ? TradingOrderSide.Sell : TradingOrderSide.Buy);
                        var orderType = action.OrderType ?? TradingOrderType.Market;
                        var qty = action.Quantity ?? 1m;
                        var lev = action.Leverage ?? 1;
                        var maxLev = await _marketData.GetMaxLeverageAsync(symbol, ct);
                        if (lev > maxLev) lev = maxLev;

                        if (orderType == TradingOrderType.Market)
                        {
                            var marketPrice = await _marketData.GetCurrentPriceAsync(symbol, action.Price ?? 100m, ct);
                            var position = TradingPosition.Open(
                                account.Id,
                                cmd.UserId,
                                cmd.TenantId,
                                symbol,
                                action.AssetName ?? symbol,
                                assetClass,
                                side,
                                qty,
                                marketPrice,
                                lev,
                                action.StopLoss,
                                action.TakeProfit,
                                action.MultipleTakeProfitsJson,
                                action.TrailingStopDistance);

                            if (account.FreeMargin < position.InitialMargin)
                            {
                                results.Add(new TradingActionResultDto(false, $"Order failed for {symbol}: Insufficient free margin."));
                                break;
                            }

                            account.DeductMargin(position.InitialMargin);

                            var filledOrder = TradingOrder.Create(
                                account.Id,
                                cmd.UserId,
                                cmd.TenantId,
                                symbol,
                                action.AssetName ?? symbol,
                                assetClass,
                                side,
                                orderType,
                                qty,
                                marketPrice,
                                action.TriggerPrice,
                                lev,
                                action.StopLoss,
                                action.TakeProfit,
                                action.MultipleTakeProfitsJson,
                                action.TrailingStopDistance,
                                action.TimeInForce ?? TimeInForce.GTC);

                            filledOrder.Fill();

                            _uow.TradingPositions.Add(position);
                            _uow.TradingOrders.Add(filledOrder);

                            results.Add(new TradingActionResultDto(true, $"Market {side} order executed for {qty} {symbol} at ${marketPrice:N2}.", position.Id));
                        }
                        else
                        {
                            var pendingOrder = TradingOrder.Create(
                                account.Id,
                                cmd.UserId,
                                cmd.TenantId,
                                symbol,
                                action.AssetName ?? symbol,
                                assetClass,
                                side,
                                orderType,
                                qty,
                                action.Price,
                                action.TriggerPrice,
                                lev,
                                action.StopLoss,
                                action.TakeProfit,
                                action.MultipleTakeProfitsJson,
                                action.TrailingStopDistance,
                                action.TimeInForce ?? TimeInForce.GTC);

                            _uow.TradingOrders.Add(pendingOrder);
                            results.Add(new TradingActionResultDto(true, $"{orderType} {side} order placed for {qty} {symbol}.", pendingOrder.Id));
                        }
                        break;
                    }

                case "close_position":
                case "close":
                    {
                        if (!action.PositionId.HasValue)
                        {
                            // Close first matching open position by symbol if specified
                            var openPos = (await _uow.TradingPositions.GetAllAsync())
                                .FirstOrDefault(p => p.UserId == cmd.UserId && (cmd.TenantId == Guid.Empty || p.TenantId == cmd.TenantId) && p.Status == TradingPositionStatus.Open &&
                                    (string.IsNullOrEmpty(action.Symbol) || p.Symbol.Equals(action.Symbol, StringComparison.OrdinalIgnoreCase)));

                            if (openPos == null)
                            {
                                results.Add(new TradingActionResultDto(false, "No matching open position found to close."));
                                break;
                            }

                            var exitPrice = action.Price ?? await _marketData.GetCurrentPriceAsync(openPos.Symbol, openPos.CurrentPrice, ct);
                            openPos.Close(exitPrice);
                            account.ApplyRealizedPnl(openPos.RealizedPnl, openPos.InitialMargin);
                            results.Add(new TradingActionResultDto(true, $"Position {openPos.Symbol} closed at ${exitPrice:N2} with PnL ${openPos.RealizedPnl:N2}.", openPos.Id));
                        }
                        else
                        {
                            var pos = await _uow.TradingPositions.GetByIdAsync(action.PositionId.Value);
                            if (pos == null || pos.Status != TradingPositionStatus.Open || pos.UserId != cmd.UserId || (cmd.TenantId != Guid.Empty && pos.TenantId != cmd.TenantId))
                            {
                                results.Add(new TradingActionResultDto(false, $"Position {action.PositionId} not found or already closed."));
                                break;
                            }

                            var exitPrice = action.Price ?? await _marketData.GetCurrentPriceAsync(pos.Symbol, pos.CurrentPrice, ct);
                            pos.Close(exitPrice);
                            account.ApplyRealizedPnl(pos.RealizedPnl, pos.InitialMargin);
                            results.Add(new TradingActionResultDto(true, $"Position {pos.Symbol} closed at ${exitPrice:N2} with PnL ${pos.RealizedPnl:N2}.", pos.Id));
                        }
                        break;
                    }

                case "cancel_order":
                case "cancel":
                    {
                        if (action.OrderId.HasValue)
                        {
                            var order = await _uow.TradingOrders.GetByIdAsync(action.OrderId.Value);
                            if (order != null && order.Status == TradingOrderStatus.Pending && order.UserId == cmd.UserId && (cmd.TenantId == Guid.Empty || order.TenantId == cmd.TenantId))
                            {
                                order.Cancel();
                                results.Add(new TradingActionResultDto(true, $"Order {order.Symbol} cancelled.", order.Id));
                            }
                            else
                            {
                                results.Add(new TradingActionResultDto(false, $"Order {action.OrderId} not found or not pending."));
                            }
                        }
                        break;
                    }

                case "reset_balance":
                case "set_balance":
                    {
                        var newBal = action.ResetBalance ?? 50000m;
                        account.ResetBalance(newBal);

                        // Permanently delete all position records and order records scoped to this user and tenant
                        var userPositions = (await _uow.TradingPositions.GetAllAsync())
                            .Where(p => p.UserId == cmd.UserId && (cmd.TenantId == Guid.Empty || p.TenantId == cmd.TenantId))
                            .ToList();
                        foreach (var p in userPositions)
                        {
                            _uow.TradingPositions.Remove(p);
                        }

                        var userOrders = (await _uow.TradingOrders.GetAllAsync())
                            .Where(o => o.UserId == cmd.UserId && (cmd.TenantId == Guid.Empty || o.TenantId == cmd.TenantId))
                            .ToList();
                        foreach (var o in userOrders)
                        {
                            _uow.TradingOrders.Remove(o);
                        }

                        results.Add(new TradingActionResultDto(true, $"Paper trading balance successfully reset to ${newBal:N2} and all past records cleared.", account.Id));
                        break;
                    }

                case "toggle_watchlist":
                case "watchlist":
                    {
                        if (!string.IsNullOrEmpty(action.Symbol))
                        {
                            var sym = action.Symbol.Trim().ToUpperInvariant();
                            var watchlists = await _uow.TradingWatchlists.GetAllAsync();
                            var exist = watchlists.FirstOrDefault(w => w.UserId == cmd.UserId && (cmd.TenantId == Guid.Empty || w.TenantId == cmd.TenantId) && w.Symbol.Equals(sym, StringComparison.OrdinalIgnoreCase));
                            if (exist != null)
                            {
                                _uow.TradingWatchlists.Remove(exist);
                                results.Add(new TradingActionResultDto(true, $"Removed {sym} from watchlist."));
                            }
                            else
                            {
                                var w = TradingWatchlist.Create(cmd.UserId, cmd.TenantId, sym, action.AssetName ?? sym, action.AssetClass ?? AssetClass.Crypto);
                                _uow.TradingWatchlists.Add(w);
                                results.Add(new TradingActionResultDto(true, $"Added {sym} to watchlist.", w.Id));
                            }
                        }
                        break;
                    }

                default:
                    results.Add(new TradingActionResultDto(false, $"Unknown action '{actionType}'"));
                    break;
            }
        }

        await _uow.SaveChangesAsync(ct);
        return Result<IReadOnlyList<TradingActionResultDto>>.Success(results);
    }
}
