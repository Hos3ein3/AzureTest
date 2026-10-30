using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Crypto;
using AzureTest.Application.Crypto.Queries.GetCryptoStrategies;
using AzureTest.Application.Interfaces.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AzureTest.API.Feature.Crypto;

public static class GetCryptoStrategies
{
    public static async Task<IResult> HandleAsync(
        [FromServices] ICryptoQueryUnit queries,
        [FromServices] ICurrentUser currentUser,
        CancellationToken cancellationToken)
    {
        var result = await queries.GetStrategies.HandleAsync(new GetCryptoStrategiesQuery(currentUser.Id), cancellationToken);
        return result.ToEndpointResult();
    }
}
