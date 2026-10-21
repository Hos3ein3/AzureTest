using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Contracts.DTOs.Forex;
using AzureTest.Domain.Entities.Forex.Enums;

namespace AzureTest.Application.Forex.Services;

public interface IForexMarketDataService
{
    Task<MarketOverviewDto> GetMarketOverviewAsync(ForexMarketFilter filter, CancellationToken cancellationToken = default);
    Task<ForexQuoteDto> GetQuoteAsync(string symbol, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ForexCandleDto>> GetCandlesAsync(string symbol, ForexTimeframe timeframe, int count = 100, CancellationToken cancellationToken = default);
    Task<ForexTickDto> GetLiveTickAsync(string symbol, CancellationToken cancellationToken = default);
    IReadOnlyList<ForexSessionDto> GetMarketSessions();
    IReadOnlyList<ContextMarketQuoteDto> GetContextMarkets();
    IReadOnlyList<SessionSpreadLiquidityDto> GetSessionSpreads();
    IReadOnlyList<BrokerSpreadComparisonDto> GetBrokerComparisons(string? symbol = null);
    IReadOnlyList<HourlyVolatilityDto> GetHourlyVolatilityHeatmap(string? symbol = null);
}
