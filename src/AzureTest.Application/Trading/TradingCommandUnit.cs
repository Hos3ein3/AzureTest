using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Messaging;
using AzureTest.Application.Trading.Commands.CancelTradingOrder;
using AzureTest.Application.Trading.Commands.CloseTradingPosition;
using AzureTest.Application.Trading.Commands.ExecuteTradingJsonBatch;
using AzureTest.Application.Trading.Commands.ModifyTradingPosition;
using AzureTest.Application.Trading.Commands.PlaceTradingOrder;
using AzureTest.Application.Trading.Commands.ResetTradingAccountBalance;
using AzureTest.Application.Trading.Commands.SyncTradingPrices;
using AzureTest.Application.Trading.Commands.ToggleTradingWatchlist;
using AzureTest.Contracts.DTOs.Trading;

namespace AzureTest.Application.Trading;

public interface ITradingCommandUnit
{
    Task<Result<Guid>> PlaceOrderAsync(PlaceTradingOrderCommand cmd, CancellationToken ct = default);
    Task<Result<Guid>> ClosePositionAsync(CloseTradingPositionCommand cmd, CancellationToken ct = default);
    Task<Result<Guid>> ModifyPositionAsync(ModifyTradingPositionCommand cmd, CancellationToken ct = default);
    Task<Result<Guid>> CancelOrderAsync(CancelTradingOrderCommand cmd, CancellationToken ct = default);
    Task<Result<Guid>> ResetAccountBalanceAsync(ResetTradingAccountBalanceCommand cmd, CancellationToken ct = default);
    Task<Result<bool>> ToggleWatchlistAsync(ToggleTradingWatchlistCommand cmd, CancellationToken ct = default);
    Task<Result<IReadOnlyList<TradingActionResultDto>>> ExecuteJsonBatchAsync(ExecuteTradingJsonBatchCommand cmd, CancellationToken ct = default);
    Task<Result<int>> SyncPricesAsync(SyncTradingPricesCommand cmd, CancellationToken ct = default);
}

public sealed class TradingCommandUnit : ITradingCommandUnit
{
    private readonly ICommandHandler<PlaceTradingOrderCommand, Result<Guid>> _placeOrder;
    private readonly ICommandHandler<CloseTradingPositionCommand, Result<Guid>> _closePosition;
    private readonly ICommandHandler<ModifyTradingPositionCommand, Result<Guid>> _modifyPosition;
    private readonly ICommandHandler<CancelTradingOrderCommand, Result<Guid>> _cancelOrder;
    private readonly ICommandHandler<ResetTradingAccountBalanceCommand, Result<Guid>> _resetBalance;
    private readonly ICommandHandler<ToggleTradingWatchlistCommand, Result<bool>> _toggleWatchlist;
    private readonly ICommandHandler<ExecuteTradingJsonBatchCommand, Result<IReadOnlyList<TradingActionResultDto>>> _executeJsonBatch;
    private readonly ICommandHandler<SyncTradingPricesCommand, Result<int>> _syncPrices;

    public TradingCommandUnit(
        ICommandHandler<PlaceTradingOrderCommand, Result<Guid>> placeOrder,
        ICommandHandler<CloseTradingPositionCommand, Result<Guid>> closePosition,
        ICommandHandler<ModifyTradingPositionCommand, Result<Guid>> modifyPosition,
        ICommandHandler<CancelTradingOrderCommand, Result<Guid>> cancelOrder,
        ICommandHandler<ResetTradingAccountBalanceCommand, Result<Guid>> resetBalance,
        ICommandHandler<ToggleTradingWatchlistCommand, Result<bool>> toggleWatchlist,
        ICommandHandler<ExecuteTradingJsonBatchCommand, Result<IReadOnlyList<TradingActionResultDto>>> executeJsonBatch,
        ICommandHandler<SyncTradingPricesCommand, Result<int>> syncPrices)
    {
        _placeOrder = placeOrder;
        _closePosition = closePosition;
        _modifyPosition = modifyPosition;
        _cancelOrder = cancelOrder;
        _resetBalance = resetBalance;
        _toggleWatchlist = toggleWatchlist;
        _executeJsonBatch = executeJsonBatch;
        _syncPrices = syncPrices;
    }

    public Task<Result<Guid>> PlaceOrderAsync(PlaceTradingOrderCommand cmd, CancellationToken ct = default) =>
        _placeOrder.HandleAsync(cmd, ct);

    public Task<Result<Guid>> ClosePositionAsync(CloseTradingPositionCommand cmd, CancellationToken ct = default) =>
        _closePosition.HandleAsync(cmd, ct);

    public Task<Result<Guid>> ModifyPositionAsync(ModifyTradingPositionCommand cmd, CancellationToken ct = default) =>
        _modifyPosition.HandleAsync(cmd, ct);

    public Task<Result<Guid>> CancelOrderAsync(CancelTradingOrderCommand cmd, CancellationToken ct = default) =>
        _cancelOrder.HandleAsync(cmd, ct);

    public Task<Result<Guid>> ResetAccountBalanceAsync(ResetTradingAccountBalanceCommand cmd, CancellationToken ct = default) =>
        _resetBalance.HandleAsync(cmd, ct);

    public Task<Result<bool>> ToggleWatchlistAsync(ToggleTradingWatchlistCommand cmd, CancellationToken ct = default) =>
        _toggleWatchlist.HandleAsync(cmd, ct);

    public Task<Result<IReadOnlyList<TradingActionResultDto>>> ExecuteJsonBatchAsync(ExecuteTradingJsonBatchCommand cmd, CancellationToken ct = default) =>
        _executeJsonBatch.HandleAsync(cmd, ct);

    public Task<Result<int>> SyncPricesAsync(SyncTradingPricesCommand cmd, CancellationToken ct = default) =>
        _syncPrices.HandleAsync(cmd, ct);
}
