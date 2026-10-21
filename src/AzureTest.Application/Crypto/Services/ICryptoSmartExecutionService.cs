using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Contracts.DTOs.Crypto;
using AzureTest.Domain.Entities.Crypto.Enums;

namespace AzureTest.Application.Crypto.Services;

public interface ICryptoSmartExecutionService
{
    Task<Result<CryptoTwapOrderDto>> CreateTwapPlanAsync(
        Guid accountId,
        string symbol,
        CryptoOrderSide side,
        decimal totalQuantity,
        decimal targetPrice,
        int durationMinutes,
        int intervalMinutes,
        CancellationToken ct = default);

    Task<Result<CryptoIcebergOrderDto>> CreateIcebergPlanAsync(
        Guid accountId,
        string symbol,
        CryptoOrderSide side,
        decimal totalQuantity,
        decimal visibleQuantity,
        decimal limitPrice,
        decimal variancePercent,
        CancellationToken ct = default);

    Task<Result<CryptoTrailingStopOrderDto>> EvaluateTrailingStopAsync(
        Guid positionId,
        decimal currentPrice,
        decimal? customDistanceUsd = null,
        CancellationToken ct = default);

    Task<Result<IReadOnlyList<CryptoTwapOrderDto>>> GetActiveTwapOrdersAsync(
        Guid accountId,
        CancellationToken ct = default);

    Task<Result<IReadOnlyList<CryptoIcebergOrderDto>>> GetActiveIcebergOrdersAsync(
        Guid accountId,
        CancellationToken ct = default);
}
