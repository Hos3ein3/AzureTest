using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Forex;
using AzureTest.Application.Forex.Queries.GetForexJournal;
using AzureTest.Contracts.DTOs.Forex;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AzureTest.API.Feature.Forex;

public static class ExportForexTax
{
    public static async Task<IResult> HandleAsync(
        [FromQuery] int? year,
        [FromServices] IForexQueryUnit queries,
        CancellationToken cancellationToken)
    {
        var result = await queries.GetJournal.HandleAsync(new GetForexJournalQuery(new ForexJournalFilter()), cancellationToken);
        if (result.IsFailure)
        {
            return result.ToEndpointResult();
        }

        var tax = result.Value.TaxSummary;
        var csv = new StringBuilder();
        csv.AppendLine("OpenDate,CloseDate,Symbol,Direction,LotSize,OpenPrice,ClosePrice,NetProfit,FeesAndSwaps");

        foreach (var row in tax.ClosedTrades)
        {
            csv.AppendLine($"{row.OpenDate:yyyy-MM-dd HH:mm},{row.CloseDate:yyyy-MM-dd HH:mm},{row.Symbol},{row.Direction},{row.LotSize},{row.OpenPrice},{row.ClosePrice},{row.NetProfit},{row.FeesAndSwaps}");
        }

        var bytes = Encoding.UTF8.GetBytes(csv.ToString());
        return Results.File(bytes, "text/csv", $"Forex_Tax_Report_{tax.TaxYear}.csv");
    }
}
