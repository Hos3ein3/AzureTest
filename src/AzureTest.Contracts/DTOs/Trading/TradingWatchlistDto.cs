using System;
using AzureTest.Domain.Entities.Trading.Enums;

namespace AzureTest.Contracts.DTOs.Trading;

public sealed record TradingWatchlistDto(
    Guid Id,
    string Symbol,
    string AssetName,
    AssetClass AssetClass,
    DateTime AddedAtUtc
);
