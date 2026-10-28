using System.Collections.Generic;

namespace AzureTest.Contracts.DTOs.Trading;

public sealed record TradingDashboardDto(
    TradingAccountDto Account,
    IReadOnlyList<TradingPositionDto> OpenPositions,
    IReadOnlyList<TradingPositionDto> ClosedPositions,
    IReadOnlyList<TradingOrderDto> PendingOrders,
    IReadOnlyList<TradingWatchlistDto> Watchlist,
    IReadOnlyList<TradingQuoteDto> Quotes,
    TradingQuoteDto SelectedQuote
);
