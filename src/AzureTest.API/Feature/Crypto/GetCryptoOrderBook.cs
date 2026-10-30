using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Crypto;
using AzureTest.Application.Crypto.Queries.GetCryptoOrderBook;
using AzureTest.Contracts.DTOs.Crypto;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AzureTest.API.Feature.Crypto;

public static class GetCryptoOrderBook
{
    public static async Task<IResult> HandleAsync(
        [AsParameters] CryptoSymbolRequestDto request,
        [FromServices] ICryptoQueryUnit queries,
        CancellationToken cancellationToken)
    {
        var result = await queries.GetOrderBook.HandleAsync(
            new GetCryptoOrderBookQuery(request.Symbol, 20), cancellationToken);

        return result.ToEndpointResult();
    }
}
