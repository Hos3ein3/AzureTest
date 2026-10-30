using AzureTest.Application.Interfaces.Data.Trading;
using AzureTest.Domain.Entities.Trading;
using AzureTest.Persistence.Context;

using Microsoft.EntityFrameworkCore;

namespace AzureTest.Persistence.Repositories.Trading;

public class TradingWatchlistRepository : GenericRepository<TradingWatchlist>, ITradingWatchlistRepository
{
    private readonly AzureTestDbContext _dbContext;
    public TradingWatchlistRepository(AzureTestDbContext dbContext) : base(dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<TradingWatchlist?> GetByUserAndSymbolAsync(Guid userId, string symbol, CancellationToken ct)
    =>
        await _dbContext.TradingWatchlists
            .FirstOrDefaultAsync(w => w.UserId == userId && w.Symbol == symbol, ct);
}
