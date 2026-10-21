using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Contracts.DTOs.Crypto;
using AzureTest.Domain.Entities.Crypto.Enums;

namespace AzureTest.Application.Crypto.Services;

public interface ICryptoAnalysisService
{
    CryptoTechnicalIndicatorsDto CalculateIndicators(string symbol, CryptoTimeframe timeframe, IReadOnlyList<CryptoCandleDto> candles);
    CryptoSupportResistanceDto CalculateSupportResistance(IReadOnlyList<CryptoCandleDto> candles, decimal currentPrice);
    Task<CryptoAnalysisDto> GetComprehensiveAnalysisAsync(string symbol, CryptoTimeframe timeframe, CancellationToken ct = default);
    Task<IReadOnlyList<CryptoScreenerItemDto>> RunScreenerAsync(CancellationToken ct = default);
}
