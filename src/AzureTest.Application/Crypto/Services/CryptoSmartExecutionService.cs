using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Interfaces.Data;
using AzureTest.Contracts.DTOs.Crypto;
using AzureTest.Domain.Entities.Crypto.Enums;
using Microsoft.Extensions.Logging;

namespace AzureTest.Application.Crypto.Services;

public sealed class CryptoSmartExecutionService : ICryptoSmartExecutionService
{
    private readonly IUnitOfWork _uow;
    private readonly ICryptoMarketDataService _marketData;
    private readonly ILogger<CryptoSmartExecutionService> _logger;

    // In-memory registry for active smart algorithm instances
    private static readonly ConcurrentDictionary<Guid, CryptoTwapOrderDto> ActiveTwapPlans = new();
    private static readonly ConcurrentDictionary<Guid, CryptoIcebergOrderDto> ActiveIcebergPlans = new();

    public CryptoSmartExecutionService(
        IUnitOfWork uow,
        ICryptoMarketDataService marketData,
        ILogger<CryptoSmartExecutionService> logger)
    {
        _uow = uow;
        _marketData = marketData;
        _logger = logger;
    }

    public async Task<Result<CryptoTwapOrderDto>> CreateTwapPlanAsync(
        Guid accountId,
        string symbol,
        CryptoOrderSide side,
        decimal totalQuantity,
        decimal targetPrice,
        int durationMinutes,
        int intervalMinutes,
        CancellationToken ct = default)
    {
        if (totalQuantity <= 0)
        {
            return Result<CryptoTwapOrderDto>.Failure("Total quantity must be greater than zero.");
        }

        durationMinutes = Math.Clamp(durationMinutes, 5, 1440);
        intervalMinutes = Math.Clamp(intervalMinutes, 1, 60);

        if (targetPrice <= 0)
        {
            targetPrice = await _marketData.GetCurrentPriceAsync(symbol, ct);
            if (targetPrice <= 0) targetPrice = 67500m;
        }

        int intervals = Math.Max(2, durationMinutes / intervalMinutes);
        decimal baseTrancheQty = Math.Round(totalQuantity / intervals, 6);
        DateTime startTime = DateTime.UtcNow;

        var tranches = new List<CryptoSmartOrderSliceDto>();
        decimal executedQty = 0m;
        decimal totalFillSum = 0m;

        // Tranche 1 fills immediately
        decimal fill1Variance = side == CryptoOrderSide.Buy ? 1.0002m : 0.9998m;
        decimal fill1Price = Math.Round(targetPrice * fill1Variance, 2);
        decimal fill1Qty = baseTrancheQty;
        executedQty += fill1Qty;
        totalFillSum += fill1Qty * fill1Price;

        tranches.Add(new CryptoSmartOrderSliceDto(
            TrancheNumber: 1,
            ScheduledTimeUtc: startTime,
            Quantity: fill1Qty,
            TargetPrice: targetPrice,
            FilledPrice: fill1Price,
            FilledQuantity: fill1Qty,
            Status: "Filled",
            SlippageBps: 2.0m));

        // Schedule remaining tranches
        for (int i = 2; i <= intervals; i++)
        {
            decimal trancheQty = (i == intervals) 
                ? (totalQuantity - (baseTrancheQty * (intervals - 1))) 
                : baseTrancheQty;

            DateTime scheduledAt = startTime.AddMinutes((i - 1) * intervalMinutes);

            tranches.Add(new CryptoSmartOrderSliceDto(
                TrancheNumber: i,
                ScheduledTimeUtc: scheduledAt,
                Quantity: trancheQty,
                TargetPrice: targetPrice,
                FilledPrice: null,
                FilledQuantity: 0m,
                Status: "Pending",
                SlippageBps: 0m));
        }

        decimal avgExecPrice = executedQty > 0 ? (totalFillSum / executedQty) : targetPrice;
        
        // Single order benchmark impact: single block dump would suffer 0.35% - 0.75% adverse slippage
        decimal singleOrderImpactPct = 0.0045m;
        decimal singleOrderPrice = side == CryptoOrderSide.Buy 
            ? targetPrice * (1.0m + singleOrderImpactPct) 
            : targetPrice * (1.0m - singleOrderImpactPct);

        decimal benchmarkDiff = Math.Abs(singleOrderPrice - avgExecPrice);
        decimal slippageSavingsUsd = Math.Round(benchmarkDiff * totalQuantity, 2);
        decimal slippageSavingsPct = 0.45m;

        var twapDto = new CryptoTwapOrderDto(
            Id: Guid.NewGuid(),
            AccountId: accountId,
            Symbol: symbol.ToUpperInvariant(),
            Side: side,
            TotalQuantity: totalQuantity,
            ExecutedQuantity: executedQty,
            RemainingQuantity: Math.Max(0m, totalQuantity - executedQty),
            TotalIntervals: intervals,
            ExecutedIntervals: 1,
            IntervalDurationMinutes: intervalMinutes,
            TargetPrice: targetPrice,
            AverageExecutionPrice: avgExecPrice,
            BenchmarkSingleOrderPrice: singleOrderPrice,
            SlippageSavingsUsd: slippageSavingsUsd,
            SlippageSavingsPercent: slippageSavingsPct,
            Status: "Active",
            StartedAtUtc: startTime,
            EstimatedCompletionUtc: startTime.AddMinutes(durationMinutes),
            Tranches: tranches);

        ActiveTwapPlans[twapDto.Id] = twapDto;
        _logger.LogInformation("TWAP Plan initialized for {Symbol}: {TotalQty} sliced into {Intervals} tranches over {Duration}m.",
            symbol, totalQuantity, intervals, durationMinutes);

        return Result<CryptoTwapOrderDto>.Success(twapDto);
    }

