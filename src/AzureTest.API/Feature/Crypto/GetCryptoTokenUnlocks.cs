using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Crypto;
using AzureTest.Application.Crypto.Queries.GetCryptoTokenUnlocks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AzureTest.API.Feature.Crypto;

public static class GetCryptoTokenUnlocks
{
    public static async Task<IResult> HandleAsync(
        [FromServices] ICryptoQueryUnit queries,
        CancellationToken cancellationToken)
    {
        var result = await queries.GetTokenUnlocks.HandleAsync(new GetCryptoTokenUnlocksQuery(), cancellationToken);
        return result.ToEndpointResult();
    }
}
