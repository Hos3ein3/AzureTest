using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Contracts.DTOs.Crypto;
using AzureTest.Domain.Entities.Crypto.Enums;

namespace AzureTest.Application.Crypto.Services;

public interface ICryptoMarketDataService
{
    Task<CryptoMarketOverviewDto> GetMarketOverviewAsync(CryptoMarketFilter filter, CancellationToken ct = default);
    Task<IReadOnlyList<CryptoCandleDto>> GetCandlesAsync(string symbol, CryptoTimeframe timeframe, int limit = 120, CancellationToken ct = default);
    Task<CryptoOrderBookDto> GetOrderBookAsync(string symbol, int limit = 20, CancellationToken ct = default);
    Task<IReadOnlyList<CryptoRecentTradeDto>> GetRecentTradesAsync(string symbol, int limit = 20, CancellationToken ct = default);
    Task<CryptoFuturesFundingDto> GetFuturesFundingAsync(string symbol, CancellationToken ct = default);
    Task<CryptoSentimentDto> GetSentimentAsync(CancellationToken ct = default);
    Task<IReadOnlyList<CryptoWhaleAlertDto>> GetWhaleAlertsAsync(CancellationToken ct = default);
    Task<decimal> GetCurrentPriceAsync(string symbol, CancellationToken ct = default);
    Task<CryptoMarketDataHubDto> GetMarketDataHubAsync(string symbol, CryptoTimeframe timeframe, CancellationToken ct = default);
    Task<IReadOnlyList<CryptoFootprintBarDto>> GetFootprintBarsAsync(string symbol, CryptoTimeframe timeframe, int limit = 30, CancellationToken ct = default);
    Task<CryptoOrderBookHeatmapDto> GetOrderBookHeatmapAsync(string symbol, CancellationToken ct = default);
    Task<CryptoCrossExchangePremiumDto> GetCrossExchangePremiumsAsync(string symbol, CancellationToken ct = default);
    Task<IReadOnlyList<CryptoStablecoinDto>> GetStablecoinDepegMonitorAsync(CancellationToken ct = default);
    Task<CryptoDerivativesHubDto> GetDerivativesHubAsync(string symbol, CancellationToken ct = default);
    Task<CryptoOnChainHubDto> GetOnChainHubAsync(CancellationToken ct = default);
}


