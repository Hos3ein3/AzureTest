using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Forex.Services;
using AzureTest.Application.Interfaces.Data;
using AzureTest.Application.Interfaces.Services;
using AzureTest.Application.Messaging;
using AzureTest.Contracts.DTOs.Forex;
using FluentValidation;

namespace AzureTest.Application.Forex.Queries.GetForexStrategies;

public sealed record GetForexStrategiesQuery(ForexStrategyFilter Filter)
    : IQuery<Result<(IReadOnlyList<ForexStrategyDto> Strategies, IReadOnlyList<BacktestResultDto> RecentBacktests)>>;

public sealed class GetForexStrategiesQueryValidator : AbstractValidator<GetForexStrategiesQuery>
{
    public GetForexStrategiesQueryValidator()
    {
        RuleFor(x => x.Filter.Page).GreaterThan(0);
        RuleFor(x => x.Filter.PageSize).InclusiveBetween(1, 100);
    }
}

public sealed class GetForexStrategiesQueryHandler
    : IQueryHandler<GetForexStrategiesQuery, Result<(IReadOnlyList<ForexStrategyDto> Strategies, IReadOnlyList<BacktestResultDto> RecentBacktests)>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IForexStrategyService _strategyService;
    private readonly ICurrentUser _currentUser;

    public GetForexStrategiesQueryHandler(
        IUnitOfWork unitOfWork,
        IForexStrategyService strategyService,
        ICurrentUser currentUser)
    {
        _unitOfWork = unitOfWork;
        _strategyService = strategyService;
        _currentUser = currentUser;
    }

    public async Task<Result<(IReadOnlyList<ForexStrategyDto> Strategies, IReadOnlyList<BacktestResultDto> RecentBacktests)>> HandleAsync(
        GetForexStrategiesQuery query,
        CancellationToken cancellationToken)
    {
        var prebuilt = _strategyService.GetPrebuiltStrategies();

        var backtestEntities = await _unitOfWork.ForexBacktests.FindAsync(b => b.UserId == _currentUser.Id);
        var recentBacktests = backtestEntities.OrderByDescending(b => b.CreatedOn).Take(10).Select(b => new BacktestResultDto(
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
            new List<BacktestTradeLogDto>(),
            new List<decimal> { b.InitialBalance, b.FinalBalance }
        )).ToList();

        return Result<(IReadOnlyList<ForexStrategyDto> Strategies, IReadOnlyList<BacktestResultDto> RecentBacktests)>.Success((prebuilt, recentBacktests));
    }
}
