using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Crypto;
using AzureTest.Application.Crypto.Queries.GetCryptoTaxReport;
using AzureTest.Application.Interfaces.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AzureTest.API.Feature.Crypto;

public static class GetCryptoTaxReport
{
    public static async Task<IResult> HandleAsync(
        [FromQuery] int? taxYear,
        [FromServices] ICryptoQueryUnit queries,
        [FromServices] ICurrentUser currentUser,
        CancellationToken cancellationToken)
    {
        var year = taxYear ?? 2026;
        var result = await queries.GetTaxReport.HandleAsync(new GetCryptoTaxReportQuery(currentUser.Id, year), cancellationToken);
        return result.ToEndpointResult();
    }
}
