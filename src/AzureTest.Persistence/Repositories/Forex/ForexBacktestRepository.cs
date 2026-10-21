using AzureTest.Application.Interfaces.Data.Forex;
using AzureTest.Domain.Entities.Forex;
using AzureTest.Persistence.Context;

namespace AzureTest.Persistence.Repositories.Forex;

public class ForexBacktestRepository : GenericRepository<ForexBacktest>, IForexBacktestRepository
{
    public ForexBacktestRepository(AzureTestDbContext dbContext) : base(dbContext)
    {
    }
}
