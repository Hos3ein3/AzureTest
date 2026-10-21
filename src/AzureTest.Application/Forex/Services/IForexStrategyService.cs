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
}
