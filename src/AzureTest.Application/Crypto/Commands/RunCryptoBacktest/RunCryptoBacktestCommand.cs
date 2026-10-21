using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Crypto.Services;
using AzureTest.Application.Interfaces.Data;
using AzureTest.Application.Messaging;
using AzureTest.Contracts.DTOs.Crypto;
using AzureTest.Domain.Entities.Crypto;
using FluentValidation;

namespace AzureTest.Application.Crypto.Commands.RunCryptoBacktest;

public sealed record RunCryptoBacktestCommand(
    Guid UserId,
    Guid TenantId,
    BacktestRequestDto Request) : ICommand<Result<BacktestResultDto>>
{
    public static class Mapping
    {
        public static CryptoBacktest ToDomain(
            RunCryptoBacktestCommand cmd,
            BacktestResultDto result)
        {
            return CryptoBacktest.Create(
                strategyId: null,
                userId: cmd.UserId,
                tenantId: cmd.TenantId,
                strategyName: result.StrategyName,
                strategyType: result.StrategyType,
                symbol: result.Symbol,
                timeframe: result.Timeframe,
                startDate: DateTime.UtcNow.AddDays(-cmd.Request.DaysBack),
                endDate: DateTime.UtcNow,
                initialCapital: result.InitialCapital,
                finalCapital: result.FinalCapital,
                totalTrades: result.TotalTrades,
                winningTrades: result.WinningTrades,
                losingTrades: result.LosingTrades,
                winRate: result.WinRate,
                profitFactor: result.ProfitFactor,
                maxDrawdownPercent: result.MaxDrawdownPercent,
                sharpeRatio: result.SharpeRatio,
                commissionFeePercent: cmd.Request.CommissionPercent,
                slippagePercent: cmd.Request.SlippagePercent,
                totalFeesPaid: result.TotalFeesPaid,
                equityCurveJson: JsonSerializer.Serialize(result.EquityCurve),
                walkForwardSummaryJson: JsonSerializer.Serialize(result.WalkForward));
        }
    }
}

public sealed class RunCryptoBacktestCommandValidator : AbstractValidator<RunCryptoBacktestCommand>
{
    public RunCryptoBacktestCommandValidator()
    {
        RuleFor(x => x.Request.Symbol)
            .NotEmpty()
            .MaximumLength(CryptoBacktest.Rules.SymbolMaxLength);

        RuleFor(x => x.Request.InitialCapital)
            .GreaterThan(0m);

        RuleFor(x => x.Request.DaysBack)
            .InclusiveBetween(1, 365);
    }
}

public sealed class RunCryptoBacktestCommandHandler : ICommandHandler<RunCryptoBacktestCommand, Result<BacktestResultDto>>
{
    private readonly ICryptoStrategyService _strategyService;
    private readonly IUnitOfWork _uow;

    public RunCryptoBacktestCommandHandler(ICryptoStrategyService strategyService, IUnitOfWork uow)
    {
        _strategyService = strategyService;
        _uow = uow;
    }

    public async Task<Result<BacktestResultDto>> HandleAsync(RunCryptoBacktestCommand cmd, CancellationToken ct)
    {
        var result = await _strategyService.RunBacktestAsync(cmd.Request, ct);

        // Persist backtest record
        var backtestEntity = RunCryptoBacktestCommand.Mapping.ToDomain(cmd, result);
        _uow.CryptoBacktests.Add(backtestEntity);
        await _uow.SaveChangesAsync(ct);

        return Result<BacktestResultDto>.Success(result);
    }
}
