using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Forex;
using AzureTest.Application.Forex.Queries.GetForexJournal;
using AzureTest.Contracts.DTOs.Forex;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AzureTest.API.Feature.Forex;

public static class GetForexJournal
{
    public static async Task<IResult> HandleAsync(
        [AsParameters] ForexJournalFilter filter,
        [FromServices] IForexQueryUnit queries,
        CancellationToken cancellationToken)
    {
        var result = await queries.GetJournal.HandleAsync(new GetForexJournalQuery(filter), cancellationToken);
        return result.ToEndpointResult();
    }
}
