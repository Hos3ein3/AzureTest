using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Forex;
using AzureTest.Application.Forex.Commands.CreateBacktest;
using AzureTest.Application.Forex.Queries.GetBacktestDetails;
using AzureTest.Application.Forex.Queries.GetForexStrategies;
using AzureTest.Application.Interfaces.Services;
using AzureTest.Contracts.DTOs.Forex;
using AzureTest.Domain.Entities.Forex.Enums;
using AzureTest.MVC.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AzureTest.MVC.Areas.Forex.Controllers;

[Area("Forex")]
[Authorize]
[DisplayName("Forex Strategy & Backtesting")]
[Description("Explore rule-based strategies, execute quantitative backtests, and perform walk-forward validation.")]
public class ForexStrategyController : Controller
{
    private readonly IForexQueryUnit _queries;
    private readonly IForexCommandUnit _commands;
    private readonly IValidationService _validator;

    public ForexStrategyController(
        IForexQueryUnit queries,
        IForexCommandUnit commands,
        IValidationService validator)
    {
        _queries = queries;
        _commands = commands;
        _validator = validator;
    }

    [HttpGet]
    [DisplayName("Strategies Catalog")]
    [Description("Browse London Breakout, Trend Pullback, and Range strategies alongside backtest runs.")]
    public async Task<IActionResult> Index([FromQuery] ForexStrategyFilter? filter, CancellationToken cancellationToken)
    {
        filter ??= new ForexStrategyFilter();
        var result = await _queries.GetStrategies.HandleAsync(new GetForexStrategiesQuery(filter), cancellationToken);
        ViewBag.Filter = filter;
        return result.ToView(this);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [DisplayName("Execute Backtest")]
    [Description("Run simulation with custom spread, slippage, and swap modeling.")]
    public async Task<IActionResult> Execute(CreateBacktestCommand command, CancellationToken cancellationToken)
    {
        var validation = await _validator.ValidateAsync(command, cancellationToken);
        if (validation.IsFailure)
        {
            return validation.ToFormResult(this, command, nameof(Index));
        }

        var result = await _commands.CreateBacktest.HandleAsync(command, cancellationToken);
        if (result.IsFailure)
        {
            return result.ToFormResult(this, command, nameof(Index));
        }

        return RedirectToAction(nameof(Results), new { id = result.Value.Id });
    }

    [HttpGet]
    [DisplayName("Backtest Results")]
    [Description("Inspect equity curve, trade log distribution, win rate, expectancy, and profit factor.")]
    public async Task<IActionResult> Results(Guid id, CancellationToken cancellationToken)
    {
        var result = await _queries.GetBacktestDetails.HandleAsync(new GetBacktestDetailsQuery(id), cancellationToken);
        return result.ToView(this, viewName: "Results");
    }
}
