using AzureTest.Application.Interfaces.Data.Trading;
using AzureTest.Domain.Entities.Trading;
using AzureTest.Persistence.Context;

namespace AzureTest.Persistence.Repositories.Trading;

public class TradingWatchlistRepository : GenericRepository<TradingWatchlist>, ITradingWatchlistRepository
{
    public TradingWatchlistRepository(AzureTestDbContext dbContext) : base(dbContext)
    {
    }
}
