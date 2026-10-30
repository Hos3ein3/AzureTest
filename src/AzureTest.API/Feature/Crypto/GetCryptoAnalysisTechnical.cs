using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Crypto;
using AzureTest.Application.Crypto.Queries.GetCryptoAnalysis;
using AzureTest.Contracts.DTOs.Crypto;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AzureTest.API.Feature.Crypto;

public static class GetCryptoAnalysisTechnical
{
    public static async Task<IResult> HandleAsync(
        [AsParameters] CryptoChartRequestDto request,
        [FromServices] ICryptoQueryUnit queries,
        CancellationToken cancellationToken)
    {
        var result = await queries.GetAnalysis.HandleAsync(new GetCryptoAnalysisQuery(request.Symbol, request.Timeframe), cancellationToken);
        return result.ToEndpointResult();
    }
}
