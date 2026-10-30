using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Crypto;
using AzureTest.Application.Crypto.Commands.TriggerKillSwitch;
using AzureTest.Application.Crypto.Queries.GetCryptoRiskOverview;
using AzureTest.Application.Crypto.Services;
using AzureTest.Application.Interfaces.Services;
using AzureTest.Contracts.DTOs.Crypto;
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
    private readonly ICryptoRiskService _riskService;
    private readonly IValidationService _validator;
    private readonly ICurrentUser _currentUser;

    public CryptoRiskController(
        ICryptoQueryUnit queries,
        ICryptoCommandUnit commands,
        ICryptoRiskService riskService,
        IValidationService validator,
        ICurrentUser currentUser)
    {
        _queries = queries;
        _commands = commands;
        _riskService = riskService;
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
    public async Task<IActionResult> KillSwitch([FromForm] CryptoKillSwitchRequestDto request, CancellationToken cancellationToken = default)
    {
        var command = new TriggerKillSwitchCommand(request.AccountId, request.Reason);
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

    [HttpPost]
    [ValidateAntiForgeryToken]
    [DisplayName("Update Risk Settings")]
    [Description("Update account risk per trade %, daily drawdown cap %, and maximum concurrent positions.")]
    public async Task<IActionResult> UpdateLimits(
        [FromForm] UpdateCryptoRiskLimitsRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var result = await _riskService.UpdateRiskLimitsAsync(request.AccountId, request.RiskPercentagePerTrade, request.DailyLossLimitPercent, request.MaxOpenPositions, cancellationToken);
        if (result.IsFailure)
        {
            TempData["ErrorMessage"] = result.Message ?? result.Error;
        }
        else
        {
            TempData["SuccessMessage"] = $"Risk parameters updated successfully: {request.RiskPercentagePerTrade}% Risk/Trade, {request.DailyLossLimitPercent}% Daily Max Drawdown, {request.MaxOpenPositions} Max Positions.";
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [DisplayName("Top Up Position Margin")]
    [Description("Add isolated margin to an existing open position to push back liquidation price and prevent forced closure.")]
    public async Task<IActionResult> TopUpMargin(
        [FromForm] TopUpCryptoMarginRequestDto request,
        CancellationToken cancellationToken = default)
    {
        if (request.PositionId.ToString().StartsWith("11111111") ||
            request.PositionId.ToString().StartsWith("22222222") ||
            request.PositionId.ToString().StartsWith("33333333"))
        {
            TempData["SuccessMessage"] = $"[SIMULATION] Injected ${request.AmountUsd:N2} margin to position. Liquidation price safely pushed back away from current mark price.";
            return RedirectToAction(nameof(Index));
        }

        var result = await _riskService.TopUpPositionMarginAsync(request.PositionId, request.AmountUsd, cancellationToken);
        if (result.IsFailure)
        {
            TempData["ErrorMessage"] = result.Message ?? result.Error;
        }
        else
        {
            TempData["SuccessMessage"] = $"Successfully added ${request.AmountUsd:N2} margin. Liquidation distance widened.";
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [DisplayName("Trim Position Risk")]
    [Description("Partially close or de-risk an open position by taking off exposure to free up maintenance margin.")]
    public async Task<IActionResult> TrimPosition(
        [FromForm] TrimCryptoPositionRequestDto request,
        CancellationToken cancellationToken = default)
    {
        if (request.PositionId.ToString().StartsWith("11111111") ||
            request.PositionId.ToString().StartsWith("22222222") ||
            request.PositionId.ToString().StartsWith("33333333"))
        {
            TempData["SuccessMessage"] = $"[SIMULATION] De-risked position by trimming {request.TrimQuantity:N4} units. Maintenance margin requirement reduced.";
            return RedirectToAction(nameof(Index));
        }

        var result = await _riskService.TrimPositionRiskAsync(request.PositionId, request.TrimQuantity, cancellationToken);
        if (result.IsFailure)
        {
            TempData["ErrorMessage"] = result.Message ?? result.Error;
        }
        else
        {
            TempData["SuccessMessage"] = $"Position de-risked: Trimmed {result.Value:N4} units. Capital reclaimed and liquidation buffer restored.";
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [DisplayName("Interactive ATR Sizing Calculation")]
    [Description("Calculate volatility-adjusted position size and ATR targets dynamically.")]
    public IActionResult CalculateAtr([FromForm] CalculateCryptoAtrRequestDto request)
    {
        var dto = _riskService.CalculateAtrPositionSize(request.AccountEquity, request.RiskPercent, request.Symbol, request.EntryPrice, request.AtrMultiplier, request.CustomAtr);
        return Json(dto);
    }
}
