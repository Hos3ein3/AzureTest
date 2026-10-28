using AzureTest.Application.Interfaces.Data.Trading;
using AzureTest.Domain.Entities.Trading;
using AzureTest.Persistence.Context;

namespace AzureTest.Persistence.Repositories.Trading;

public class TradingAccountRepository : GenericRepository<TradingAccount>, ITradingAccountRepository
{
    public TradingAccountRepository(AzureTestDbContext dbContext) : base(dbContext)
    {
    }
}
