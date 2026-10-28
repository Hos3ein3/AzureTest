using AzureTest.Application.Interfaces.Data.Trading;
using AzureTest.Domain.Entities.Trading;
using AzureTest.Persistence.Context;

namespace AzureTest.Persistence.Repositories.Trading;

public class TradingPositionRepository : GenericRepository<TradingPosition>, ITradingPositionRepository
{
    public TradingPositionRepository(AzureTestDbContext dbContext) : base(dbContext)
    {
    }
}
