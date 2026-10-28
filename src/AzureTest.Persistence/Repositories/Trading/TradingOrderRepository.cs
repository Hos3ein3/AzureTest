using AzureTest.Application.Interfaces.Data.Trading;
using AzureTest.Domain.Entities.Trading;
using AzureTest.Persistence.Context;

namespace AzureTest.Persistence.Repositories.Trading;

public class TradingOrderRepository : GenericRepository<TradingOrder>, ITradingOrderRepository
{
    public TradingOrderRepository(AzureTestDbContext dbContext) : base(dbContext)
    {
    }
}
