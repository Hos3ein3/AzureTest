using System;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
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
        return result.ToView(this);
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
            return validation.ToFormResult(this, command, nameof(Index));
        }

        var result = await _commands.SaveJournalEntry.HandleAsync(command, cancellationToken);
        return result.ToFormResult(this, command, nameof(Index));
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
}