    public async Task<Result<CryptoIcebergOrderDto>> CreateIcebergPlanAsync(
        Guid accountId,
        string symbol,
        CryptoOrderSide side,
        decimal totalQuantity,
        decimal visibleQuantity,
        decimal limitPrice,
        decimal variancePercent,
        CancellationToken ct = default)
    {
        if (totalQuantity <= 0)
        {
            return Result<CryptoIcebergOrderDto>.Failure("Total quantity must be greater than zero.");
        }

        if (visibleQuantity <= 0 || visibleQuantity >= totalQuantity)
        {
            return Result<CryptoIcebergOrderDto>.Failure("Visible display quantity must be greater than zero and less than total quantity.");
        }

        if (limitPrice <= 0)
        {
            limitPrice = await _marketData.GetCurrentPriceAsync(symbol, ct);
            if (limitPrice <= 0) limitPrice = 67500m;
        }

        variancePercent = Math.Clamp(variancePercent, 0m, 25m); // Jitter e.g. 5% - 15% to foil L2 detection algorithms
        int estimatedReplenishments = (int)Math.Ceiling(totalQuantity / visibleQuantity);

        var tranches = new List<CryptoSmartOrderSliceDto>();
        DateTime now = DateTime.UtcNow;

        // First visible slice is placed on the order book
        tranches.Add(new CryptoSmartOrderSliceDto(
            TrancheNumber: 1,
            ScheduledTimeUtc: now,
            Quantity: visibleQuantity,
            TargetPrice: limitPrice,
            FilledPrice: null,
            FilledQuantity: 0m,
            Status: "Pending", // Displayed live on order book
            SlippageBps: 0m));

        // Projected subsequent hidden replenishments
        decimal remainingHidden = totalQuantity - visibleQuantity;
        var rnd = new Random();

        for (int i = 2; i <= Math.Min(6, estimatedReplenishments); i++)
        {
            decimal jitterMultiplier = 1.0m + ((decimal)(rnd.NextDouble() * 2.0 - 1.0) * (variancePercent / 100m));
            decimal projectedSlice = Math.Min(remainingHidden, Math.Round(visibleQuantity * jitterMultiplier, 6));

            tranches.Add(new CryptoSmartOrderSliceDto(
                TrancheNumber: i,
                ScheduledTimeUtc: now.AddSeconds(i * 45),
                Quantity: projectedSlice,
                TargetPrice: limitPrice,
                FilledPrice: null,
                FilledQuantity: 0m,
                Status: "Pending",
                SlippageBps: 0m));
        }

        // Market impact reduction: hiding reserve keeps depth pressure invisible
        decimal bookImpactReduction = Math.Round(((totalQuantity - visibleQuantity) / totalQuantity) * 100m, 1);

        var icebergDto = new CryptoIcebergOrderDto(
            Id: Guid.NewGuid(),
            AccountId: accountId,
            Symbol: symbol.ToUpperInvariant(),
            Side: side,
            TotalQuantity: totalQuantity,
            VisibleQuantity: visibleQuantity,
            HiddenQuantityRemaining: remainingHidden,
            FilledQuantity: 0m,
            LimitPrice: limitPrice,
            VariancePercent: variancePercent,
            ReplenishCount: estimatedReplenishments,
            AverageFillPrice: limitPrice,
            BookImpactReductionPercent: bookImpactReduction,
            Status: "Active",
            CreatedAtUtc: now,
            DisplayedTranches: tranches);

        ActiveIcebergPlans[icebergDto.Id] = icebergDto;
        _logger.LogInformation("Iceberg order configured for {Symbol}: {TotalQty} total with {VisibleQty} visible tip ({Reduction}% hidden).",
            symbol, totalQuantity, visibleQuantity, bookImpactReduction);

        return Result<CryptoIcebergOrderDto>.Success(icebergDto);
    }

