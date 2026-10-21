using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Crypto;
using AzureTest.Application.Crypto.Commands.RunCryptoBacktest;
using AzureTest.Application.Crypto.Queries.GetCryptoStrategies;
using AzureTest.Application.Interfaces.Services;
using AzureTest.Contracts.DTOs.Crypto;
using AzureTest.Domain.Entities.Crypto.Enums;
using AzureTest.MVC.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AzureTest.MVC.Areas.Crypto.Controllers;

[Area("Crypto")]
[Authorize]
[DisplayName("Crypto Strategies & Backtesting")]
[Description("Algorithmic strategy catalog, historical backtester with fees and slippage, performance metrics, and walk-forward cross-validation.")]
public class CryptoStrategyController : Controller
{
    private readonly ICryptoQueryUnit _queries;
    private readonly ICryptoCommandUnit _commands;
    private readonly IValidationService _validator;
    private readonly ICurrentUser _currentUser;

    public CryptoStrategyController(
        ICryptoQueryUnit queries,
        ICryptoCommandUnit commands,
        IValidationService validator,
        ICurrentUser currentUser)
    {
        _queries = queries;
        _commands = commands;
        _validator = validator;
        _currentUser = currentUser;
    }

    [HttpGet]
    [DisplayName("Strategy Catalog")]
    [Description("Explore rule-based algorithmic strategies (Trend Following, Mean Reversion, Breakout) and past backtest evaluations.")]
    public async Task<IActionResult> Index(CancellationToken cancellationToken = default)
    {
        var result = await _queries.GetStrategies.HandleAsync(new GetCryptoStrategiesQuery(_currentUser.Id), cancellationToken);
        return result.ToView(this);
    }

    [HttpGet]
    [DisplayName("Backtest Simulation")]
    [Description("Configure backtest engine parameters including symbol, timeframe, initial capital, trading fees, and slippage.")]
    public IActionResult Backtest(string symbol = "BTCUSDT", CryptoStrategyType strategy = CryptoStrategyType.TrendFollowing, CryptoTimeframe timeframe = CryptoTimeframe.H1)
    {
        var model = new BacktestRequestDto
        {
            Symbol = symbol,
            StrategyType = strategy,
            Timeframe = timeframe,
            DaysBack = 90,
            InitialCapital = 10000m,
            CommissionPercent = 0.075m,
            SlippagePercent = 0.05m,
            FastPeriod = 9,
            SlowPeriod = 21,
            RsiPeriod = 14,
            BbPeriod = 20,
            BbStdDev = 2.0m
        };
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [DisplayName("Execute Backtest")]
    [Description("Simulate strategy performance over historical candles, calculate win rate, profit factor, max drawdown, Sharpe ratio, and walk-forward overfitting test.")]
    public async Task<IActionResult> Backtest(BacktestRequestDto request, CancellationToken cancellationToken)
    {
        var command = new RunCryptoBacktestCommand(_currentUser.Id, _currentUser.TenantId, request);
        var validation = await _validator.ValidateAsync(command, cancellationToken);
        if (validation.IsFailure)
        {
            return validation.ToFormResult(this, request, nameof(Backtest));
        }

        var result = await _commands.RunBacktest.HandleAsync(command, cancellationToken);
        if (result.IsFailure)
        {
            return result.ToFormResult(this, request, nameof(Backtest));
        }

        ViewBag.BacktestResult = result.Value;
        return View("Results", result.Value);
    }
}
