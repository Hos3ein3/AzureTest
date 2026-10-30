using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Crypto;
using AzureTest.Application.Crypto.Queries.GetCryptoTaxReport;
using AzureTest.Application.Interfaces.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AzureTest.API.Feature.Crypto;

public static class ExportCryptoTaxCsv
{
    public static async Task<IResult> HandleAsync(
        [FromQuery] int? taxYear,
        [FromServices] ICryptoQueryUnit queries,
        [FromServices] ICurrentUser currentUser,
        CancellationToken cancellationToken)
    {
        var year = taxYear ?? 2026;
        var result = await queries.GetTaxReport.HandleAsync(new GetCryptoTaxReportQuery(currentUser.Id, year), cancellationToken);
        if (result.IsFailure)
        {
            return result.ToEndpointResult();
        }

        var report = result.Value;
        var csv = new StringBuilder();
        csv.AppendLine("Transaction Date (UTC),Asset,Action,Units,Unit Price (EUR),Gross Total (EUR),Realized PnL (EUR),Reference Peildatum Jan 1,Dutch Box 3 Relevance,Regulatory Framework");

        foreach (var item in report.Items)
        {
            csv.AppendLine($"\"{item.Date:yyyy-MM-dd HH:mm:ss}\",\"{item.Symbol}\",\"{item.Action}\",{item.Quantity},{item.PriceEur:F4},{item.TotalValueEur:F2},{item.RealizedGainLossEur:F2},\"2026-01-01 00:00 CET\",\"Box 3 Overige Bezittingen\",\"EU MiCA / Dutch AFM\"");
        }

        csv.AppendLine();
        csv.AppendLine($"# DUTCH BOX 3 TAX SUMMARY (PEILDATUM 1 JANUARI {year})");
        csv.AppendLine($"# Total Jan 1st Valuation: EUR {report.EstimatedBox3ValuationJan1stEur:F2}");
        csv.AppendLine($"# Tax-Free Allowance (Heffingvrij vermogen): EUR {report.TaxFreeAllowanceEur:F2}");
        csv.AppendLine($"# Taxable Box 3 Wealth: EUR {report.TaxableBox3WealthEur:F2}");
        csv.AppendLine($"# Deemed Yield Rate (Forfaitair rendement): {report.DeemedYieldPercent}%");
        csv.AppendLine($"# Box 3 Wealth Tax Rate: {report.Box3TaxRatePercent}%");
        csv.AppendLine($"# Estimated Box 3 Tax Payable: EUR {report.EstimatedBox3TaxPayableEur:F2}");

        var bytes = Encoding.UTF8.GetBytes(csv.ToString());
        return Results.File(bytes, "text/csv", $"Dutch_Box3_Crypto_Tax_{year}_{DateTime.UtcNow:yyyyMMdd}.csv");
    }
}
