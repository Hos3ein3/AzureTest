using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Crypto;
using AzureTest.Application.Crypto.Queries.GetCryptoDexScreener;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AzureTest.API.Feature.Crypto;

public static class GetCryptoDexScreener
{
    public static async Task<IResult> HandleAsync(
        [FromServices] ICryptoQueryUnit queries,
        CancellationToken cancellationToken)
    {
        var result = await queries.GetDexScreener.HandleAsync(new GetCryptoDexScreenerQuery(), cancellationToken);
        return result.ToEndpointResult();
    }
}
