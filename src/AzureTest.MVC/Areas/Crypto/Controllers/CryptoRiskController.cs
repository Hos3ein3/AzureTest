using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Crypto;
using AzureTest.Application.Crypto.Commands.TriggerKillSwitch;
using AzureTest.Application.Crypto.Queries.GetCryptoRiskOverview;
using AzureTest.Application.Interfaces.Services;
using AzureTest.MVC.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AzureTest.MVC.Areas.Crypto.Controllers;

[Area("Crypto")]
[Authorize]
[DisplayName("Crypto Risk Management")]
[Description("Account capital preservation, position sizing calculator, daily loss caps, leverage guardrails, and emergency kill switch.")]
public class CryptoRiskController : Controller
{
    private readonly ICryptoQueryUnit _queries;
    private readonly ICryptoCommandUnit _commands;
    private readonly IValidationService _validator;
    private readonly ICurrentUser _currentUser;

    public CryptoRiskController(
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
    [DisplayName("Risk Dashboard")]
    [Description("Monitor real-time margin usage, daily drawdown limit, position exposure, AFM compliance alerts, and position sizing tools.")]
    public async Task<IActionResult> Index(CancellationToken cancellationToken = default)
    {
        var result = await _queries.GetRiskOverview.HandleAsync(new GetCryptoRiskOverviewQuery(_currentUser.Id), cancellationToken);
        return result.ToView(this);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [DisplayName("Emergency Kill Switch")]
    [Description("Immediately cancel all working orders and market-close all open cryptocurrency positions to prevent further drawdown.")]
    public async Task<IActionResult> KillSwitch(Guid accountId, string reason = "User initiated emergency halt", CancellationToken cancellationToken = default)
    {
        var command = new TriggerKillSwitchCommand(accountId, reason);
        var validation = await _validator.ValidateAsync(command, cancellationToken);
        if (validation.IsFailure)
        {
            return validation.ToFormResult(this, command, nameof(Index));
        }

        var result = await _commands.TriggerKillSwitch.HandleAsync(command, cancellationToken);
        if (result.IsFailure)
        {
            TempData["ErrorMessage"] = result.Message ?? result.Error;
        }
        else
        {
            TempData["SuccessMessage"] = $"KILL SWITCH ACTIVATED: {result.Value} positions closed and all pending orders cancelled.";
        }

        return RedirectToAction(nameof(Index));
    }
}
