using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Messaging;
using AzureTest.Application.Trading.Queries.GetTradingDashboard;
using AzureTest.Application.Trading.Queries.GetTradingHistory;
using AzureTest.Application.Trading.Queries.GetTradingQuotes;
using AzureTest.Contracts.DTOs.Trading;

namespace AzureTest.Application.Trading;

public interface ITradingQueryUnit
{
    Task<Result<TradingDashboardDto>> GetDashboardAsync(GetTradingDashboardQuery query, CancellationToken ct = default);
    Task<Result<IReadOnlyList<TradingQuoteDto>>> GetQuotesAsync(GetTradingQuotesQuery query, CancellationToken ct = default);
    Task<Result<TradingHistoryDto>> GetHistoryAsync(GetTradingHistoryQuery query, CancellationToken ct = default);
}

public sealed class TradingQueryUnit : ITradingQueryUnit
{
    private readonly IQueryHandler<GetTradingDashboardQuery, Result<TradingDashboardDto>> _getDashboard;
    private readonly IQueryHandler<GetTradingQuotesQuery, Result<IReadOnlyList<TradingQuoteDto>>> _getQuotes;
    private readonly IQueryHandler<GetTradingHistoryQuery, Result<TradingHistoryDto>> _getHistory;

    public TradingQueryUnit(
        IQueryHandler<GetTradingDashboardQuery, Result<TradingDashboardDto>> getDashboard,
        IQueryHandler<GetTradingQuotesQuery, Result<IReadOnlyList<TradingQuoteDto>>> getQuotes,
        IQueryHandler<GetTradingHistoryQuery, Result<TradingHistoryDto>> getHistory)
    {
        _getDashboard = getDashboard;
        _getQuotes = getQuotes;
        _getHistory = getHistory;
    }

    public Task<Result<TradingDashboardDto>> GetDashboardAsync(GetTradingDashboardQuery query, CancellationToken ct = default) =>
        _getDashboard.HandleAsync(query, ct);

    public Task<Result<IReadOnlyList<TradingQuoteDto>>> GetQuotesAsync(GetTradingQuotesQuery query, CancellationToken ct = default) =>
        _getQuotes.HandleAsync(query, ct);

    public Task<Result<TradingHistoryDto>> GetHistoryAsync(GetTradingHistoryQuery query, CancellationToken ct = default) =>
        _getHistory.HandleAsync(query, ct);
}
