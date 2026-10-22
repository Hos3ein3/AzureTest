using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Forex;
using AzureTest.Application.Forex.Commands.SaveJournalEntry;
using AzureTest.Application.Forex.Queries.GetForexJournal;
using AzureTest.Application.Interfaces.Services;
using AzureTest.Contracts.DTOs.Forex;
using AzureTest.MVC.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AzureTest.MVC.Areas.Forex.Controllers;

[Area("Forex")]
[Authorize]
[DisplayName("Forex Trade Journal")]
[Description("Log trade psychology, track execution discipline, review mistakes, and export tax summaries.")]
public class ForexJournalController : Controller
{
    private readonly IForexQueryUnit _queries;
    private readonly IForexCommandUnit _commands;
    private readonly IValidationService _validator;
    private readonly IBlobService _blobService;

    public ForexJournalController(
        IForexQueryUnit queries,
        IForexCommandUnit commands,
        IValidationService validator,
        IBlobService blobService)
    {
        _queries = queries;
        _commands = commands;
        _validator = validator;
        _blobService = blobService;
    }

    [HttpGet]
    [DisplayName("Journal & Analytics")]
    [Description("Review psychological reflections, setup ratings, emotional state, and annual tax calculations.")]
    public async Task<IActionResult> Index([FromQuery] ForexJournalFilter? filter, CancellationToken cancellationToken)
    {
        filter ??= new ForexJournalFilter();
        var result = await _queries.GetJournal.HandleAsync(new GetForexJournalQuery(filter), cancellationToken);
        ViewBag.Filter = filter;

        if (result.IsFailure)
        {
            var emptyTax = new TaxReportSummaryDto(DateTime.UtcNow.Year, 0m, 0m, 0m, 0m, 0, Array.Empty<TaxTradeRowDto>());
            var emptyModel = (
                Entries: (IReadOnlyList<ForexJournalEntryDto>)Array.Empty<ForexJournalEntryDto>(),
                TaxSummary: emptyTax,
                Analytics: BuildAnalyticsDashboard(Array.Empty<ForexJournalEntryDto>(), emptyTax)
            );
            return ((Result)result).ToView(this, model: emptyModel);
        }

        var analytics = BuildAnalyticsDashboard(result.Value.Entries, result.Value.TaxSummary);
        var viewModel = (
            Entries: result.Value.Entries,
            TaxSummary: result.Value.TaxSummary,
            Analytics: analytics
        );

        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [DisplayName("Save Journal Entry")]
    [Description("Record trade setup, mindset notes, mistakes, lessons learned, and screenshot link.")]
    public async Task<IActionResult> Create(SaveJournalEntryCommand command, Microsoft.AspNetCore.Http.IFormFile? screenshotFile, CancellationToken cancellationToken)
    {
        if (screenshotFile != null && screenshotFile.Length > 0)
        {
            var ext = Path.GetExtension(screenshotFile.FileName);
            var blobName = $"{Guid.NewGuid()}{ext}";
            await using var stream = screenshotFile.OpenReadStream();
            var imageUrl = await _blobService.UploadFileAsync("forex-journal", blobName, stream, screenshotFile.ContentType, cancellationToken);
            command = command with { ScreenshotUrl = imageUrl };
        }

        var validation = await _validator.ValidateAsync(command, cancellationToken);
        if (validation.IsFailure)
        {
            var errorMessages = validation.ValidationErrors.SelectMany(kvp => kvp.Value);
            TempData["ErrorMessage"] = string.Join("; ", errorMessages);
            return RedirectToAction(nameof(Index));
        }

        var result = await _commands.SaveJournalEntry.HandleAsync(command, cancellationToken);
        if (result.IsFailure)
        {
            TempData["ErrorMessage"] = result.Message ?? result.Error ?? "Failed to save journal entry.";
            return RedirectToAction(nameof(Index));
        }

        TempData["SuccessMessage"] = "Journal entry saved successfully.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    [DisplayName("Export Tax Report")]
    [Description("Generate downloadable CSV file of all closed forex trades and realized gain/loss.")]
    public async Task<IActionResult> ExportTax(int? year, CancellationToken cancellationToken)
    {
        var result = await _queries.GetJournal.HandleAsync(new GetForexJournalQuery(new ForexJournalFilter()), cancellationToken);
        if (result.IsFailure) return BadRequest();

        var tax = result.Value.TaxSummary;
        var csv = new StringBuilder();
        csv.AppendLine("OpenDate,CloseDate,Symbol,Direction,LotSize,OpenPrice,ClosePrice,NetProfit,FeesAndSwaps");

        foreach (var row in tax.ClosedTrades)
        {
            csv.AppendLine($"{row.OpenDate:yyyy-MM-dd HH:mm},{row.CloseDate:yyyy-MM-dd HH:mm},{row.Symbol},{row.Direction},{row.LotSize},{row.OpenPrice},{row.ClosePrice},{row.NetProfit},{row.FeesAndSwaps}");
        }

        var bytes = Encoding.UTF8.GetBytes(csv.ToString());
        return File(bytes, "text/csv", $"Forex_Tax_Report_{tax.TaxYear}.csv");
    }

    private static JournalAnalyticsDashboardDto BuildAnalyticsDashboard(
        IReadOnlyList<ForexJournalEntryDto> entries,
        TaxReportSummaryDto taxSummary)
    {
        var trades = taxSummary.ClosedTrades ?? Array.Empty<TaxTradeRowDto>();
        int totalTrades = trades.Count;

        var winningTrades = trades.Where(t => t.NetProfit > 0).ToList();
        var losingTrades = trades.Where(t => t.NetProfit < 0).ToList();

        decimal totalGrossProfits = winningTrades.Sum(t => t.NetProfit);
        decimal totalGrossLosses = losingTrades.Sum(t => Math.Abs(t.NetProfit));
        decimal netProfit = taxSummary.NetRealizedGainLoss;

        decimal winRate = totalTrades > 0 ? Math.Round((decimal)winningTrades.Count / totalTrades * 100m, 1) : 0m;
        decimal profitFactor = totalGrossLosses > 0 ? Math.Round(totalGrossProfits / totalGrossLosses, 2) : (totalGrossProfits > 0 ? 99.99m : 0m);

        decimal avgWin = winningTrades.Count > 0 ? Math.Round(totalGrossProfits / winningTrades.Count, 2) : 0m;
        decimal avgLoss = losingTrades.Count > 0 ? Math.Round(totalGrossLosses / losingTrades.Count, 2) : 0m;
        decimal winLossRatio = avgLoss > 0 ? Math.Round(avgWin / avgLoss, 2) : (avgWin > 0 ? 99.99m : 0m);

        decimal expectancyDollars = totalTrades > 0 ? Math.Round(netProfit / totalTrades, 2) : 0m;
        decimal expectancyPips = 0m;

        decimal sharpeRatio = 0m;
        decimal sortinoRatio = 0m;
        if (totalTrades > 1)
        {
            var mean = trades.Average(t => (double)t.NetProfit);
            var variance = trades.Sum(t => Math.Pow((double)t.NetProfit - mean, 2)) / (totalTrades - 1);
            var stdDev = Math.Sqrt(variance);
            if (stdDev > 0)
            {
                sharpeRatio = Math.Round((decimal)(mean / stdDev * Math.Sqrt(252)), 2);
            }

            var downsideTrades = trades.Where(t => t.NetProfit < 0).ToList();
            if (downsideTrades.Count > 0)
            {
                var downsideVariance = downsideTrades.Sum(t => Math.Pow((double)t.NetProfit, 2)) / (totalTrades - 1);
                var downsideDev = Math.Sqrt(downsideVariance);
                if (downsideDev > 0)
                {
                    sortinoRatio = Math.Round((decimal)(mean / downsideDev * Math.Sqrt(252)), 2);
                }
            }
        }

        decimal maxDrawdownPercent = 0m;
        decimal peak = 0m;
        decimal runningEquity = 0m;
        decimal maxDd = 0m;
        foreach (var trade in trades.OrderBy(t => t.CloseDate))
        {
            runningEquity += trade.NetProfit;
            if (runningEquity > peak) peak = runningEquity;
            var dd = peak - runningEquity;
            if (dd > maxDd) maxDd = dd;
        }
        if (peak > 0)
        {
            maxDrawdownPercent = Math.Round((maxDd / peak) * 100m, 1);
        }

        decimal oneR = avgLoss > 0 ? avgLoss : 100m;

        var rBuckets = new List<RMultipleDistributionBucketDto>();
        var rRanges = new (string Label, Func<decimal, bool> Predicate)[]
        {
            ("< -2R", r => r < -2.0m),
            ("-2R to -1R", r => r >= -2.0m && r < -1.0m),
            ("-1R to 0R", r => r >= -1.0m && r < 0.0m),
            ("0R to +1R", r => r >= 0.0m && r < 1.0m),
            ("+1R to +2R", r => r >= 1.0m && r < 2.0m),
            ("> +2R", r => r >= 2.0m)
        };

        foreach (var (label, predicate) in rRanges)
        {
            var matchedTrades = trades.Where(t => predicate(t.NetProfit / oneR)).ToList();
            int count = matchedTrades.Count;
            decimal pct = totalTrades > 0 ? Math.Round((decimal)count / totalTrades * 100m, 1) : 0m;
            decimal profit = matchedTrades.Sum(t => t.NetProfit);
            rBuckets.Add(new RMultipleDistributionBucketDto(label, count, pct, profit));
        }

        var sessions = new[] { "London", "New York", "Tokyo", "Sydney" };
        var perfBySession = new List<PerformanceByDimensionDto>();
        foreach (var session in sessions)
        {
            var attributedTrades = trades.Where(t =>
            {
                var h = t.OpenDate.Hour;
                return session switch
                {
                    "London" => h >= 8 && h < 16,
                    "New York" => h >= 13 && h < 21,
                    "Tokyo" => h >= 0 && h < 8,
                    "Sydney" => (h >= 21 && h <= 23) || (h >= 0 && h < 5),
                    _ => false
                };
            }).ToList();

            int sCount = attributedTrades.Count;
            int sWins = attributedTrades.Count(t => t.NetProfit > 0);
            decimal sGrossWins = attributedTrades.Where(t => t.NetProfit > 0).Sum(t => t.NetProfit);
            decimal sGrossLoss = attributedTrades.Where(t => t.NetProfit < 0).Sum(t => Math.Abs(t.NetProfit));
            decimal sProfit = attributedTrades.Sum(t => t.NetProfit);
            decimal sWinRate = sCount > 0 ? Math.Round((decimal)sWins / sCount * 100m, 1) : 0m;
            decimal sPf = sGrossLoss > 0 ? Math.Round(sGrossWins / sGrossLoss, 2) : (sGrossWins > 0 ? 99.99m : 0m);

            perfBySession.Add(new PerformanceByDimensionDto("Session", session, sCount, sWinRate, sPf, sProfit, 0m));
        }

        var weekdays = new[] { DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday };
        var perfByWeekday = new List<PerformanceByDimensionDto>();
        foreach (var day in weekdays)
        {
            var dayTrades = trades.Where(t => t.OpenDate.DayOfWeek == day).ToList();
            int dCount = dayTrades.Count;
            int dWins = dayTrades.Count(t => t.NetProfit > 0);
            decimal dGrossWins = dayTrades.Where(t => t.NetProfit > 0).Sum(t => t.NetProfit);
            decimal dGrossLoss = dayTrades.Where(t => t.NetProfit < 0).Sum(t => Math.Abs(t.NetProfit));
            decimal dProfit = dayTrades.Sum(t => t.NetProfit);
            decimal dWinRate = dCount > 0 ? Math.Round((decimal)dWins / dCount * 100m, 1) : 0m;
            decimal dPf = dGrossLoss > 0 ? Math.Round(dGrossWins / dGrossLoss, 2) : (dGrossWins > 0 ? 99.99m : 0m);

            perfByWeekday.Add(new PerformanceByDimensionDto("Weekday", day.ToString(), dCount, dWinRate, dPf, dProfit, 0m));
        }

        var perfByPair = new List<PerformanceByDimensionDto>();
        var symbols = trades.Select(t => t.Symbol).Distinct().DefaultIfEmpty("EUR/USD");
        foreach (var sym in symbols)
        {
            var symTrades = trades.Where(t => string.Equals(t.Symbol, sym, StringComparison.OrdinalIgnoreCase)).ToList();
            int pCount = symTrades.Count;
            int pWins = symTrades.Count(t => t.NetProfit > 0);
            decimal pGrossWins = symTrades.Where(t => t.NetProfit > 0).Sum(t => t.NetProfit);
            decimal pGrossLoss = symTrades.Where(t => t.NetProfit < 0).Sum(t => Math.Abs(t.NetProfit));
            decimal pProfit = symTrades.Sum(t => t.NetProfit);
            decimal pWinRate = pCount > 0 ? Math.Round((decimal)pWins / pCount * 100m, 1) : 0m;
            decimal pPf = pGrossLoss > 0 ? Math.Round(pGrossWins / pGrossLoss, 2) : (pGrossWins > 0 ? 99.99m : 0m);

            perfByPair.Add(new PerformanceByDimensionDto("Pair", sym, pCount, pWinRate, pPf, pProfit, 0m));
        }

        var perfByStrategy = new List<PerformanceByDimensionDto>();
        var strategyNames = entries.Select(e => e.SetupType).Where(s => !string.IsNullOrWhiteSpace(s)).Distinct().ToList();
        if (strategyNames.Count == 0)
        {
            strategyNames = new List<string> { "London Breakout", "Trend Pullback", "Range Bound" };
        }
        foreach (var strat in strategyNames)
        {
            var stratEntries = entries.Where(e => string.Equals(e.SetupType, strat, StringComparison.OrdinalIgnoreCase)).ToList();
            int count = stratEntries.Count;
            int wins = stratEntries.Count(e => e.FollowedPlan && e.ExecutionRating >= 4);
            decimal stratWinRate = count > 0 ? Math.Round((decimal)wins / count * 100m, 1) : 0m;

            perfByStrategy.Add(new PerformanceByDimensionDto("Strategy", strat, count, stratWinRate, count > 0 ? 1.5m : 0m, 0m, 0m));
        }

        return new JournalAnalyticsDashboardDto(
            TotalNetProfit: netProfit,
            ProfitFactor: profitFactor,
            WinRatePercent: winRate,
            ExpectancyPips: expectancyPips,
            ExpectancyDollars: expectancyDollars,
            SharpeRatio: sharpeRatio,
            SortinoRatio: sortinoRatio,
            MaxDrawdownPercent: maxDrawdownPercent,
            AverageWinAmount: avgWin,
            AverageLossAmount: avgLoss,
            WinLossRatio: winLossRatio,
            RMultipleDistribution: rBuckets,
            PerformanceBySession: perfBySession,
            PerformanceByWeekday: perfByWeekday,
            PerformanceByPair: perfByPair,
            PerformanceByStrategy: perfByStrategy
        );
    }
}
