using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Crypto;
using AzureTest.Application.Crypto.Queries.GetCryptoDerivativesHub;
using AzureTest.Contracts.DTOs.Crypto;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AzureTest.API.Feature.Crypto;

public static class GetCryptoFutures
{
    public static async Task<IResult> HandleAsync(
        [AsParameters] CryptoSymbolRequestDto request,
        [FromServices] ICryptoQueryUnit queries,
        CancellationToken cancellationToken)
    {
        var result = await queries.GetDerivativesHub.HandleAsync(
            new GetCryptoDerivativesHubQuery(request.Symbol), cancellationToken);

        return result.ToEndpointResult();
    }
}
