using AzureTest.Domain.Entities.Trading;

namespace AzureTest.Application.Interfaces.Data.Trading;

public interface ITradingWatchlistRepository : IGenericRepository<TradingWatchlist>
{
    Task<TradingWatchlist?> GetByUserAndSymbolAsync(Guid userId, string symbol, CancellationToken ct);
}
