using System;
using System.Collections.Generic;
using AzureTest.Domain.Entities.Trading.Enums;

namespace AzureTest.Contracts.DTOs.Trading;

public sealed class TradingJsonActionDto
{
    public string Action { get; set; } = string.Empty; // "place_order", "close_position", "cancel_order", "reset_balance", "toggle_watchlist"
    public string? Symbol { get; set; }
    public string? AssetName { get; set; }
    public AssetClass? AssetClass { get; set; }
    public TradingOrderSide? Side { get; set; }
    public TradingOrderType? OrderType { get; set; }
    public decimal? Quantity { get; set; }
    public decimal? Price { get; set; }
    public decimal? TriggerPrice { get; set; }
    public int? Leverage { get; set; }
    public decimal? StopLoss { get; set; }
    public decimal? TakeProfit { get; set; }
    public string? MultipleTakeProfitsJson { get; set; }
    public decimal? TrailingStopDistance { get; set; }
    public TimeInForce? TimeInForce { get; set; }
    public decimal? ResetBalance { get; set; }
    public Guid? PositionId { get; set; }
    public Guid? OrderId { get; set; }
}

public sealed class TradingJsonBatchRequestDto
{
    public List<TradingJsonActionDto> Actions { get; set; } = new();
}

public sealed record TradingActionResultDto(
    bool Success,
    string Message,
    Guid? EntityId = null
);