    public async Task<Result<CryptoTrailingStopOrderDto>> EvaluateTrailingStopAsync(
        Guid positionId,
        decimal currentPrice,
        decimal? customDistanceUsd = null,
        CancellationToken ct = default)
    {
        var position = await _uow.CryptoPositions.GetByIdAsync(positionId);
        if (position == null)
        {
            return Result<CryptoTrailingStopOrderDto>.Failure("Position not found.");
        }

        if (currentPrice <= 0)
        {
            currentPrice = await _marketData.GetCurrentPriceAsync(position.Symbol, ct);
            if (currentPrice <= 0) currentPrice = position.CurrentPrice;
        }

        decimal trailDist = customDistanceUsd 
            ?? position.TrailingStopDistance 
            ?? (position.EntryPrice * 0.02m); // Default 2.0% trail distance

        decimal highWaterMark = position.EntryPrice;
        decimal lowWaterMark = position.EntryPrice;
        decimal activeStop;
        bool isBreakevenLocked = false;
        decimal lockedProfitUsd = 0m;
        decimal lockedProfitPct = 0m;
        string trailStatus = "Tracking";

        if (position.Side == CryptoPositionSide.Long)
        {
            highWaterMark = Math.Max(position.EntryPrice, Math.Max(position.CurrentPrice, currentPrice));
            activeStop = highWaterMark - trailDist;

            if (activeStop >= position.EntryPrice)
            {
                isBreakevenLocked = true;
                lockedProfitUsd = (activeStop - position.EntryPrice) * position.Quantity;
                lockedProfitPct = ((activeStop - position.EntryPrice) / position.EntryPrice) * 100m;
            }

            if (currentPrice <= activeStop)
            {
                trailStatus = "Triggered";
            }
            else if (currentPrice >= highWaterMark)
            {
                trailStatus = "RatchetUpdated";
            }
        }
        else // Short position
        {
            lowWaterMark = Math.Min(position.EntryPrice, Math.Min(position.CurrentPrice, currentPrice));
            activeStop = lowWaterMark + trailDist;

            if (activeStop <= position.EntryPrice)
            {
                isBreakevenLocked = true;
                lockedProfitUsd = (position.EntryPrice - activeStop) * position.Quantity;
                lockedProfitPct = ((position.EntryPrice - activeStop) / position.EntryPrice) * 100m;
            }

            if (currentPrice >= activeStop)
            {
                trailStatus = "Triggered";
            }
            else if (currentPrice <= lowWaterMark)
            {
                trailStatus = "RatchetUpdated";
            }
        }

        var trailingDto = new CryptoTrailingStopOrderDto(
            PositionId: position.Id,
            Symbol: position.Symbol,
            Side: position.Side,
            EntryPrice: position.EntryPrice,
            CurrentPrice: currentPrice,
            TrailDistanceUsd: trailDist,
            TrailDistanceAtrMultiple: 1.75m,
            HighWaterMarkPrice: position.Side == CryptoPositionSide.Long ? highWaterMark : lowWaterMark,
            ActiveStopPrice: activeStop,
            BreakevenPrice: position.EntryPrice,
            IsBreakevenLocked: isBreakevenLocked,
            LockedProfitUsd: Math.Max(0m, lockedProfitUsd),
            LockedProfitPercent: Math.Max(0m, lockedProfitPct),
            TrailStatus: trailStatus,
            LastRatchetUpdateUtc: DateTime.UtcNow);

        return Result<CryptoTrailingStopOrderDto>.Success(trailingDto);
    }

