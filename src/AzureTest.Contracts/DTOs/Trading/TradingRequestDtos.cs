using System;
using AzureTest.Domain.Entities.Trading.Enums;

namespace AzureTest.Contracts.DTOs.Trading;

public sealed class PlaceTradingOrderRequestDto
{
    public string Symbol { get; set; } = string.Empty;
    public string AssetName { get; set; } = string.Empty;
    public AssetClass AssetClass { get; set; }
    public TradingOrderSide Side { get; set; }
    public TradingOrderType OrderType { get; set; }
    public decimal Quantity { get; set; }
    public decimal? Price { get; set; }
    public decimal? TriggerPrice { get; set; }
    public int Leverage { get; set; } = 1;
    public decimal? StopLoss { get; set; }
    public decimal? TakeProfit { get; set; }
    public string? MultipleTakeProfitsJson { get; set; }
    public decimal? TrailingStopDistance { get; set; }
    public TimeInForce TimeInForce { get; set; }
    public DateTime? ExpiryDateUtc { get; set; }
}

public sealed class CloseTradingPositionRequestDto
{
    public Guid PositionId { get; set; }
    public decimal? ClosePrice { get; set; }
    public string? Symbol { get; set; }
    public string? SourcePage { get; set; } = "index";
}

public sealed class ModifyTradingPositionRequestDto
{
    public Guid PositionId { get; set; }
    public decimal? StopLoss { get; set; }
    public decimal? TakeProfit { get; set; }
    public decimal? TrailingStopDistance { get; set; }
    public string? MultipleTakeProfitsJson { get; set; }
    public string? Symbol { get; set; }
    public string? SourcePage { get; set; } = "trade";
}

public sealed class CancelTradingOrderRequestDto
{
    public Guid OrderId { get; set; }
    public string? Symbol { get; set; }
    public string? SourcePage { get; set; } = "index";
}

public sealed class ResetTradingBalanceRequestDto
{
    public decimal NewBalance { get; set; } = 50000m;
    public string? Symbol { get; set; }
}

public sealed class ToggleTradingWatchlistRequestDto
{
    public string Symbol { get; set; } = string.Empty;
    public string AssetName { get; set; } = string.Empty;
    public AssetClass AssetClass { get; set; }
    public string? ReturnUrl { get; set; }
}

public sealed class TradingHistoryFilterDto
{
    public string? Search { get; set; }
    public TradingOrderSide? Side { get; set; }
    public AssetClass? AssetClass { get; set; }
    public string? Outcome { get; set; }
}

public sealed class TradingJsonActionRequestDto
{
    public string Json { get; set; } = string.Empty;
}
