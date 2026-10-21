using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Interfaces.Data;
using AzureTest.Application.Messaging;
using AzureTest.Contracts.DTOs.Crypto;
using FluentValidation;

namespace AzureTest.Application.Crypto.Queries.GetCryptoStrategies;

public sealed record GetCryptoStrategiesQuery(Guid UserId) : IQuery<Result<(IReadOnlyList<CryptoStrategyDto> Strategies, IReadOnlyList<BacktestResultDto> RecentBacktests)>>;

public sealed class GetCryptoStrategiesQueryValidator : AbstractValidator<GetCryptoStrategiesQuery>
{
    public GetCryptoStrategiesQueryValidator()
    {
    }
}

public sealed class GetCryptoStrategiesQueryHandler : IQueryHandler<GetCryptoStrategiesQuery, Result<(IReadOnlyList<CryptoStrategyDto> Strategies, IReadOnlyList<BacktestResultDto> RecentBacktests)>>
{
    private readonly IUnitOfWork _uow;

    public GetCryptoStrategiesQueryHandler(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<Result<(IReadOnlyList<CryptoStrategyDto> Strategies, IReadOnlyList<BacktestResultDto> RecentBacktests)>> HandleAsync(GetCryptoStrategiesQuery query, CancellationToken ct)
    {
        var strategies = (await _uow.CryptoStrategies.FindAsync(s => s.UserId == query.UserId))
            .Select(s => new CryptoStrategyDto(
                s.Id,
                s.Name,
                s.Description,
                s.StrategyType,
                s.Symbol,
                s.Timeframe,
                s.ParametersJson,
                s.IsActive,
                s.CreatedOn))
            .ToList();

        var backtestEntities = (await _uow.CryptoBacktests.FindAsync(b => b.UserId == query.UserId))
            .OrderByDescending(b => b.CreatedOn)
            .Take(10)
            .ToList();

        var backtestDtos = backtestEntities.Select(b =>
        {
            var equityCurve = new List<EquityCurvePointDto>();
            try
            {
                if (!string.IsNullOrWhiteSpace(b.EquityCurveJson))
                    equityCurve = JsonSerializer.Deserialize<List<EquityCurvePointDto>>(b.EquityCurveJson) ?? new();
            }
            catch { }

            var walkForward = new WalkForwardResultDto(0m, 0m, 0m, "Normal", "Pass", "");
            try
            {
                if (!string.IsNullOrWhiteSpace(b.WalkForwardSummaryJson))
                    walkForward = JsonSerializer.Deserialize<WalkForwardResultDto>(b.WalkForwardSummaryJson) ?? walkForward;
            }
            catch { }

            return new BacktestResultDto(
                b.Id,
                b.StrategyName,
                b.StrategyType,
                b.Symbol,
                b.Timeframe,
                b.InitialCapital,
                b.FinalCapital,
                b.TotalNetProfit,
                b.ReturnPercentage,
                b.TotalTrades,
                b.WinningTrades,
                b.LosingTrades,
                b.WinRate,
                b.ProfitFactor,
                b.MaxDrawdownPercent,
                b.SharpeRatio,
                b.TotalFeesPaid,
                Array.Empty<BacktestTradeDto>(),
                equityCurve,
                walkForward);
        }).ToList();

        return Result<(IReadOnlyList<CryptoStrategyDto>, IReadOnlyList<BacktestResultDto>)>.Success((strategies, backtestDtos));
    }
}
