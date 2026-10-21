using System;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Domain.Entities.Crypto;
using AzureTest.Domain.Entities.Crypto.Enums;

namespace AzureTest.Application.Crypto.Services;

public interface ICryptoExecutionService
{
    Task<Result<CryptoOrder>> ExecutePaperOrderAsync(CryptoOrder order, CryptoAccount account, CancellationToken ct = default);
    Task<Result<CryptoPosition>> OpenPositionAsync(CryptoOrder filledOrder, CryptoAccount account, int leverage, decimal? stopLoss, decimal? takeProfit, decimal? trailingStopDistance, CancellationToken ct = default);
    Task<Result<decimal>> ClosePositionAsync(Guid positionId, decimal? marketPriceOverride, CancellationToken ct = default);
    Task<Result> UpdateStopsAsync(Guid positionId, decimal? stopLoss, decimal? takeProfit, decimal? trailingStopDistance, CancellationToken ct = default);
    Task<Result<int>> TriggerKillSwitchAsync(Guid accountId, string reason, CancellationToken ct = default);
}
