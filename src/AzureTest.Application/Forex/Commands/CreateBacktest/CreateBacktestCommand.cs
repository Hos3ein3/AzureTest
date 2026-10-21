using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Forex.Services;
using AzureTest.Application.Interfaces.Data;
using AzureTest.Application.Interfaces.Services;
using AzureTest.Application.Messaging;
using AzureTest.Contracts.DTOs.Forex;
using AzureTest.Domain.Entities.Forex;
using AzureTest.Domain.Entities.Forex.Enums;
using FluentValidation;

namespace AzureTest.Application.Forex.Commands.CreateBacktest;

public sealed record CreateBacktestCommand(BacktestRequestDto Request) : ICommand<Result<BacktestResultDto>>;

public sealed class CreateBacktestCommandValidator : AbstractValidator<CreateBacktestCommand>
{
    public CreateBacktestCommandValidator()
    {
        RuleFor(x => x.Request.Symbol)
            .NotEmpty()
            .MaximumLength(ForexBacktest.Rules.SymbolMaxLength);

        RuleFor(x => x.Request.InitialBalance)
            .GreaterThan(0m);

        RuleFor(x => x.Request.SpreadPips)
            .GreaterThanOrEqualTo(0m);

        RuleFor(x => x.Request.SlippagePips)
            .GreaterThanOrEqualTo(0m);
    }
}

public sealed class CreateBacktestCommandHandler : ICommandHandler<CreateBacktestCommand, Result<BacktestResultDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IForexStrategyService _strategyService;
    private readonly ICurrentUser _currentUser;

    public CreateBacktestCommandHandler(
        IUnitOfWork unitOfWork,
        IForexStrategyService strategyService,
        ICurrentUser currentUser)
    {
        _unitOfWork = unitOfWork;
        _strategyService = strategyService;
        _currentUser = currentUser;
    }

    public async Task<Result<BacktestResultDto>> HandleAsync(CreateBacktestCommand command, CancellationToken cancellationToken)
    {
        var resultDto = await _strategyService.RunBacktestAsync(command.Request, cancellationToken);

        var logsJson = JsonSerializer.Serialize(resultDto.TradeLogs);
        if (logsJson.Length > ForexBacktest.Rules.JsonReportMaxLength)
        {
            logsJson = logsJson[..ForexBacktest.Rules.JsonReportMaxLength];
        }

        var backtest = ForexBacktest.Create(
            command.Request.StrategyId,
            _currentUser.Id,
            command.Request.StrategyName,
            command.Request.Symbol,
            command.Request.Timeframe,
            command.Request.StartDate,
            command.Request.EndDate,
            command.Request.InitialBalance,
            resultDto.FinalBalance,
            resultDto.MaxDrawdownPercent,
            resultDto.WinRatePercent,
            resultDto.TotalTrades,
            resultDto.WinningTrades,
            resultDto.LosingTrades,
            resultDto.ProfitFactor,
            resultDto.ExpectancyPips,
            resultDto.SharpeRatio,
            command.Request.SpreadPips,
            command.Request.SlippagePips,
            resultDto.TotalSwapCost,
            command.Request.IsWalkForward,
            resultDto.OutOfSampleEfficiency,
            logsJson,
            $"Executed for user {_currentUser.Id}"
        );

        _unitOfWork.ForexBacktests.Add(backtest);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<BacktestResultDto>.Success(resultDto with { Id = backtest.Id });
    }
}
