using System;
using AzureTest.Domain.Entities.Crypto.Enums;

namespace AzureTest.Contracts.DTOs.Crypto;

public sealed class CryptoChartRequestDto
{
    public string Symbol { get; set; } = "BTCUSDT";
    public CryptoTimeframe Timeframe { get; set; } = CryptoTimeframe.H1;

    public CryptoChartRequestDto() { }
    public CryptoChartRequestDto(string symbol, CryptoTimeframe timeframe)
    {
        Symbol = symbol;
        Timeframe = timeframe;
    }
}

public sealed class CryptoSymbolRequestDto
{
    public string Symbol { get; set; } = "BTCUSDT";

    public CryptoSymbolRequestDto() { }
    public CryptoSymbolRequestDto(string symbol)
    {
        Symbol = symbol;
    }
}

public sealed class CreateCryptoAlertRequestDto
{
    public string Symbol { get; set; } = string.Empty;
    public CryptoAlertType AlertType { get; set; }
    public decimal TargetValue { get; set; }
    public string? Message { get; set; }
    public string? Channel { get; set; } = "InApp";
}

public sealed class UpdateCryptoSettingsRequestDto
{
    public CryptoExchange Exchange { get; set; }
    public string ApiKey { get; set; } = string.Empty;
    public string ApiSecret { get; set; } = string.Empty;
    public string? Passphrase { get; set; }
    public bool IsTestnet { get; set; } = true;
}

public sealed class TestExchangeConnectionRequestDto
{
    public CryptoExchange Exchange { get; set; }
}

public sealed class PlaceCryptoOrderRequestDto
{
    public Guid AccountId { get; set; }
    public string Symbol { get; set; } = string.Empty;
    public CryptoOrderSide Side { get; set; }
    public CryptoOrderType OrderType { get; set; }
    public decimal Quantity { get; set; }
    public decimal RequestedPrice { get; set; }
    public decimal? StopPrice { get; set; }
    public int Leverage { get; set; } = 1;
    public decimal? StopLoss { get; set; }
    public decimal? TakeProfit { get; set; }
    public decimal? TrailingStopDistance { get; set; }
    public CryptoTradingMode TradingMode { get; set; } = CryptoTradingMode.PaperTrading;
    public string? IdempotencyKey { get; set; }
}

public sealed class CloseCryptoPositionRequestDto
{
    public Guid PositionId { get; set; }
    public decimal? ClosePrice { get; set; }
    public string Symbol { get; set; } = "BTCUSDT";
}

public sealed class CancelCryptoOrderRequestDto
{
    public Guid OrderId { get; set; }
    public string Symbol { get; set; } = "BTCUSDT";
}

public sealed class UpdateCryptoStopsRequestDto
{
    public Guid PositionId { get; set; }
    public decimal? StopLoss { get; set; }
    public decimal? TakeProfit { get; set; }
    public decimal? TrailingStopDistance { get; set; }
    public string Symbol { get; set; } = "BTCUSDT";
}

public sealed class PlaceCryptoSmartOrderRequestDto
{
    public Guid AccountId { get; set; }
    public string Symbol { get; set; } = string.Empty;
    public CryptoOrderSide Side { get; set; }
    public CryptoOrderType SmartOrderType { get; set; }
    public decimal TotalQuantity { get; set; }
    public decimal RequestedPrice { get; set; }
    public int DurationMinutes { get; set; } = 30;
    public int IntervalMinutes { get; set; } = 5;
    public decimal VisibleQuantity { get; set; } = 0m;
    public decimal VariancePercent { get; set; } = 10m;
    public decimal? TrailingDistanceUsd { get; set; }
    public Guid? PositionId { get; set; }
}

public sealed class ExecuteArbitrageRequestDto
{
    public string Type { get; set; } = string.Empty;
    public string PairOrRoute { get; set; } = string.Empty;
    public decimal ExpectedProfit { get; set; }
}

public sealed class SaveCryptoJournalRequestDto
{
    public Guid? Id { get; set; }
    public Guid? PositionId { get; set; }
    public string Symbol { get; set; } = string.Empty;
    public string StrategyTag { get; set; } = string.Empty;
    public string EmotionTag { get; set; } = string.Empty;
    public int Rating { get; set; }
    public string Notes { get; set; } = string.Empty;
    public string LessonsLearned { get; set; } = string.Empty;
}

public sealed class ResetCryptoAccountRequestDto
{
    public Guid AccountId { get; set; }
    public decimal InitialBalance { get; set; } = 10000m;
}

public sealed class CryptoKillSwitchRequestDto
{
    public Guid AccountId { get; set; }
    public string Reason { get; set; } = "User initiated emergency halt";
}

public sealed class UpdateCryptoRiskLimitsRequestDto
{
    public Guid AccountId { get; set; }
    public decimal RiskPercentagePerTrade { get; set; }
    public decimal DailyLossLimitPercent { get; set; }
    public int MaxOpenPositions { get; set; }
}

public sealed class TopUpCryptoMarginRequestDto
{
    public Guid PositionId { get; set; }
    public decimal AmountUsd { get; set; }
}

public sealed class TrimCryptoPositionRequestDto
{
    public Guid PositionId { get; set; }
    public decimal TrimQuantity { get; set; }
}

public sealed class CalculateCryptoAtrRequestDto
{
    public decimal AccountEquity { get; set; }
    public decimal RiskPercent { get; set; }
    public string Symbol { get; set; } = string.Empty;
    public decimal EntryPrice { get; set; }
    public decimal AtrMultiplier { get; set; } = 1.5m;
    public decimal? CustomAtr { get; set; }
}

public sealed class CryptoScreenerFilterDto
{
    public decimal? MinVolume { get; set; }
    public decimal? MinRsi { get; set; }
    public decimal? MaxRsi { get; set; }
    public decimal? MinChange { get; set; }
    public decimal? MaxChange { get; set; }
    public string? Trend { get; set; }
}
