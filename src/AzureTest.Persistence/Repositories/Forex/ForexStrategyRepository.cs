using AzureTest.Application.Interfaces.Data.Forex;
using AzureTest.Domain.Entities.Forex;
using AzureTest.Persistence.Context;

namespace AzureTest.Persistence.Repositories.Forex;

public class ForexStrategyRepository : GenericRepository<ForexStrategy>, IForexStrategyRepository
{
    public ForexStrategyRepository(AzureTestDbContext dbContext) : base(dbContext)
    {
    }
}
