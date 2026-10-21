using AzureTest.Application.Interfaces.Data.Forex;
using AzureTest.Domain.Entities.Forex;
using AzureTest.Persistence.Context;

namespace AzureTest.Persistence.Repositories.Forex;

public class ForexTradeRepository : GenericRepository<ForexTrade>, IForexTradeRepository
{
    public ForexTradeRepository(AzureTestDbContext dbContext) : base(dbContext)
    {
    }
}
