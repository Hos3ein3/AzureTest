using System;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Contracts.DTOs.Crypto;
using AzureTest.Domain.Entities.Crypto;

namespace AzureTest.Application.Crypto.Services;

public interface ICryptoRiskService
{
    PositionSizeCalculatorDto CalculatePositionSize(decimal accountEquity, decimal riskPercent, decimal entryPrice, decimal stopLossPrice, int requestedLeverage);
    Task<CryptoRiskOverviewDto> GetRiskOverviewAsync(Guid userId, CancellationToken ct = default);
    Task<bool> CanOpenNewPositionAsync(CryptoAccount account, decimal requiredMargin, CancellationToken ct = default);
}