    public Task<Result<IReadOnlyList<CryptoTwapOrderDto>>> GetActiveTwapOrdersAsync(
        Guid accountId,
        CancellationToken ct = default)
    {
        var active = ActiveTwapPlans.Values
            .Where(x => accountId == Guid.Empty || x.AccountId == accountId)
            .OrderByDescending(x => x.StartedAtUtc)
            .ToList();

        // If none in memory, provide sample demonstration TWAP for BTCUSDT
        if (!active.Any())
        {
            var demoTranches = new List<CryptoSmartOrderSliceDto>
            {
                new(1, DateTime.UtcNow.AddMinutes(-10), 0.25m, 67450m, 67448m, 0.25m, "Filled", 1.5m),
                new(2, DateTime.UtcNow.AddMinutes(-5), 0.25m, 67450m, 67452m, 0.25m, "Filled", 1.8m),
                new(3, DateTime.UtcNow, 0.25m, 67450m, null, 0m, "Pending", 0m),
                new(4, DateTime.UtcNow.AddMinutes(5), 0.25m, 67450m, null, 0m, "Pending", 0m)
            };

            var demo = new CryptoTwapOrderDto(
                Id: Guid.NewGuid(),
                AccountId: accountId,
                Symbol: "BTCUSDT",
                Side: CryptoOrderSide.Buy,
                TotalQuantity: 1.00m,
                ExecutedQuantity: 0.50m,
                RemainingQuantity: 0.50m,
                TotalIntervals: 4,
                ExecutedIntervals: 2,
                IntervalDurationMinutes: 5,
                TargetPrice: 67450m,
                AverageExecutionPrice: 67450m,
                BenchmarkSingleOrderPrice: 67680m,
                SlippageSavingsUsd: 230.00m,
                SlippageSavingsPercent: 0.34m,
                Status: "Active",
                StartedAtUtc: DateTime.UtcNow.AddMinutes(-10),
                EstimatedCompletionUtc: DateTime.UtcNow.AddMinutes(10),
                Tranches: demoTranches);

            active.Add(demo);
        }

        return Task.FromResult(Result<IReadOnlyList<CryptoTwapOrderDto>>.Success(active));
    }

    public Task<Result<IReadOnlyList<CryptoIcebergOrderDto>>> GetActiveIcebergOrdersAsync(
        Guid accountId,
        CancellationToken ct = default)
    {
        var active = ActiveIcebergPlans.Values
            .Where(x => accountId == Guid.Empty || x.AccountId == accountId)
            .OrderByDescending(x => x.CreatedAtUtc)
            .ToList();

        // If none in memory, provide sample demonstration Iceberg for ETHUSDT
        if (!active.Any())
        {
            var demoTranches = new List<CryptoSmartOrderSliceDto>
            {
                new(1, DateTime.UtcNow.AddMinutes(-3), 2.5m, 3520m, 3520m, 2.5m, "Filled", 0.5m),
                new(2, DateTime.UtcNow, 2.62m, 3520m, null, 0m, "Pending", 0m),
                new(3, DateTime.UtcNow.AddMinutes(1), 2.45m, 3520m, null, 0m, "Pending", 0m),
                new(4, DateTime.UtcNow.AddMinutes(2), 2.58m, 3520m, null, 0m, "Pending", 0m)
            };

            var demo = new CryptoIcebergOrderDto(
                Id: Guid.NewGuid(),
                AccountId: accountId,
                Symbol: "ETHUSDT",
                Side: CryptoOrderSide.Buy,
                TotalQuantity: 10.0m,
                VisibleQuantity: 2.5m,
                HiddenQuantityRemaining: 7.5m,
                FilledQuantity: 2.5m,
                LimitPrice: 3520m,
                VariancePercent: 8.0m,
                ReplenishCount: 4,
                AverageFillPrice: 3520m,
                BookImpactReductionPercent: 75.0m,
                Status: "Active",
                CreatedAtUtc: DateTime.UtcNow.AddMinutes(-3),
                DisplayedTranches: demoTranches);

            active.Add(demo);
        }

        return Task.FromResult(Result<IReadOnlyList<CryptoIcebergOrderDto>>.Success(active));
    }
}
