using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Crypto;
using AzureTest.Application.Crypto.Commands.ResetCryptoAccount;
using AzureTest.Application.Crypto.Commands.SaveCryptoJournal;
using AzureTest.Application.Crypto.Queries.GetCryptoPortfolio;
using AzureTest.Application.Crypto.Queries.GetCryptoTaxReport;
using AzureTest.Application.Interfaces.Services;
using AzureTest.Contracts.DTOs.Crypto;
using AzureTest.MVC.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AzureTest.MVC.Areas.Crypto.Controllers;

[Area("Crypto")]
[Authorize]
[DisplayName("Crypto Portfolio & Reporting")]
[Description("Portfolio analytics, asset allocation, trading journal with psychological tags, and Dutch Box 3 / MiCA tax reports.")]
public class CryptoPortfolioController : Controller
{
    private readonly ICryptoQueryUnit _queries;
    private readonly ICryptoCommandUnit _commands;
    private readonly IValidationService _validator;
    private readonly ICurrentUser _currentUser;

    public CryptoPortfolioController(
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
    [DisplayName("Portfolio Overview")]
    [Description("Track net asset value, unrealized/realized P&L, open positions, trade history, and portfolio performance.")]
    public async Task<IActionResult> Index(CancellationToken cancellationToken = default)
    {
        var result = await _queries.GetPortfolio.HandleAsync(new GetCryptoPortfolioQuery(_currentUser.Id), cancellationToken);
        return result.ToView(this);
    }

    [HttpGet]
    [DisplayName("Trade Journal")]
    [Description("Review trade execution logs, psychological discipline reflections, strategy adherence, and lessons learned.")]
    public async Task<IActionResult> Journal(CancellationToken cancellationToken = default)
    {
        var result = await _queries.GetPortfolio.HandleAsync(new GetCryptoPortfolioQuery(_currentUser.Id), cancellationToken);
        return result.ToView(this, viewName: "Journal");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [DisplayName("Save Journal Entry")]
    [Description("Record trade reflections, psychological state (FOMO, Greed, Disciplined), and strategic lessons learned.")]
    public async Task<IActionResult> SaveJournal(
        Guid? id,
        Guid? positionId,
        string symbol,
        string strategyTag,
        string emotionTag,
        int rating,
        string notes,
        string lessonsLearned,
        CancellationToken cancellationToken = default)
    {
        var command = new SaveCryptoJournalCommand(
            id,
            positionId,
            _currentUser.Id,
            _currentUser.TenantId,
            symbol,
            strategyTag,
            emotionTag,
            rating,
            notes,
            lessonsLearned);

        var validation = await _validator.ValidateAsync(command, cancellationToken);
        if (validation.IsFailure)
        {
            return validation.ToFormResult(this, command, nameof(Journal));
        }

        var result = await _commands.SaveJournal.HandleAsync(command, cancellationToken);
        if (result.IsFailure)
        {
            TempData["ErrorMessage"] = result.Message ?? result.Error;
        }
        else
        {
            TempData["SuccessMessage"] = "Trade journal entry recorded successfully.";
        }

        return RedirectToAction(nameof(Journal));
    }

    [HttpGet]
    [DisplayName("Tax & Regulatory Report")]
    [Description("Generate Dutch Box 3 asset valuation (pegged to January 1 reference date) and MiCA-compliant trade transaction ledger.")]
    public async Task<IActionResult> TaxReport(int taxYear = 2026, CancellationToken cancellationToken = default)
    {
        var result = await _queries.GetTaxReport.HandleAsync(new GetCryptoTaxReportQuery(_currentUser.Id, taxYear), cancellationToken);
        ViewBag.TaxYear = taxYear;
        return result.ToView(this, viewName: "TaxReport");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [DisplayName("Reset Paper Capital")]
    [Description("Reset paper trading account balance to fresh initial capital ($10,000 USDT) and clean slate.")]
    public async Task<IActionResult> ResetAccount(Guid accountId, decimal initialBalance = 10000m, CancellationToken cancellationToken = default)
    {
        var command = new ResetCryptoAccountCommand(accountId, initialBalance);
        var result = await _commands.ResetAccount.HandleAsync(command, cancellationToken);
        if (result.IsFailure)
        {
            TempData["ErrorMessage"] = result.Message ?? result.Error;
        }
        else
        {
            TempData["SuccessMessage"] = $"Paper account reset successfully to {initialBalance:C0}.";
        }

        return RedirectToAction(nameof(Index));
    }
}
