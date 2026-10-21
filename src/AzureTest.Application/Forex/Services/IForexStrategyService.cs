using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Contracts.DTOs.Forex;
using AzureTest.Domain.Entities.Forex;

namespace AzureTest.Application.Forex.Services;

public interface IForexStrategyService
{
    IReadOnlyList<ForexStrategyDto> GetPrebuiltStrategies();
    Task<BacktestResultDto> RunBacktestAsync(BacktestRequestDto request, CancellationToken cancellationToken = default);
    Task<WalkForwardResultDto> RunWalkForwardAsync(Guid? strategyId, string symbol, CancellationToken cancellationToken = default);
    MonteCarloSimulationDto RunMonteCarloSimulation(IReadOnlyList<BacktestTradeLogDto> trades, decimal initialBalance, int iterations = 500);
    OverfittingDiagnosticDto EvaluateOverfitting(decimal sharpeRatio, int totalTrades, int sampleDays);
    Task<PortfolioBacktestResultDto> RunPortfolioBacktestAsync(IReadOnlyList<string> symbols, Guid? strategyId, CancellationToken cancellationToken = default);
}
