using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Crypto;
using AzureTest.Application.Crypto.Queries.GetCryptoVolumeProfile;
using AzureTest.Contracts.DTOs.Crypto;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AzureTest.API.Feature.Crypto;

public static class GetCryptoVolumeProfile
{
    public static async Task<IResult> HandleAsync(
        [AsParameters] CryptoChartRequestDto request,
        [FromServices] ICryptoQueryUnit queries,
        CancellationToken cancellationToken)
    {
        var result = await queries.GetVolumeProfile.HandleAsync(new GetCryptoVolumeProfileQuery(request.Symbol, request.Timeframe), cancellationToken);
        return result.ToEndpointResult();
    }
}
