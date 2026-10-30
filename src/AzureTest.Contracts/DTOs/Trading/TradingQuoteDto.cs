using AzureTest.Domain.Entities.Trading.Enums;

namespace AzureTest.Contracts.DTOs.Trading;

public sealed record TradingQuoteDto(
    string Symbol,
    string AssetName,
    AssetClass AssetClass,
    decimal Price,
    decimal Change24h,
    decimal ChangePercent24h,
    decimal High24h,
    decimal Low24h,
    decimal Volume24h,
    bool IsWatchlisted,
    bool IsSimulated = false,
    string FeedSource = "binance",
    int MaxLeverage = 250
);
