using System;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Contracts.DTOs.Crypto;
using AzureTest.Domain.Entities.Crypto;

namespace AzureTest.Application.Crypto.Services;

public interface ICryptoRiskService
{
    PositionSizeCalculatorDto CalculatePositionSize(decimal accountEquity, decimal riskPercent, decimal entryPrice, decimal stopLossPrice, int requestedLeverage);
    CryptoAtrPositionSizingDto CalculateAtrPositionSize(decimal accountEquity, decimal riskPercent, string symbol, decimal entryPrice, decimal atrMultiplier = 1.5m, decimal? customAtr = null);
    Task<CryptoRiskOverviewDto> GetRiskOverviewAsync(Guid userId, CancellationToken ct = default);
    Task<bool> CanOpenNewPositionAsync(CryptoAccount account, decimal requiredMargin, CancellationToken ct = default);
    Task<Result> TopUpPositionMarginAsync(Guid positionId, decimal amountUsd, CancellationToken ct = default);
    Task<Result<decimal>> TrimPositionRiskAsync(Guid positionId, decimal trimQuantity, CancellationToken ct = default);
    Task<Result> UpdateRiskLimitsAsync(Guid accountId, decimal riskPercentagePerTrade, decimal dailyLossLimitPercent, int maxOpenPositions, CancellationToken ct = default);
}
