using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Contracts.DTOs.Crypto;

namespace AzureTest.Application.Crypto.Services;

public interface ICryptoArbitrageService
{
    Task<Result<IReadOnlyList<CryptoCrossExchangeArbitrageOpportunityDto>>> GetCrossExchangeOpportunitiesAsync(CancellationToken ct = default);
    Task<Result<IReadOnlyList<CryptoTriangularArbitrageDto>>> GetTriangularArbitrageOpportunitiesAsync(CancellationToken ct = default);
    Task<Result<IReadOnlyList<CryptoFundingArbitrageDto>>> GetFundingRateArbitrageOpportunitiesAsync(CancellationToken ct = default);
    Task<Result<CryptoArbitrageHubDto>> GetArbitrageHubAsync(CancellationToken ct = default);
}
