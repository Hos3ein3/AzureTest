using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Forex;
using AzureTest.Application.Forex.Commands.UpdateApiSettings;
using AzureTest.Application.Forex.Queries.GetForexSettings;
using AzureTest.Application.Interfaces.Services;
using AzureTest.Contracts.DTOs.Forex;
using AzureTest.MVC.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AzureTest.MVC.Areas.Forex.Controllers;

[Area("Forex")]
[Authorize]
[DisplayName("Forex Settings & Disclaimers")]
[Description("Manage API keys for data sources, configure execution simulator, and review legal risk disclosures.")]
public class ForexSettingsController : Controller
{
    private readonly IForexQueryUnit _queries;
    private readonly IForexCommandUnit _commands;
    private readonly IValidationService _validator;

    public ForexSettingsController(
        IForexQueryUnit queries,
        IForexCommandUnit commands,
        IValidationService validator)
    {
        _queries = queries;
        _commands = commands;
        _validator = validator;
    }

    [HttpGet]
    [DisplayName("Settings & Disclaimers")]
    [Description("Configure API keys for Twelve Data, Alpha Vantage, FRED, Finnhub, and read risk warnings.")]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var result = await _queries.GetSettings.HandleAsync(new GetForexSettingsQuery(), cancellationToken);
        return result.ToView(this);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [DisplayName("Save API Settings")]
    [Description("Update API credentials and simulator latency preferences.")]
    public async Task<IActionResult> Update(UpdateApiSettingsCommand command, CancellationToken cancellationToken)
    {
        var validation = await _validator.ValidateAsync(command, cancellationToken);
        if (validation.IsFailure)
        {
            return validation.ToFormResult(this, command, nameof(Index));
        }

        var result = await _commands.UpdateApiSettings.HandleAsync(command, cancellationToken);
        return result.ToFormResult(this, command, nameof(Index));
    }
}
