using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Contracts.DTOs.Crypto;

namespace AzureTest.Application.Crypto.Services;

public interface ICryptoStrategyService
{
    Task<BacktestResultDto> RunBacktestAsync(BacktestRequestDto request, CancellationToken ct = default);
    WalkForwardResultDto PerformWalkForwardValidation(IReadOnlyList<BacktestTradeDto> trades, decimal initialCapital);
    IReadOnlyList<CryptoWalkForwardWindowDto> RunMultiWindowWalkForwardAnalysis(IReadOnlyList<CryptoCandleDto> candles, BacktestRequestDto request);
    CryptoOverfittingShieldDto CalculateOverfittingShield(IReadOnlyList<BacktestTradeDto> trades, decimal sharpeRatio, int parameterCombinationsTested);
    CryptoMonteCarloSimulationDto RunMonteCarloSimulation(IReadOnlyList<BacktestTradeDto> trades, decimal initialCapital, int iterations = 10000);
}
