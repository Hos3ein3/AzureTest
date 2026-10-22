using System;
using System.ComponentModel;
using System.Text;
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

    [HttpGet]
    [DisplayName("Export Dutch Box 3 Tax CSV")]
    [Description("Export Dutch Belastingdienst and MiCA audit-ready crypto tax report to RFC 4180 CSV.")]
    public async Task<IActionResult> ExportTaxCsv(int taxYear = 2026, CancellationToken cancellationToken = default)
    {
        var result = await _queries.GetTaxReport.HandleAsync(new GetCryptoTaxReportQuery(_currentUser.Id, taxYear), cancellationToken);
        if (result.IsFailure)
        {
            TempData["ErrorMessage"] = result.Message ?? result.Error;
            return RedirectToAction(nameof(TaxReport), new { taxYear });
        }

        var report = result.Value;
        var csv = new StringBuilder();
        csv.AppendLine("Transaction Date (UTC),Asset,Action,Units,Unit Price (EUR),Gross Total (EUR),Realized PnL (EUR),Reference Peildatum Jan 1,Dutch Box 3 Relevance,Regulatory Framework");

        foreach (var item in report.Items)
        {
            csv.AppendLine($"\"{item.Date:yyyy-MM-dd HH:mm:ss}\",\"{item.Symbol}\",\"{item.Action}\",{item.Quantity},{item.PriceEur:F4},{item.TotalValueEur:F2},{item.RealizedGainLossEur:F2},\"2026-01-01 00:00 CET\",\"Box 3 Overige Bezittingen\",\"EU MiCA / Dutch AFM\"");
        }

        csv.AppendLine();
        csv.AppendLine($"# DUTCH BOX 3 TAX SUMMARY (PEILDATUM 1 JANUARI {taxYear})");
        csv.AppendLine($"# Total Jan 1st Valuation: EUR {report.EstimatedBox3ValuationJan1stEur:F2}");
        csv.AppendLine($"# Tax-Free Allowance (Heffingvrij vermogen): EUR {report.TaxFreeAllowanceEur:F2}");
        csv.AppendLine($"# Taxable Box 3 Wealth: EUR {report.TaxableBox3WealthEur:F2}");
        csv.AppendLine($"# Deemed Yield Rate (Forfaitair rendement): {report.DeemedYieldPercent}%");
        csv.AppendLine($"# Box 3 Wealth Tax Rate: {report.Box3TaxRatePercent}%");
        csv.AppendLine($"# Estimated Box 3 Tax Payable: EUR {report.EstimatedBox3TaxPayableEur:F2}");

        var bytes = Encoding.UTF8.GetBytes(csv.ToString());
        return File(bytes, "text/csv", $"Dutch_Box3_Crypto_Tax_{taxYear}_{DateTime.UtcNow:yyyyMMdd}.csv");
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
