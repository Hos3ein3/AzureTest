using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Forex;
using AzureTest.Application.Forex.Queries.GetForexCandles;
using AzureTest.Application.Forex.Queries.GetForexMarketOverview;
using AzureTest.Application.Interfaces.Services;
using AzureTest.Contracts.DTOs.Forex;
using AzureTest.Domain.Entities.Forex.Enums;
using AzureTest.MVC.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AzureTest.MVC.Areas.Forex.Controllers;

[Area("Forex")]
[Authorize]
[DisplayName("Forex Market")]
[Description("Explore live and historical forex rates, spreads, candlestick charts, and global market sessions.")]
public class ForexMarketController : Controller
{
    private readonly IForexQueryUnit _queries;
    private readonly IForexCommandUnit _commands;
    private readonly IValidationService _validator;

    public ForexMarketController(
        IForexQueryUnit queries,
        IForexCommandUnit commands,
        IValidationService validator)
    {
        _queries = queries;
        _commands = commands;
        _validator = validator;
    }

    [HttpGet]
    [DisplayName("Market Overview")]
    [Description("View live quotes, bid/ask spreads, daily percent changes, and metals/indices.")]
    public async Task<IActionResult> Index([FromQuery] ForexMarketFilter? filter, CancellationToken cancellationToken)
    {
        filter ??= new ForexMarketFilter();
        var result = await _queries.GetMarketOverview.HandleAsync(new GetForexMarketOverviewQuery(filter), cancellationToken);
        ViewBag.Filter = filter;
        return result.ToView(this);
    }

    [HttpGet]
    [DisplayName("Interactive Chart")]
    [Description("View candlestick price action across multiple timeframes with volume.")]
    public async Task<IActionResult> Chart(string symbol = "EUR/USD", ForexTimeframe timeframe = ForexTimeframe.H1, CancellationToken cancellationToken = default)
    {
        var result = await _queries.GetCandles.HandleAsync(new GetForexCandlesQuery(symbol, timeframe, 120), cancellationToken);
        ViewBag.Symbol = symbol;
        ViewBag.Timeframe = timeframe;
        return result.ToView(this);
    }

    [HttpGet]
    [DisplayName("Market Sessions")]
    [Description("Monitor Sydney, Tokyo, London, and New York market session hours and overlaps.")]
    public async Task<IActionResult> Sessions(CancellationToken cancellationToken)
    {
        var result = await _queries.GetMarketOverview.HandleAsync(new GetForexMarketOverviewQuery(new ForexMarketFilter()), cancellationToken);
        return result.ToView(this, viewName: "Sessions");
    }
}
