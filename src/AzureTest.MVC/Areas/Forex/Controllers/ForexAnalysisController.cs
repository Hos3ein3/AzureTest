using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Forex;
using AzureTest.Application.Forex.Queries.GetForexAnalysis;
using AzureTest.Application.Interfaces.Services;
using AzureTest.Contracts.DTOs.Forex;
using AzureTest.Domain.Entities.Forex.Enums;
using AzureTest.MVC.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AzureTest.MVC.Areas.Forex.Controllers;

[Area("Forex")]
[Authorize]
[DisplayName("Forex Analysis")]
[Description("Technical indicators, pivot points, currency strength, economic calendar, and COT positioning.")]
public class ForexAnalysisController : Controller
{
    private readonly IForexQueryUnit _queries;
    private readonly IForexCommandUnit _commands;
    private readonly IValidationService _validator;

    public ForexAnalysisController(
        IForexQueryUnit queries,
        IForexCommandUnit commands,
        IValidationService validator)
    {
        _queries = queries;
        _commands = commands;
        _validator = validator;
    }

    [HttpGet]
    [DisplayName("Analysis Hub")]
    [Description("Integrated view of technical indicators, multi-timeframe alignment, and fundamental drivers.")]
    public async Task<IActionResult> Index([FromQuery] ForexAnalysisRequestDto? request, CancellationToken cancellationToken = default)
    {
        request ??= new ForexAnalysisRequestDto();
        var result = await _queries.GetAnalysis.HandleAsync(new GetForexAnalysisQuery(request.Symbol, request.Timeframe), cancellationToken);
        ViewBag.Symbol = request.Symbol;
        ViewBag.Timeframe = request.Timeframe;
        return result.ToView(this);
    }

    [HttpGet]
    [DisplayName("Technical Indicators")]
    [Description("Inspect moving averages, RSI, MACD, ATR, Bollinger Bands, and Pivot Points.")]
    public async Task<IActionResult> Technical([FromQuery] ForexAnalysisRequestDto? request, CancellationToken cancellationToken = default)
    {
        request ??= new ForexAnalysisRequestDto();
        var result = await _queries.GetAnalysis.HandleAsync(new GetForexAnalysisQuery(request.Symbol, request.Timeframe), cancellationToken);
        ViewBag.Symbol = request.Symbol;
        ViewBag.Timeframe = request.Timeframe;
        return result.ToView(this, viewName: "Technical");
    }

    [HttpGet]
    [DisplayName("Currency Strength & Correlation")]
    [Description("Real-time relative strength index across 8 major currencies and currency pair correlation matrix.")]
    public async Task<IActionResult> Strength(CancellationToken cancellationToken)
    {
        var result = await _queries.GetAnalysis.HandleAsync(new GetForexAnalysisQuery("EUR/USD", ForexTimeframe.H1), cancellationToken);
        return result.ToView(this, viewName: "Strength");
    }

    [HttpGet]
    [DisplayName("Fundamental Analysis")]
    [Description("Economic calendar with impact levels, central bank rate differentials, and COT non-commercial positions.")]
    public async Task<IActionResult> Fundamental(CancellationToken cancellationToken)
    {
        var result = await _queries.GetAnalysis.HandleAsync(new GetForexAnalysisQuery("EUR/USD", ForexTimeframe.H1), cancellationToken);
        return result.ToView(this, viewName: "Fundamental");
    }
}
