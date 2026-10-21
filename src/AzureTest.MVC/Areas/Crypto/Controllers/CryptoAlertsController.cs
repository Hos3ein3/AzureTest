using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Crypto;
using AzureTest.Application.Crypto.Commands.CreateCryptoAlert;
using AzureTest.Application.Crypto.Commands.DeleteCryptoAlert;
using AzureTest.Application.Crypto.Queries.GetCryptoAlerts;
using AzureTest.Application.Interfaces.Services;
using AzureTest.Contracts.DTOs.Crypto;
using AzureTest.Domain.Entities.Crypto.Enums;
using AzureTest.MVC.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AzureTest.MVC.Areas.Crypto.Controllers;

[Area("Crypto")]
[Authorize]
[DisplayName("Crypto Alerts & Notifications")]
[Description("Manage automated notifications for price breaches, indicator thresholds, and algorithmic trading signals.")]
public class CryptoAlertsController : Controller
{
    private readonly ICryptoQueryUnit _queries;
    private readonly ICryptoCommandUnit _commands;
    private readonly IValidationService _validator;
    private readonly ICurrentUser _currentUser;

    public CryptoAlertsController(
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
    [DisplayName("Alerts List")]
    [Description("View active and triggered price, indicator, and signal alerts.")]
    public async Task<IActionResult> Index(CancellationToken cancellationToken = default)
    {
        var result = await _queries.GetAlerts.HandleAsync(new GetCryptoAlertsQuery(_currentUser.Id), cancellationToken);
        return result.ToView(this);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [DisplayName("Create Alert")]
    [Description("Create a new alert based on price targets, RSI/MACD triggers, or breakout signals.")]
    public async Task<IActionResult> Create(
        string symbol,
        CryptoAlertType alertType,
        decimal targetValue,
        string? message,
        string? channel,
        CancellationToken cancellationToken = default)
    {
        var command = new CreateCryptoAlertCommand(
            _currentUser.Id,
            _currentUser.TenantId,
            symbol,
            alertType,
            targetValue,
            message,
            channel ?? "InApp");

        var validation = await _validator.ValidateAsync(command, cancellationToken);
        if (validation.IsFailure)
        {
            return validation.ToFormResult(this, command, nameof(Index));
        }

        var result = await _commands.CreateAlert.HandleAsync(command, cancellationToken);
        if (result.IsFailure)
        {
            TempData["ErrorMessage"] = result.Message ?? result.Error;
        }
        else
        {
            TempData["SuccessMessage"] = $"Alert created for {symbol} ({alertType} at {targetValue}).";
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [DisplayName("Delete Alert")]
    [Description("Delete an existing price or indicator alert.")]
    public async Task<IActionResult> Delete(Guid alertId, CancellationToken cancellationToken = default)
    {
        var command = new DeleteCryptoAlertCommand(alertId);
        var result = await _commands.DeleteAlert.HandleAsync(command, cancellationToken);
        if (result.IsFailure)
        {
            TempData["ErrorMessage"] = result.Message ?? result.Error;
        }
        else
        {
            TempData["SuccessMessage"] = "Alert removed successfully.";
        }

        return RedirectToAction(nameof(Index));
    }
}
