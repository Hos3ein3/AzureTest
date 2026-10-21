using AzureTest.Application.Interfaces.Data.Forex;
using AzureTest.Domain.Entities.Forex;
using AzureTest.Persistence.Context;

namespace AzureTest.Persistence.Repositories.Forex;

public class ForexOrderRepository : GenericRepository<ForexOrder>, IForexOrderRepository
{
    public ForexOrderRepository(AzureTestDbContext dbContext) : base(dbContext)
    {
    }
}
