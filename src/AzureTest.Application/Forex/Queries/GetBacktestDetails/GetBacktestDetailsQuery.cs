using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Interfaces.Data;
using AzureTest.Application.Interfaces.Services;
using AzureTest.Application.Messaging;
using AzureTest.Contracts.DTOs.Forex;
using FluentValidation;

namespace AzureTest.Application.Forex.Queries.GetBacktestDetails;

public sealed record GetBacktestDetailsQuery(Guid BacktestId) : IQuery<Result<BacktestResultDto>>;

public sealed class GetBacktestDetailsQueryValidator : AbstractValidator<GetBacktestDetailsQuery>
{
    public GetBacktestDetailsQueryValidator()
    {
        RuleFor(x => x.BacktestId).NotEmpty();
    }
}

public sealed class GetBacktestDetailsQueryHandler : IQueryHandler<GetBacktestDetailsQuery, Result<BacktestResultDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUser _currentUser;

    public GetBacktestDetailsQueryHandler(IUnitOfWork unitOfWork, ICurrentUser currentUser)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task<Result<BacktestResultDto>> HandleAsync(GetBacktestDetailsQuery query, CancellationToken cancellationToken)
    {
        var b = await _unitOfWork.ForexBacktests.GetByIdAsync(query.BacktestId);
        if (b == null) return Result<BacktestResultDto>.Failure("Backtest run not found.");
        if (b.UserId != _currentUser.Id) return Result<BacktestResultDto>.Failure("Unauthorized access.");

        var tradeLogs = new List<BacktestTradeLogDto>();
        if (!string.IsNullOrWhiteSpace(b.TradeLogsJson))
        {
            try
            {
                tradeLogs = JsonSerializer.Deserialize<List<BacktestTradeLogDto>>(b.TradeLogsJson) ?? new();
            }
            catch { }
        }

        var dto = new BacktestResultDto(
            b.Id,
            b.StrategyName,
            b.Symbol,
            b.Timeframe,
            b.StartDateUtc,
            b.EndDateUtc,
            b.InitialBalance,
            b.FinalBalance,
            b.NetProfit,
            b.ReturnPercent,
            b.MaxDrawdownPercent,
            b.WinRatePercent,
            b.TotalTrades,
            b.WinningTrades,
            b.LosingTrades,
            b.ProfitFactor,
            b.ExpectancyPips,
            b.SharpeRatio,
            b.SpreadPips,
            b.SlippagePips,
            b.TotalSwapCost,
            b.IsWalkForward,
            b.OutOfSampleEfficiency,
            tradeLogs,
            new List<decimal> { b.InitialBalance, b.FinalBalance }
        );

        return Result<BacktestResultDto>.Success(dto);
    }
}
