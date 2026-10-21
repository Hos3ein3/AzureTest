using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Forex;
using AzureTest.Application.Forex.Queries.GetForexRiskAssessment;
using AzureTest.Application.Forex.Services;
using AzureTest.Application.Interfaces.Services;
using AzureTest.Contracts.DTOs.Forex;
using AzureTest.MVC.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AzureTest.MVC.Areas.Forex.Controllers;

[Area("Forex")]
[Authorize]
[DisplayName("Forex Risk Management")]
[Description("Position sizing, drawdown limits, news no-trade filter, and ESMA/AFM regulatory compliance.")]
public class ForexRiskController : Controller
{
    private readonly IForexQueryUnit _queries;
    private readonly IForexCommandUnit _commands;
    private readonly IForexRiskService _riskService;
    private readonly IValidationService _validator;

    public ForexRiskController(
        IForexQueryUnit queries,
        IForexCommandUnit commands,
        IForexRiskService riskService,
        IValidationService validator)
    {
        _queries = queries;
        _commands = commands;
        _riskService = riskService;
        _validator = validator;
    }

    [HttpGet]
    [DisplayName("Risk Management Hub")]
    [Description("Review account risk limits, currency exposure distribution, news trade restrictions, and ESMA/AFM status.")]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var result = await _queries.GetRiskAssessment.HandleAsync(new GetForexRiskAssessmentQuery(), cancellationToken);
        return result.ToView(this);
    }

    [HttpGet]
    [DisplayName("Lot Size Calculator")]
    [Description("Calculate exact lot size from equity, stop loss pips, and risk percentage under ESMA leverage rules.")]
    public IActionResult Calculate(
        decimal balance = 10_000m,
        decimal riskPercent = 1.0m,
        decimal stopLossPips = 25m,
        string symbol = "EUR/USD",
        decimal leverage = 30m)
    {
        var request = new LotSizeCalculationRequestDto(balance, riskPercent, stopLossPips, symbol, leverage);
        var result = _riskService.CalculateLotSize(request);
        return Json(result);
    }
}
