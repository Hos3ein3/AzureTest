using AzureTest.Application.Interfaces.Data.Forex;
using AzureTest.Domain.Entities.Forex;
using AzureTest.Persistence.Context;

namespace AzureTest.Persistence.Repositories.Forex;

public class ForexAccountRepository : GenericRepository<ForexAccount>, IForexAccountRepository
{
    public ForexAccountRepository(AzureTestDbContext dbContext) : base(dbContext)
    {
    }
}
