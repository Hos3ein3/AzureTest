using System;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Contracts.DTOs.Forex;
using AzureTest.Domain.Entities.Forex;
using AzureTest.Domain.Entities.Forex.Enums;

namespace AzureTest.Application.Forex.Services;

public interface IForexExecutionService
{
    Task<Result<ForexTrade>> ExecuteOrderAsync(
        ForexAccount account,
        string symbol,
        ForexTradeDirection direction,
        ForexOrderType orderType,
        decimal lotSize,
        decimal requestedPrice,
        decimal? stopLoss,
        decimal? takeProfit,
        string idempotencyKey,
        CancellationToken cancellationToken = default);

    Task<Result<decimal>> ClosePositionAsync(
        ForexAccount account,
        ForexTrade trade,
        decimal currentMarketPrice,
        ForexExitReason reason,
        CancellationToken cancellationToken = default);
}
